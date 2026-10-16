import {
  decodeReadRequest,
  decodePushReadReceipt,
  decodePushReadRequest,
  decodePushReadPart,
  DiscoverySuccess,
  PushReadRequest,
  PushReadChanged,
  ReadRequest,
  InvokeReadRequest,
  decodeInvokeReadRequest,
  decodeInvokeReadReceipt,
  decodeInvokeReadResponse,
  validateReadRequest,
} from '@allors/system/common/protocol-json';
import {
  IChangeSet,
  IInvokeResult,
  InvokeOptions,
  IObject,
  IPushResult,
  PushOptions,
  MutationApplicationError,
  MutationRefresh,
  PushDiscovery,
  MutationAcknowledgementStatus,
  MutationRefreshApplicationStatus,
  IRule,
  Method,
  Operations,
  Pull,
} from '@allors/system/workspace/domain';
import {
  Class,
  MethodType,
  RelationType,
  RoleType,
} from '@allors/system/workspace/meta';
import {
  Strategy as SystemStrategy,
  IRange,
  ChangeSet,
  SessionOriginState,
} from '@allors/system/workspace/adapters';
import { Session as SystemSession } from '@allors/system/workspace/adapters';
import { DatabaseOriginState } from '../session/originstate/database-origin-state';
import { Workspace } from '../workspace/workspace';
import { dependenciesToJson } from '../json/to-json';
import {
  DatabaseConnection,
  SessionOwner,
  registerSession,
} from '../database/database-connection';
import { ScopedDatabaseRecord } from '../read/scoped-database-record';
import { PullResult } from '../read/scoped-pull-result';
import { ReadFailedError } from '../read/read-failed-error';
import { CompletedRead } from '../read/completed-read';
import {
  ScopedPushResult,
  PendingPull,
  PushResultState,
} from '../read/scoped-push-result';
import {
  ScopedInvokeResult,
  MutationResultState,
  mutationDiagnostics,
  mutationDiagnostic,
  readDiagnostics,
} from '../read/scoped-mutation-result';
import {
  captureScopedMessage,
  captureScopedPull,
} from '../read/scoped-to-json';

class ScopedOriginState extends DatabaseOriginState {
  protected override assertStrategy(object: IObject): void {
    if (object == null)
      throw new Error(
        'Object is not present in this scoped Session. Request the target through an include, dependency, or Pull before navigating the relation.'
      );
  }
  constructor(
    object: IObject,
    record: ScopedDatabaseRecord,
    private owner: Session
  ) {
    super(object, record);
  }
  protected override get compareExactRecords(): boolean {
    return true;
  }
  override canRead(role: RoleType): boolean {
    this.owner.assertActive();
    return (
      this.owner.connection.permission(
        this.object.id,
        this.object.strategy.cls,
        role,
        Operations.Read,
        !!this.databaseRecord
      ) ?? super.canRead(role)
    );
  }
  override canWrite(role: RoleType): boolean {
    this.owner.assertActive();
    const permitted = this.owner.connection.permission(
      this.object.id,
      this.object.strategy.cls,
      role,
      Operations.Write,
      !!this.databaseRecord
    );
    return this.isPushed ? false : permitted ?? super.canWrite(role);
  }
  override canExecute(method: MethodType): boolean {
    this.owner.assertActive();
    return (
      this.owner.connection.permission(
        this.object.id,
        this.object.strategy.cls,
        method,
        Operations.Execute,
        !!this.databaseRecord
      ) ?? super.canExecute(method)
    );
  }
  override setUnitRole(
    role: RoleType,
    value: Parameters<DatabaseOriginState['setUnitRole']>[1]
  ) {
    this.owner.assertEditable();
    super.setUnitRole(role, value);
  }
  override setCompositeRole(role: RoleType, value?: IObject) {
    this.owner.assertEditable();
    super.setCompositeRole(role, value);
  }
  override setCompositesRole(role: RoleType, value: IRange<IObject>) {
    this.owner.assertEditable();
    super.setCompositesRole(role, value);
  }
  override addCompositesRole(role: RoleType, value: IObject) {
    this.owner.assertEditable();
    super.addCompositesRole(role, value);
  }
  override removeCompositesRole(role: RoleType, value: IObject) {
    this.owner.assertEditable();
    super.removeCompositesRole(role, value);
  }
  override reset() {
    this.owner.assertEditable();
    super.reset();
  }
  override restoreRole(role: RoleType) {
    this.owner.assertEditable();
    super.restoreRole(role);
  }
  override checkpoint(changeSet: ChangeSet): void {
    this.owner.assertEditable();
    super.checkpoint(changeSet);
  }
}

