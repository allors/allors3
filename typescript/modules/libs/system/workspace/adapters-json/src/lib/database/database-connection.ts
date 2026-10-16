import {
  DiscoverySuccess,
  PushReadChanged,
  PushReadRequest,
  ReadScope,
  ReadFailure,
  InvokeReadRequest,
  ReadResponse,
} from '@allors/system/common/protocol-json';
import { Configuration, Operations } from '@allors/system/workspace/domain';
import {
  Class,
  OperandType,
  RoleType,
  MethodType,
} from '@allors/system/workspace/meta';
import { DatabaseConnection as SystemDatabaseConnection } from '@allors/system/workspace/adapters';
import { Workspace } from '../workspace/workspace';
import { ReadCoordinator } from '../read/read-coordinator';
import { ReadRequest } from '@allors/system/common/protocol-json';
import { CompletedRead } from '../read/completed-read';
import {
  ReadStoredRecord,
  ReadStoredPermission,
  ReadStoreScope,
  ReadSecurityKey,
} from '../read/read-store-values';
import { ReadMetadata } from '../read/read-metadata';
import { Session } from '../session/session';
import { ScopedDatabaseRecord } from '../read/scoped-database-record';
import { ReadStore } from '../read/read-store';

/** Each supplied mutation method must make one attempt without hidden retries. */
export interface DatabaseConnectionTransport {
  read(request: ReadRequest, cancellation?: AbortSignal): Promise<ReadResponse>;
  invokeRead(
    request: InvokeReadRequest,
    cancellation?: AbortSignal
  ): Promise<unknown>;
  pushRead(
    request: PushReadRequest,
    cancellation?: AbortSignal
  ): Promise<unknown>;
}

export interface DatabaseConnectionOptions {
  readonly revision: string;
  readonly context?: string | null;
  readonly maxHints?: number;
  readonly maxRequestBytes?: number;
  readonly maxResponseBytes?: number;
}

/** Internal operation capability, held only by Sessions created by the owner. */
export interface SessionOwner {
  unregister(): void;
  run<T>(assertSession: () => void, operation: () => Promise<T>): Promise<T>;
  read(
    request: ReadRequest,
    cancellation?: AbortSignal
  ): Promise<CompletedRead>;
  checkMessageSize(json: string, kind: 'request' | 'response'): void;
  observeFailure(failure: ReadFailure): void;
  validateDiscovery(discovery: DiscoverySuccess): void;
  validateChanged(changed: PushReadChanged): void;
  assertNewIdAvailable(id: number): void;
  pushReadScope(): ReadScope;
  pushReadOnce(
    request: PushReadRequest,
    cancellation?: AbortSignal
  ): Promise<unknown>;
  acceptChanged(changed: PushReadChanged): CompletedRead;
  complete(
    request: ReadRequest,
    discovery: DiscoverySuccess,
    cancellation?: AbortSignal
  ): Promise<CompletedRead>;
  adaptRecord(record: ReadStoredRecord): ScopedDatabaseRecord;
  assertMutation(ids: readonly number[]): void;
  invokeReadScope(): ReadScope;
  invokeReadOnce(
    request: InvokeReadRequest,
    cancellation?: AbortSignal
  ): Promise<unknown>;
}

const sessionRegistrations = new WeakMap<
  DatabaseConnection,
  (session: Session) => SessionOwner
>();

/** Internal constructor bridge; deliberately absent from the public barrel. */
export function registerSession(
  connection: DatabaseConnection,
  session: Session
): SessionOwner {
  const register = sessionRegistrations.get(connection);
  if (!register) throw new Error('Session requires a normal ready connection.');
  return register(session);
}

/** One ready connection owns all of its workspace Sessions; the transport is borrowed. */
export class DatabaseConnection extends SystemDatabaseConnection {
  #state: 'initializing' | 'ready' | 'closing' | 'closed' = 'initializing';
  #sessions = new Set<Session>();
  #closing?: Promise<void>;
  readonly context: string | null;
  #coordinator: ReadCoordinator;
  #store?: ReadStore;
  #pushRead?: DatabaseConnectionTransport['pushRead'];
  #invokeRead?: DatabaseConnectionTransport['invokeRead'];
  #queue: Promise<unknown> = Promise.resolve();
  #metadata: ReadMetadata;
  #classes = new Map<string, Class>();
  #roleTags = new Map<RoleType, string>();

  private constructor(
    configuration: Configuration,
    transport: DatabaseConnectionTransport,
    options: DatabaseConnectionOptions
  ) {
    const captured = {
      ...configuration,
      rules: configuration.rules ? [...configuration.rules] : undefined,
    };
    if (captured.rules) Object.freeze(captured.rules);
    super(Object.freeze(captured));
    const scope = { ...options, workspace: captured.name };
    sessionRegistrations.set(this, (session) => this.#registerSession(session));
    this.context = scope.context ?? null;
    this.#metadata = new ReadMetadata(configuration.metaPopulation);
    const store = () => this.#store;
    this.#coordinator = new ReadCoordinator(
      transport,
      {
        ...scope,
        metaPopulation: configuration.metaPopulation,
      },
      {
        get store() {
          return store();
        },
        bind: (store) => {
          this.assertReadActive();
          this.#store = store;
        },
        assertActive: () => this.assertReadActive(),
        invalidate: () => {
          void this.close().catch(() => undefined);
        },
      }
    );
    this.#pushRead = transport.pushRead.bind(transport);
    this.#invokeRead = transport.invokeRead.bind(transport);
    for (const cls of configuration.metaPopulation.classes) {
      this.#classes.set(cls.tag, cls);
      for (const role of cls.databaseOriginRoleTypes)
        this.#roleTags.set(role, role.relationType.tag);
    }
  }

