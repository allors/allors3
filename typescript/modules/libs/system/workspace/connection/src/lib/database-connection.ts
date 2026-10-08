import {
  AccessResponseGrant,
  AccessResponseRevocation,
  InvokeRequest,
  PermissionResponsePermission,
  PullRequest,
  PullResponse,
  PushRequest,
  SyncResponseObject,
} from '@allors/system/common/protocol-json';
import {
  Class,
  Dependency,
  MetaPopulation,
  MethodType,
  OperandType,
  RelationType,
} from '@allors/system/workspace/meta';
import { ICache } from './cache/icache';
import { MemoryCache } from './cache/memory-cache';
import { DefaultNumberRanges } from './collections/ranges/default-number-ranges';
import { Ranges } from './collections/ranges/ranges';
import { Procedure } from './data/procedure';
import { Pull } from './data/pull';
import { Grant } from './grant';
import { Invocation } from './invoke/invocation';
import { InvokeOptions } from './invoke-options';
import { PushEncoder } from './json/push-encoder';
import {
  dependenciesToJson,
  procedureToJson,
  pullToJson,
} from './json/to-json';
import { Operations } from './operations';
import { Permission } from './permission';
import { PushChangedObject } from './push/push-changed-object';
import { PushNewObject } from './push/push-new-object';
import { IRecord, Record } from './record';
import { RecordChangedEvent } from './record-changed-event';
import { ResponseContext } from './response-context';
import { InvokeResult } from './results/invoke-result';
import { PullResult } from './results/pull-result';
import { PushResult } from './results/push-result';
import { responseHasErrors } from './results/call-result';
import { Revocation } from './revocation';
import { Emitter, Subscribable } from './subscribable';
import { ITransport } from './transport';

/**
 * What a call may carry besides its arguments: the tracing context the server appends to
 * its events.
 */
export interface CallOptions {
  context?: string;
}

export interface PullOptions extends CallOptions {
  /**
   * A procedure the server runs before the pulls.
   */
  procedure?: Procedure;

  /**
   * The dependencies of the derived roles the caller has activated, so that the server
   * includes what they need.
   */
  dependencies?: Iterable<Dependency>;
}

export type InvokeCallOptions = InvokeOptions & CallOptions;

export interface DatabaseConnectionOptions {
  /**
   * The cache to keep the user's view in; a MemoryCache of the connection's own when absent.
   */
  cache?: ICache;

  /**
   * The ranges that order the ids of composites roles; the default number ranges when
   * absent.
   */
  ranges?: Ranges<number>;
}

/**
 * The lowest layer of a workspace: the full contract between the workspace and the database,
 * reads and writes alike, in ids, versions, meta types and role values, without objects. It
 * pulls by the query model and keeps what it receives as records, grants, revocations and
 * permissions; it pushes new and changed objects and invokes methods. The layers above build
 * objects and change tracking on it; the transport underneath carries the wire.
 */
export interface IDatabaseConnection {
  /**
   * The name of the workspace the server serves: it decides which classes and roles the
   * records carry.
   */
  readonly workspaceName: string;

  readonly metaPopulation: MetaPopulation;

  readonly ranges: Ranges<number>;

  /**
   * What the connection keeps of the user's view of the database.
   */
  readonly cache: ICache;

  /**
   * Raised after a pull for every record that the pull replaced by a newer one, once the
   * records, grants and permissions of the pull are in.
   */
  readonly recordChanged: Subscribable<RecordChangedEvent>;

  /**
   * The record of the object, or undefined when the connection has not received it.
   */
  getRecord(id: number): IRecord | undefined;

  /**
   * The id of the permission for the operation on the operand type of the class, or 0 when
   * the connection has not received it.
   */
  getPermission(
    cls: Class,
    operandType: OperandType,
    operation: Operations
  ): number;

  /**
   * Pulls by the query model, with a procedure on the server when given, and brings the
   * records of every object in the result up to date.
   */
  pull(pulls: Pull[], options?: PullOptions): Promise<PullResult>;