class ScopedStrategy extends SystemStrategy {
  constructor(
    public override session: Session,
    cls: Class,
    id: number,
    record?: ScopedDatabaseRecord
  ) {
    super(session, cls, id);
    this.DatabaseOriginState = new ScopedOriginState(
      this.object,
      record,
      session
    );
  }
  override canRead(role: RoleType): boolean {
    this.session.assertActive();
    return super.canRead(role);
  }
  override canWrite(role: RoleType): boolean {
    this.session.assertActive();
    return super.canWrite(role);
  }
  override canExecute(method: MethodType): boolean {
    this.session.assertActive();
    return super.canExecute(method);
  }
  override setUnitRole(
    role: RoleType,
    value: Parameters<SystemStrategy['setUnitRole']>[1]
  ) {
    this.session.assertEditable();
    super.setUnitRole(role, value);
  }
  override setCompositeRole(role: RoleType, value: IObject) {
    this.session.assertEditable();
    super.setCompositeRole(role, value);
  }
  override setCompositesRole(role: RoleType, value: IObject[]) {
    this.session.assertEditable();
    super.setCompositesRole(role, value);
  }
  override addCompositesRole(role: RoleType, value: IObject) {
    this.session.assertEditable();
    super.addCompositesRole(role, value);
  }
  override removeCompositesRole(role: RoleType, value: IObject) {
    this.session.assertEditable();
    super.removeCompositesRole(role, value);
  }
  override delete(): void {
    this.session.assertEditable();
    super.delete();
  }
  override reset(): void {
    this.session.assertEditable();
    super.reset();
  }
  override restoreRole(role: RoleType) {
    this.session.assertEditable();
    return super.restoreRole(role);
  }
}

class ProtectedSessionOriginState extends SessionOriginState {
  constructor(private readonly owner: Session) {
    super(owner.ranges);
  }
  override checkpoint(...args: Parameters<SessionOriginState['checkpoint']>) {
    this.owner.assertEditable();
    return super.checkpoint(...args);
  }
  override setUnitRole(...args: Parameters<SessionOriginState['setUnitRole']>) {
    this.owner.assertEditable();
    return super.setUnitRole(...args);
  }
  override setCompositeRole(
    ...args: Parameters<SessionOriginState['setCompositeRole']>
  ) {
    this.owner.assertEditable();
    return super.setCompositeRole(...args);
  }
  override setCompositesRole(
    ...args: Parameters<SessionOriginState['setCompositesRole']>
  ) {
    this.owner.assertEditable();
    return super.setCompositesRole(...args);
  }
  override addCompositesRole(
    ...args: Parameters<SessionOriginState['addCompositesRole']>
  ) {
    this.owner.assertEditable();
    return super.addCompositesRole(...args);
  }
  override removeCompositesRole(
    ...args: Parameters<SessionOriginState['removeCompositesRole']>
  ) {
    this.owner.assertEditable();
    return super.removeCompositesRole(...args);
  }
}

/** Explicit deterministic lifetime for a Session attached to one fixed connection scope. */
export class Session extends SystemSession {
  #closed = false;
  #saving = false;
  #operations = 0;
  #reconciliationRequired = false;
  #faulted = false;
  #requestId = 0;
  #entries = new Map<
    number,
    {
      object: IObject;
      strategy: ScopedStrategy;
      state: DatabaseOriginState;
      cls: Class;
    }
  >();

  readonly connection: DatabaseConnection;
  readonly database: DatabaseConnection;
  readonly #owner: SessionOwner;

  constructor(workspace: Workspace) {
    const connection = workspace.database as DatabaseConnection;
    connection.assertActive();
    super(workspace);
    this.connection = this.database = connection;
    this.context = connection.context;
    this.sessionOriginState = new ProtectedSessionOriginState(this);
    this.#owner = registerSession(connection, this);
  }