  static async create(
    configuration: Configuration,
    transport: DatabaseConnectionTransport,
    options: DatabaseConnectionOptions,
    cancellation?: AbortSignal
  ): Promise<DatabaseConnection> {
    for (const capability of ['read', 'pushRead', 'invokeRead'] as const) {
      if (typeof transport?.[capability] !== 'function')
        throw new Error(
          `DatabaseConnection requires a ${capability} transport function before initialization.`
        );
    }
    if (cancellation?.aborted)
      throw new Error('Connection initialization cancelled.');
    const connection = new DatabaseConnection(
      configuration,
      transport,
      options
    );
    try {
      const initialization = connection.#coordinator.read(
        {
          v: 1,
          i: 'connection-bootstrap',
          k: 'read',
          x: connection.context,
          p: { l: [], d: [] },
        },
        { cancellation }
      );
      connection.#queue = initialization.then(
        () => undefined,
        () => undefined
      );
      await initialization;
      if (cancellation?.aborted)
        throw new Error('Connection initialization cancelled.');
      if (connection.#state !== 'initializing')
        throw new Error(
          'Connection closed during initialization. Create a replacement connection.'
        );
      connection.#state = 'ready';
      return connection;
    } catch (error) {
      await connection.close();
      throw error;
    }
  }

  override createWorkspace(): Workspace {
    this.assertActive();
    return new Workspace(this);
  }

  /** Internal registration; ordinary Session constructors never accept a caller-owned capability. */
  #registerSession(session: Session): SessionOwner {
    this.assertActive();
    if (
      !(session instanceof Session) ||
      session.workspace.database !== this ||
      this.#sessions.has(session)
    )
      throw new Error(
        'Register each Session once with its own ready connection.'
      );
    this.#sessions.add(session);
    return {
      unregister: () => this.#sessions.delete(session),
      run: (assertSession, operation) =>
        this.run(() => {
          if (!this.#sessions.has(session))
            throw new Error(
              'Session closed. Create a new Session before starting an operation.'
            );
          session.assertActive();
          assertSession();
        }, operation),
      read: this.read.bind(this),
      pushReadScope: this.pushReadScope.bind(this),
      checkMessageSize: this.#coordinator.checkMessageSize.bind(
        this.#coordinator
      ),
      observeFailure: this.#coordinator.observeFailure.bind(this.#coordinator),
      validateDiscovery: this.validateDiscovery.bind(this),
      validateChanged: this.validateChanged.bind(this),
      assertNewIdAvailable: this.#coordinator.assertNewIdAvailable.bind(
        this.#coordinator
      ),
      pushReadOnce: this.pushReadOnce.bind(this),
      acceptChanged: this.#coordinator.acceptChanged.bind(this.#coordinator),
      complete: (request, discovery, cancellation) =>
        this.#coordinator.complete(request, discovery, { cancellation }),
      adaptRecord: this.adaptRecord.bind(this),
      assertMutation: this.assertMutation.bind(this),
      invokeReadScope: this.invokeReadScope.bind(this),
      invokeReadOnce: this.invokeReadOnce.bind(this),
    };
  }

  get closed(): boolean {
    return this.#state === 'closing' || this.#state === 'closed';
  }

  close(): Promise<void> {
    if (!this.#closing) {
      this.#state = 'closing';
      this.#closing = this.drain();
    }
    return this.#closing;
  }

  private async drain(): Promise<void> {
    await this.#queue;
    let failure: unknown;
    let hasFailure = false;
    for (const session of this.#sessions) {
      try {
        session.close();
      } catch (error) {
        if (!hasFailure) {
          failure = error;
          hasFailure = true;
        }
      }
    }
    this.#sessions.clear();
    this.#store?.clear();
    this.#state = 'closed';
    if (hasFailure) throw failure;
  }

  assertActive(context = this.context): void {
    if (this.#state !== 'ready')
      throw new Error(
        'Connection is not ready or is closed/retired. Create a new ready connection before active use.'
      );
    if (context !== this.context)
      throw new Error(
        'Session context differs from the configured scope. Create a replacement connection.'
      );
  }

  private assertReadActive(): void {
    if (this.closed)
      throw new Error(
        'Connection closed or retired. Create a new ready connection before reading.'
      );
  }

  override getRecord(): undefined {
    return undefined;
  }
  override getPermission(): undefined {
    return undefined;
  }

  /** Internal Session admission holds the connection gate through synchronous application. */
  private run<T>(
    assertSession: () => void,
    operation: () => Promise<T>
  ): Promise<T> {
    assertSession();
    const operationResult = this.#queue.then(() => {
      assertSession();
      return operation();
    });
    this.#queue = operationResult.then(
      () => undefined,
      () => undefined
    );
    return operationResult;
  }

  private read(
    request: ReadRequest,
    cancellation?: AbortSignal
  ): Promise<CompletedRead> {
    return this.#coordinator.read(request, { cancellation });
  }

  private pushReadScope(): ReadScope {
    this.assertMutation([]);
    if (!this.#pushRead)
      throw new Error(
        'Protected Push requires a transport with pushRead support. Configure the protected transport before saving.'
      );
    const scope = this.#store.scope;
    return {
      p: scope.population,
      u: scope.userId,
      w: scope.workspace,
      r: scope.revision,
      x: scope.context,
    };
  }

  private pushReadOnce(
    request: PushReadRequest,
    cancellation?: AbortSignal
  ): Promise<unknown> {
    return this.#pushRead(request, cancellation);
  }

  // Receipt/read classification must remain available after local lifetime admission ends.
  private validateDiscovery(discovery: DiscoverySuccess): void {
    for (const record of discovery.p) this.#metadata.requireClass(record.c);
  }

  private validateChanged(changed: PushReadChanged): void {
    const sc = changed.sc;
    const scope = new ReadStoreScope(sc.p, sc.u, sc.w, sc.r, sc.x ?? null);
    const keys = changed.s.map(
      (entry) => new ReadSecurityKey(entry.k, entry.i, entry.v)
    );
    const records = changed.p.map(
      (entry) =>
        new ReadStoredRecord(
          scope,
          entry.i,
          entry.c,
          entry.v,
          entry.g.map((index) => keys[index]),
          entry.r.map((index) => keys[index]),
          this.#metadata.decodeRoles(entry.c, entry.p.ro)
        )
    );
    const pool = new Map(records.map((record) => [record.id, record]));
    for (const record of records) this.#metadata.validateRecord(record, pool);
    for (const permission of changed.pm)
      this.#metadata.validatePermission(
        new ReadStoredPermission(
          scope,
          permission.i,
          permission.p.c,
          permission.p.o,
          permission.p.t ?? null
        )
      );
  }

  private adaptRecord(record: ReadStoredRecord): ScopedDatabaseRecord {
    const cls = this.#classes.get(record.classTag);
    if (!cls)
      throw new Error(
        'Read class is absent from captured metadata. Regenerate the workspace and create a new connection.'
      );
    return new ScopedDatabaseRecord(cls, record, this.#roleTags);
  }

  private assertMutation(ids: readonly number[]): void {
    this.assertActive();
    if (!this.#store)
      throw new Error('Create a ready connection before Push or Invoke.');
    for (const id of ids) {
      if (id > 0 && this.#store.isUnavailable(id))
        throw new Error(
          `Object ${id} is unavailable. Pull it successfully before Push or Invoke.`
        );
    }
  }

  private invokeReadScope(): ReadScope {
    this.assertMutation([]);
    if (!this.#invokeRead)
      throw new Error(
        'Protected Invoke requires a transport with invokeRead support. Configure the protected transport before invoking.'
      );
    const scope = this.#store.scope;
    return {
      p: scope.population,
      u: scope.userId,
      w: scope.workspace,
      r: scope.revision,
      x: scope.context,
    };
  }
  private invokeReadOnce(
    request: InvokeReadRequest,
    cancellation?: AbortSignal
  ): Promise<unknown> {
    return this.#invokeRead(request, cancellation);
  }

  /** undefined preserves initial/new-object rules; committed records always require membership. */
  permission(
    id: number,
    cls: Class,
    operand: OperandType,
    operation: Operations,
    committed: boolean
  ): boolean | undefined {
    this.assertActive();
    if (id <= 0) return undefined;
    const store = this.#store;
    if (store?.isUnavailable(id)) return false;
    if (!committed) return undefined;
    const record = store?.getCurrentRecord(id);
    if (!record || record.classTag !== cls.tag) return false;
    const tag =
      this.#roleTags.get(operand as RoleType) ?? (operand as MethodType).tag;
    const applicable = (permissionId: number) => {
      const descriptor = store.getCurrentPermission(permissionId);
      return (
        descriptor?.classTag === cls.tag &&
        descriptor.operation === operation &&
        descriptor.operandTag === tag
      );
    };
    for (const key of record.revocations) {
      const body = store.getCurrentSecurity(key.kind, key.id);
      if (
        !body ||
        body.permissionIds.some((id) => !store.getCurrentPermission(id)) ||
        body.permissionIds.some(applicable)
      )
        return false;
    }
    let granted = false;
    for (const key of record.grants) {
      const body = store.getCurrentSecurity(key.kind, key.id);
      if (
        !body ||
        body.permissionIds.some((id) => !store.getCurrentPermission(id))
      )
        return false;
      granted ||= body.permissionIds.some(applicable);
    }
    return granted;
  }
}