  /**
   * Pushes new objects, each with a workspace id, and changed objects, each with the
   * version it was changed from; null for none.
   */
  push(
    newObjects: PushNewObject[] | null | undefined,
    changedObjects: PushChangedObject[] | null | undefined,
    options?: CallOptions
  ): Promise<PushResult>;

  invoke(
    invocations: Invocation[],
    options?: InvokeCallOptions
  ): Promise<InvokeResult>;
}

/**
 * The connection over a transport. After a pull it brings the records of the pulled objects
 * up to date: it syncs the objects whose version, grants or revocations differ from what it
 * holds, requests the grants and revocations it lacks, and then the permissions those name.
 */
export class DatabaseConnection implements IDatabaseConnection {
  readonly ranges: Ranges<number>;

  readonly cache: ICache;

  private readonly pushEncoder: PushEncoder;

  private readonly recordChangedEmitter = new Emitter<RecordChangedEvent>();

  constructor(
    public readonly workspaceName: string,
    public readonly metaPopulation: MetaPopulation,
    private readonly transport: ITransport,
    options?: DatabaseConnectionOptions
  ) {
    if (workspaceName == null) {
      throw new Error('A connection needs the name of the workspace the server serves.');
    }

    if (metaPopulation == null) {
      throw new Error('A connection needs the meta population of its workspace.');
    }

    if (transport == null) {
      throw new Error('A connection needs a transport to the server.');
    }

    this.ranges = options?.ranges ?? new DefaultNumberRanges();
    this.cache = options?.cache ?? new MemoryCache(workspaceName, metaPopulation);
    this.pushEncoder = new PushEncoder(this.cache, this.ranges);
  }

  get recordChanged(): Subscribable<RecordChangedEvent> {
    return this.recordChangedEmitter;
  }

  getRecord(id: number): IRecord | undefined {
    return this.cache.getRecord(id);
  }

  getPermission(
    cls: Class,
    operandType: OperandType,
    operation: Operations
  ): number {
    return this.cache.getPermission(cls, operandType, operation);
  }

  async pull(pulls: Pull[], options?: PullOptions): Promise<PullResult> {
    pulls ??= [];

    for (const pull of pulls) {
      if (pull.objectId < 0 || pull.object?.id < 0) {
        throw new Error('Id is not in the database');
      }
    }

    const request: PullRequest = {
      x: options?.context,
      d: dependenciesToJson(options?.dependencies),
      p: procedureToJson(options?.procedure),
      l: pulls.map((v) => pullToJson(v)),
    };

    const response = await this.transport.pull(request);
    return await this.onPull(response, options?.context);
  }

  async push(
    newObjects: PushNewObject[] | null | undefined,
    changedObjects: PushChangedObject[] | null | undefined,
    options?: CallOptions
  ): Promise<PushResult> {
    const request: PushRequest = {
      x: options?.context,
    };

    if (newObjects != null) {
      request.n = newObjects.map((v) => this.pushEncoder.newObject(v));
    }

    if (changedObjects != null) {
      request.o = changedObjects.map((v) => this.pushEncoder.changedObject(v));
    }

    const response = await this.transport.push(request);
    return new PushResult(this.metaPopulation, response);
  }

  async invoke(
    invocations: Invocation[],
    options?: InvokeCallOptions
  ): Promise<InvokeResult> {
    const request: InvokeRequest = {
      x: options?.context,
      l: invocations.map((v) => ({
        i: v.id,
        v: v.version,
        m: v.methodType.tag,
      })),
      o:
        options?.continueOnError != null || options?.isolated != null
          ? {
              c: options.continueOnError,
              i: options.isolated,
            }
          : null,
    };

    const response = await this.transport.invoke(request);
    return new InvokeResult(this.metaPopulation, response);
  }

  private async onPull(
    response: PullResponse,
    context: string | undefined
  ): Promise<PullResult> {
    if (!responseHasErrors(response)) {
      await this.sync(response, context);
    }

    return new PullResult(this.metaPopulation, response);
  }