  get closed(): boolean {
    return this.#closed;
  }
  assertActive(): void {
    if (this.#closed)
      throw new Error(
        'Scoped Session closed. Create a new Session for active operations.'
      );
    if (this.#faulted)
      throw new Error(
        'Scoped Session adaptation failed. Create a new Session; existing values and diffs remain inspectable.'
      );
    this.connection.assertActive(this.context);
  }

  get saving(): boolean {
    return this.#saving;
  }
  override get reconciliationRequired(): boolean {
    return this.#reconciliationRequired;
  }

  assertEditable(): void {
    this.assertAvailable();
    if (this.#reconciliationRequired)
      throw new Error(
        'This Session requires reconciliation before further mutation. Inspect the outcome and use a replacement Session; do not replay automatically.'
      );
  }

  private assertAvailable(): void {
    this.assertActive();
    if (this.#saving)
      throw new Error(
        'A save or invocation is in progress. Wait for its result to apply before editing or starting another Session operation.'
      );
  }

  private async sessionOperation<T>(
    mutation: boolean,
    operation: () => Promise<T>
  ): Promise<T> {
    if (mutation) this.assertEditable();
    else this.assertAvailable();
    ++this.#operations;
    try {
      return await operation();
    } finally {
      --this.#operations;
    }
  }

  close(): void {
    if (this.#closed) return;
    this.#closed = true;
    this.#owner.unregister();
  }

  override create<T extends IObject>(cls: Class): T {
    this.assertEditable();
    const strategy = new ScopedStrategy(this, cls, this.database.nextId());
    const entry = {
      object: strategy.object,
      strategy,
      state: strategy.DatabaseOriginState as DatabaseOriginState,
      cls,
    };
    this.validateEntry(entry, strategy.id);
    this.assertEditable();
    this.addObjectWithIdentity(entry.object, strategy.id, cls);
    this.#entries.set(strategy.id, entry);
    this.pushToDatabaseTracker.onCreated(entry.object);
    this.changeSetTracker.onCreated(entry.object);
    return entry.object as T;
  }

  override instantiateDatabaseStrategy(): IObject {
    return null;
  }

  override activate(rules: IRule<IObject>[]): void {
    this.assertEditable();
    super.activate(rules);
  }

  override onDelete(strategy: ScopedStrategy): void {
    this.assertEditable();
    this.#entries.delete(strategy.id);
    this.removeObject(strategy.object);
    this.pushToDatabaseTracker.onDelete(strategy.object);
    this.changeSetTracker.onDelete(strategy.object);
  }

  override checkpoint(): IChangeSet {
    this.assertEditable();
    return super.checkpoint();
  }

  override pull(
    input: Pull | Pull[],
    cancellation?: AbortSignal
  ): Promise<PullResult> {
    return this.sessionOperation(false, () =>
      this.pullCore(input, cancellation)
    );
  }

  private async pullCore(
    input: Pull | Pull[],
    cancellation?: AbortSignal
  ): Promise<PullResult> {
    this.assertActive();
    const request = captureScopedMessage({
      v: 1,
      i: `session-${++this.#requestId}`,
      k: 'read',
      x: this.context,
      p: {
        l: (Array.isArray(input) ? input : [input]).map(captureScopedPull),
        d: dependenciesToJson(this.dependencies) ?? [],
      },
    });
    validateReadRequest(request);
    const captured = decodeReadRequest(JSON.stringify(request));
    return this.#owner.run(
      () => this.assertActive(),
      async () => {
        let completed;
        try {
          completed = await this.#owner.read(captured, cancellation);
        } catch (error) {
          try {
            this.assertActive();
          } catch (retired) {
            Object.defineProperty(retired, 'cause', { value: error });
            throw retired;
          }
          if (error instanceof ReadFailedError)
            return new PullResult(this, captured.i, undefined, error);
          throw error;
        }
        return this.applyRead(completed, captured.i);
      }
    );
  }

  private applyRead(completed: CompletedRead, requestId: string): PullResult {
    let applying = false;
    try {
      this.assertActive();
      const result = new PullResult(this, requestId, completed);
      const staged = completed.records.map((stored) => {
        const record = this.#owner.adaptRecord(stored);
        const previous = this.#entries.get(record.id);
        if (previous && previous.cls !== record.cls)
          throw new Error(
            'A refreshed object cannot change its class. Obtain a consistent read before applying its baseline.'
          );
        const strategy =
          previous?.strategy ??
          new ScopedStrategy(this, record.cls, record.id, record);
        const entry = previous ?? {
          object: strategy.object,
          strategy,
          state: strategy.DatabaseOriginState as DatabaseOriginState,
          cls: record.cls,
        };
        this.validateEntry(entry, record.id);
        const accept = !previous || entry.state.canAcceptRecord(record);
        return { record, entry, accept, fresh: !previous };
      });
      // Public tracker collections may be host-supplied subclasses. Enumerate them
      // before admission and apply through our own concrete Set without callbacks.
      const changedStates = new Set(this.changeSetTracker.databaseOriginStates);
      for (const { entry } of staged) changedStates.add(entry.state);
      this.assertActive();
      // No host callback or await from this point through complete Session application.
      applying = true;
      for (const { record, entry, accept, fresh } of staged) {
        if (fresh) {
          this.addObjectWithIdentity(entry.object, record.id, entry.cls);
          this.#entries.set(record.id, entry);
        } else if (accept) entry.state.acceptRecord(record);
        else result.addMergeError(entry.object);
      }
      this.changeSetTracker.databaseOriginStates = changedStates;
      // Materialize output membership while this operation still owns the connection gate.
      void result.collections;
      void result.objects;
      return result;
    } catch (error) {
      if (applying) {
        this.#faulted = true;
      }
      throw error;
    }
  }

  private beginMutation(): void {
    this.assertEditable();
    if (this.#operations)
      throw new Error(
        'An earlier Session operation is still in progress. Wait for it before Push or Invoke.'
      );
    this.#saving = true;
  }

  override async push(
    options: PushOptions = {},
    cancellation?: AbortSignal
  ): Promise<IPushResult> {
    this.beginMutation();
    try {
      const scope = this.#owner.pushReadScope();
      const discover = options?.discover;
      const request: PushReadRequest = {
        v: 1,
        i: `session-${++this.#requestId}`,
        sc: scope,
        x: this.context,
        push: {
          n:
            this.pushToDatabaseTracker.created == null
              ? undefined
              : [...this.pushToDatabaseTracker.created].map((object) =>
                  (
                    (object.strategy as SystemStrategy)
                      .DatabaseOriginState as DatabaseOriginState
                  ).pushNew()
                ),
          o:
            this.pushToDatabaseTracker.changed == null
              ? undefined
              : [...this.pushToDatabaseTracker.changed].map((state) =>
                  (state as DatabaseOriginState).pushExisting()
                ),
        },
        returnChanged: options?.refresh ?? true,
        discover:
          discover == null
            ? undefined
            : {
                l: (Array.isArray(discover) ? discover : [discover]).map(
                  captureScopedPull
                ),
                d: dependenciesToJson(this.dependencies) ?? [],
              },
      };
      const json = JSON.stringify(captureScopedMessage(request));
      this.#owner.checkMessageSize(json, 'request');
      const captured = decodePushReadRequest(json);
      return await this.#owner.run(
        () => this.assertActive(),
        async () => {
          this.#owner.assertMutation(
            captured.push.o?.map((object) => object.d) ?? []
          );
          checkSaveCancellation(cancellation);
          let raw: string;
          let receipt: ReturnType<typeof decodePushReadReceipt>;
          let outcome: PushResultState['outcome'] = 'unknown';
          let diagnostics = mutationDiagnostics({}, 'unknown');
          let creationMappings: PushResultState['creationMappings'] = [];
          let refresh: MutationRefresh = {
            status: captured.returnChanged ? 'unavailable' : 'not-requested',
            errors: [],
          };
          let discovery: PushDiscovery = {
            status: captured.discover ? 'unavailable' : 'not-requested',
            errors: [],
          };
          let acknowledgement: MutationAcknowledgementStatus = 'not-applicable';
          let refreshApplication: MutationRefreshApplicationStatus =
            'not-applicable';
          const applicationErrors: MutationApplicationError[] = [];
          let pendingQuery: PendingPull;
          const result = () =>
            new ScopedPushResult(
              {
                outcome,
                diagnostics,
                creationMappings,
                refresh,
                discovery,
                application: {
                  acknowledgement,
                  refresh: refreshApplication,
                  errors: applicationErrors,
                },
                pendingQuery,
                reconciliationRequired: this.#reconciliationRequired,
              },
              (id) => this.objectByWorkspaceId.get(id),
              (tag) => {
                const meta =
                  this.workspace.configuration.metaPopulation.metaObjectByTag.get(
                    tag
                  ) as RelationType;
                return meta?.kind === 'RelationType' ? meta : undefined;
              }
            );
          const applicationError = (
            stage: MutationApplicationError['stage'],
            cause: unknown
          ) => {
            const error = asError(cause);
            applicationErrors.push({
              stage,
              message: error.message,
              cause: error,
            });
          };
          try {
            const received = await this.#owner.pushReadOnce(
              decodePushReadRequest(JSON.stringify(captured)),
              cancellation
            );
            raw =
              typeof received === 'string'
                ? received
                : JSON.stringify(captureScopedMessage(received));
            this.#owner.checkMessageSize(raw, 'response');
            receipt = decodePushReadReceipt(raw, captured);
          } catch (cause) {
            this.#reconciliationRequired = true;
            diagnostics = mutationDiagnostics({}, 'unknown', [
              mutationDiagnostic('push-outcome-unknown', cause),
            ]);
            return result();
          }
          outcome = receipt.push.k;
          diagnostics = mutationDiagnostics(receipt.push, outcome);
          if (receipt.push.k !== 'committed') {
            this.#reconciliationRequired ||= receipt.push.k === 'unknown';
            return result();
          }
          creationMappings = receipt.push.n.map((mapping) => ({
            workspaceId: mapping.w,
            databaseId: mapping.d,
          }));
          try {
            this.assertActive();
            const remaps = receipt.push.n.map((mapping) => {
              this.#owner.assertNewIdAvailable(mapping.d);
              const entry = this.#entries.get(mapping.w);
              if (!entry || this.objectByWorkspaceId.has(mapping.d))
                throw new Error(
                  'Assigned Push object ID collides or its local object is absent. Reconcile before further mutation.'
                );
              this.validateEntry(entry, mapping.w);
              return { mapping, entry };
            });
            const pushed = (captured.push.o ?? []).map((input) => {
              const entry = this.#entries.get(input.d);
              if (!entry)
                throw new Error(
                  'Submitted Push object is no longer present. Reconcile before further mutation.'
                );
              this.validateEntry(entry, input.d);
              return entry.state;
            });
            this.assertActive();
            // Every mapping was validated before changing any identity. These concrete
            // operations invoke no object factory or editing callbacks.
            for (const { mapping, entry } of remaps) {
              entry.strategy.onDatabasePushNewId(mapping.d);
              this.#entries.delete(mapping.w);
              this.objectByWorkspaceId.delete(mapping.w);
              this.#entries.set(mapping.d, entry);
              this.addObjectWithIdentity(entry.object, mapping.d, entry.cls);
              entry.state.onPushed();
            }
            this.pushToDatabaseTracker.created = null;
            this.pushToDatabaseTracker.changed = null;
            for (const state of pushed) state.onPushed();
            acknowledgement = 'applied';
          } catch (cause) {
            this.#reconciliationRequired = true;
            acknowledgement = 'failed';
            applicationError('acknowledgement', cause);
          }
          // Classify each independent read even when local acknowledgement/lifetime has failed.
          let changed: PushReadChanged;
          let headers: DiscoverySuccess;
          if (captured.returnChanged) {
            try {
              const response = decodePushReadPart(raw, captured, 'changed');
              if (response.changed.k === 'error') {
                this.#owner.observeFailure(response.changed);
                throw new ReadFailedError(response.changed);
              }
              this.#owner.validateChanged(response.changed);
              for (const record of response.changed.p) {
                const entry = this.#entries.get(record.i);
                if (entry && entry.cls.tag !== record.c)
                  throw new Error(
                    'A refreshed object cannot change its class. Obtain a consistent read before applying its baseline.'
                  );
              }
              changed = response.changed;
              refresh = { status: 'succeeded', errors: [] };
            } catch (cause) {
              refresh = {
                status: 'failed',
                errors:
                  cause instanceof ReadFailedError
                    ? readDiagnostics(cause.errors, cause)
                    : [mutationDiagnostic('push-refresh-failed', cause)],
              };
            }
          }
          if (captured.discover) {
            try {
              const response = decodePushReadPart(raw, captured, 'discovery');
              if (response.discovery.k === 'error') {
                this.#owner.observeFailure(response.discovery);
                throw new ReadFailedError(response.discovery);
              }
              this.#owner.validateDiscovery(response.discovery);
              headers = response.discovery;
              discovery = { status: 'succeeded', errors: [] };
            } catch (cause) {
              discovery = {
                status: 'failed',
                errors:
                  cause instanceof ReadFailedError
                    ? readDiagnostics(cause.errors, cause)
                    : [mutationDiagnostic('push-discovery-failed', cause)],
              };
            }
          }
          if (changed) {
            refreshApplication = 'skipped';
            if (acknowledgement === 'applied') {
              let applying = false;
              try {
                checkSaveCancellation(cancellation);
                this.assertActive();
                applying = true;
                const completed = this.#owner.acceptChanged(changed);
                const applied = this.applyRead(completed, captured.i);
                refresh = { ...refresh, result: applied };
                refreshApplication = applied.mergeErrors?.length
                  ? 'conflicted'
                  : 'applied';
              } catch (cause) {
                refreshApplication = applying ? 'failed' : 'skipped';
                this.#reconciliationRequired ||= this.#faulted;
                applicationError('refresh', cause);
              }
            }
          }
          if (headers) {
            try {
              if (acknowledgement !== 'applied')
                throw new Error(
                  'Pending query unavailable because the save acknowledgement could not be applied. Reconcile and use a fresh Session.'
                );
              checkSaveCancellation(cancellation);
              this.assertActive();
              const readRequest: ReadRequest = {
                v: 1,
                i: captured.i,
                sc: captured.sc,
                x: captured.x,
                k: 'read',
                p: captured.discover,
              };
              pendingQuery = new PendingPull((signal) =>
                this.completePending(readRequest, headers, signal)
              );
            } catch (cause) {
              applicationError('discovery', cause);
            }
          }
          return result();
        }
      );
    } finally {
      this.#saving = false;
    }
  }

  private completePending(
    request: ReadRequest,
    discovery: DiscoverySuccess,
    cancellation?: AbortSignal
  ): Promise<PullResult> {
    return this.sessionOperation(false, () =>
      this.#owner.run(
        () => this.assertActive(),
        async () => {
          try {
            return this.applyRead(
              await this.#owner.complete(request, discovery, cancellation),
              request.i
            );
          } catch (error) {
            this.assertActive();
            if (error instanceof ReadFailedError)
              return new PullResult(this, request.i, undefined, error);
            throw error;
          }
        }
      )
    );
  }

  override async invoke(
    input: Method | Method[],
    options?: InvokeOptions,
    cancellation?: AbortSignal
  ): Promise<IInvokeResult> {
    this.beginMutation();
    try {
      const scope = this.#owner.invokeReadScope();
      const capturedOptions =
        options == null ? {} : captureScopedMessage(options);
      if (
        !capturedOptions ||
        typeof capturedOptions !== 'object' ||
        Array.isArray(capturedOptions) ||
        Object.keys(capturedOptions).some((key) => key !== 'refresh')
      )
        throw new Error(
          'Invoke accepts only a plain refresh option. Remove unsupported invocation options.'
        );
      const refreshOption = (capturedOptions as InvokeOptions).refresh;
      if (refreshOption !== undefined && typeof refreshOption !== 'boolean')
        throw new Error('Invoke refresh must be a Boolean when supplied.');
      const methods = Array.isArray(input) ? input : [input];
      const request: InvokeReadRequest = {
        v: 1,
        i: `session-${++this.#requestId}`,
        sc: scope,
        x: this.context,
        l: methods.map((method) => {
          const object = method?.object;
          const strategy = object?.strategy;
          const id = object?.id;
          const methodType = method?.methodType;
          if (
            !strategy ||
            strategy.session !== this ||
            id <= 0 ||
            this.#entries.get(id)?.object !== object ||
            !methodType
          )
            throw new Error(
              'Invoke requires committed objects and methods from this scoped Session. Push and Pull new objects first.'
            );
          return { i: id, v: strategy.version, m: methodType.tag };
        }),
        refresh: refreshOption ?? true,
      };
      const json = JSON.stringify(captureScopedMessage(request));
      this.#owner.checkMessageSize(json, 'request');
      const captured = decodeInvokeReadRequest(json);
      return await this.#owner.run(
        () => this.assertActive(),
        async () => {
          this.#owner.assertMutation(captured.l.map((method) => method.i));
          checkSaveCancellation(cancellation, 'Invoke');
          let outcome: MutationResultState['outcome'] = 'unknown';
          let diagnostics = mutationDiagnostics({}, 'unknown', [], 'invoke');
          let refresh: MutationRefresh = {
            status: captured.refresh ? 'unavailable' : 'not-requested',
            errors: [],
          };
          let refreshApplication: MutationRefreshApplicationStatus =
            'not-applicable';
          const applicationErrors: MutationApplicationError[] = [];
          const result = () =>
            new ScopedInvokeResult(
              {
                outcome,
                diagnostics,
                refresh,
                application: {
                  acknowledgement: 'not-applicable',
                  refresh: refreshApplication,
                  errors: applicationErrors,
                },
                reconciliationRequired: this.#reconciliationRequired,
              },
              (id) => this.objectByWorkspaceId.get(id),
              (tag) => {
                const meta =
                  this.workspace.configuration.metaPopulation.metaObjectByTag.get(
                    tag
                  ) as RelationType;
                return meta?.kind === 'RelationType' ? meta : undefined;
              }
            );
          let raw: string;
          let receipt: ReturnType<typeof decodeInvokeReadReceipt>;
          try {
            const received = await this.#owner.invokeReadOnce(
              decodeInvokeReadRequest(JSON.stringify(captured)),
              cancellation
            );
            raw =
              typeof received === 'string'
                ? received
                : JSON.stringify(captureScopedMessage(received));
            this.#owner.checkMessageSize(raw, 'response');
            receipt = decodeInvokeReadReceipt(raw, captured);
          } catch (cause) {
            this.#reconciliationRequired = true;
            diagnostics = mutationDiagnostics(
              {},
              'unknown',
              [mutationDiagnostic('invoke-outcome-unknown', cause)],
              'invoke'
            );
            return result();
          }
          outcome = receipt.invoke.k;
          diagnostics = mutationDiagnostics(
            receipt.invoke,
            outcome,
            [],
            'invoke'
          );
          if (outcome !== 'committed') {
            this.#reconciliationRequired ||= outcome === 'unknown';
            return result();
          }
          let changed: PushReadChanged;
          if (captured.refresh) {
            try {
              const response = decodeInvokeReadResponse(raw, captured);
              if (response.changed.k === 'error') {
                this.#owner.observeFailure(response.changed);
                throw new ReadFailedError(response.changed);
              }
              this.#owner.validateChanged(response.changed);
              for (const record of response.changed.p) {
                const entry = this.#entries.get(record.i);
                if (entry && entry.cls.tag !== record.c)
                  throw new Error(
                    'A refreshed object cannot change its class. Obtain a consistent read before applying its baseline.'
                  );
              }
              changed = response.changed;
              refresh = { status: 'succeeded', errors: [] };
            } catch (cause) {
              refresh = {
                status: 'failed',
                errors:
                  cause instanceof ReadFailedError
                    ? readDiagnostics(cause.errors, cause)
                    : [mutationDiagnostic('invoke-refresh-failed', cause)],
              };
            }
          } else {
            // Unsolicited read data is never admitted, even when refresh was skipped.
            try {
              decodeInvokeReadResponse(raw, captured);
            } catch (cause) {
              applicationErrors.push({
                stage: 'refresh',
                message: asError(cause).message,
                cause: asError(cause),
              });
            }
          }
          if (changed) {
            refreshApplication = 'skipped';
            let applying = false;
            try {
              checkSaveCancellation(cancellation, 'Invoke');
              this.assertActive();
              applying = true;
              const completed = this.#owner.acceptChanged(changed);
              const applied = this.applyRead(completed, captured.i);
              refresh = { ...refresh, result: applied };
              refreshApplication = applied.mergeErrors?.length
                ? 'conflicted'
                : 'applied';
            } catch (cause) {
              refreshApplication = applying ? 'failed' : 'skipped';
              this.#reconciliationRequired ||= this.#faulted;
              const error = asError(cause);
              applicationErrors.push({
                stage: 'refresh',
                message: error.message,
                cause: error,
              });
            }
          }
          return result();
        }
      );
    } finally {
      this.#saving = false;
    }
  }

  private validateEntry(
    entry: { object: IObject; strategy: ScopedStrategy },
    id: number
  ): void {
    if (entry.object.id !== id || entry.object.strategy !== entry.strategy)
      throw new Error(
        'The object factory must preserve its supplied strategy and object identity. Correct the factory before creating a new scoped Session.'
      );
  }
}

function asError(cause: unknown): Error {
  return cause instanceof Error ? cause : new Error(String(cause));
}

function checkSaveCancellation(
  cancellation?: AbortSignal,
  operation = 'Push'
): void {
  if (cancellation?.aborted) {
    const error = new Error(
      `${operation} cancelled. Inspect the mutation outcome before any further save.`
    );
    error.name = 'AbortError';
    throw error;
  }
}