  private async sync(response: PullResponse, context: string | undefined) {
    const ctx = new ResponseContext(this.cache);
    const replaced: IRecord[] = [];

    // The objects to bring up to date: absent from the cache, or held at another version or
    // with other grants or revocations than the pull advertises.
    const staleObjectIds = this.staleObjectIds(response);
    if (staleObjectIds.length > 0) {
      const syncResponse = await this.transport.sync({
        x: context,
        o: staleObjectIds,
      });
      this.storeRecords(syncResponse.o, ctx, replaced);
    }

    // The grants and revocations to bring up to date: the ones the new records name that
    // the cache lacks.
    if (ctx.missingGrantIds.size > 0 || ctx.missingRevocationIds.size > 0) {
      const accessResponse = await this.transport.access({
        g: [...ctx.missingGrantIds],
        r: [...ctx.missingRevocationIds],
      });
      const missingPermissionIds = this.storeAccess(
        accessResponse.g,
        accessResponse.r
      );

      if (missingPermissionIds != null) {
        const permissionResponse = await this.transport.permission({
          p: [...missingPermissionIds],
        });
        this.storePermissions(permissionResponse.p);
      }
    }

    for (const record of replaced) {
      this.recordChangedEmitter.emit(new RecordChangedEvent(record));
    }
  }

  private staleObjectIds(response: PullResponse): number[] {
    return (response.p ?? [])
      .filter((v) => {
        const record = this.cache.getRecord(v.i);

        if (record == null) {
          return true;
        }

        if (record.version !== v.v) {
          return true;
        }

        if (!this.ranges.equals(record.grantIds, this.ranges.importFrom(v.g))) {
          return true;
        }

        if (
          !this.ranges.equals(record.revocationIds, this.ranges.importFrom(v.r))
        ) {
          return true;
        }

        return false;
      })
      .map((v) => v.i);
  }

  private storeRecords(
    syncResponseObjects: SyncResponseObject[] | null | undefined,
    ctx: ResponseContext,
    replaced: IRecord[]
  ) {
    if (syncResponseObjects == null) {
      return;
    }

    for (const syncResponseObject of syncResponseObjects) {
      const record = new Record(
        this.cache,
        this.metaPopulation,
        this.ranges,
        ctx,
        syncResponseObject
      );
      const previous = this.cache.getRecord(record.id);

      if (this.cache.setRecord(record) && previous != null) {
        replaced.push(record);
      }
    }
  }

  private storeAccess(
    grants: AccessResponseGrant[] | null | undefined,
    revocations: AccessResponseRevocation[] | null | undefined
  ): Set<number> | null {
    let missingPermissionIds: Set<number> | null = null;

    const collectMissingPermissions = (permissionIds: number[] | undefined) => {
      for (const permissionId of permissionIds ?? []) {
        if (this.cache.hasPermission(permissionId)) {
          continue;
        }

        missingPermissionIds ??= new Set<number>();
        missingPermissionIds.add(permissionId);
      }
    };

    if (grants != null) {
      for (const accessResponseGrant of grants) {
        const permissionIds = this.ranges.importFrom(accessResponseGrant.p);
        this.cache.setGrant(
          new Grant(accessResponseGrant.i, accessResponseGrant.v, permissionIds)
        );
        collectMissingPermissions(permissionIds);
      }
    }

    if (revocations != null) {
      for (const accessResponseRevocation of revocations) {
        const permissionIds = this.ranges.importFrom(accessResponseRevocation.p);
        this.cache.setRevocation(
          new Revocation(
            accessResponseRevocation.i,
            accessResponseRevocation.v,
            permissionIds
          )
        );
        collectMissingPermissions(permissionIds);
      }
    }

    return missingPermissionIds;
  }

  private storePermissions(
    permissions: PermissionResponsePermission[] | null | undefined
  ) {
    if (permissions == null) {
      return;
    }

    for (const permissionResponsePermission of permissions) {
      const cls = this.metaPopulation.metaObjectByTag.get(
        permissionResponsePermission.c
      ) as Class;
      const metaObject = this.metaPopulation.metaObjectByTag.get(
        permissionResponsePermission.t
      );
      const operandType: OperandType =
        (metaObject as RelationType)?.roleType ?? (metaObject as MethodType);
      const operation = permissionResponsePermission.o as Operations;

      this.cache.setPermission(
        new Permission(
          permissionResponsePermission.i,
          cls,
          operandType,
          operation
        )
      );
    }
  }
}
