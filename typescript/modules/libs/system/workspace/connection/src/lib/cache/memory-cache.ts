import { Class, MetaPopulation, OperandType } from '@allors/system/workspace/meta';
import { MapMap } from '../collections/map-map';
import { Grant } from '../grant';
import { Operations } from '../operations';
import { Permission } from '../permission';
import { IRecord } from '../record';
import { RecordChangedEvent } from '../record-changed-event';
import { Revocation } from '../revocation';
import { Emitter, Subscribable } from '../subscribable';
import { CacheKey } from './cache-key';
import { ICache } from './icache';

/**
 * The cache in memory: maps of records, grants, revocations and permissions, which the
 * connections of one user may share. A set keeps the newest version of an object, a grant or
 * a revocation, whichever connection delivers it first. It holds everything until
 * removeRecord or clear; an eviction policy builds on those two.
 */
export class MemoryCache implements ICache {
  private readonly recordById = new Map<number, IRecord>();

  private readonly grantById = new Map<number, Grant>();

  private readonly revocationById = new Map<number, Revocation>();

  private readonly permissionById = new Map<number, Permission>();

  private readonly permissionIdByOperandTypeByClassByOperation = new Map<
    Operations,
    MapMap<Class, OperandType, number>
  >();

  private readonly recordChangedEmitter = new Emitter<RecordChangedEvent>();

  private _key: CacheKey | null = null;

  constructor(
    public readonly workspaceName: string,
    public readonly metaPopulation: MetaPopulation
  ) {
    if (workspaceName == null) {
      throw new Error('A cache needs the name of the workspace it holds the records of.');
    }

    if (metaPopulation == null) {
      throw new Error('A cache needs the meta population its records are typed by.');
    }
  }

  get recordChanged(): Subscribable<RecordChangedEvent> {
    return this.recordChangedEmitter;
  }

  get key(): CacheKey | null {
    return this._key;
  }

  bind(key: CacheKey): void {
    if (key == null) {
      throw new Error('A cache binds to a key, not to null.');
    }

    if (this._key == null) {
      this._key = key;
      return;
    }

    if (!this._key.equals(key)) {
      throw new Error(
        `The cache holds the view of user ${this._key.userId} of database '${this._key.databaseId}' and cannot serve user ${key.userId} of database '${key.databaseId}': a cache serves one user of one database. Give each user a cache of its own, or clear the cache when the user signs out.`
      );
    }
  }

  getRecord(id: number): IRecord | undefined {
    return this.recordById.get(id);
  }

  setRecord(record: IRecord): boolean {
    if (record == null) {
      throw new Error('A cache keeps records, not null.');
    }

    const previous = this.recordById.get(record.id);
    if (previous != null && record.version < previous.version) {
      return false;
    }

    this.recordById.set(record.id, record);

    if (previous != null) {
      this.recordChangedEmitter.emit(new RecordChangedEvent(record));
    }

    return true;
  }

  removeRecord(id: number): void {
    this.recordById.delete(id);
  }

  clear(): void {
    this.recordById.clear();
    this.grantById.clear();
    this.revocationById.clear();
    this.permissionById.clear();
    this.permissionIdByOperandTypeByClassByOperation.clear();
    this._key = null;
  }

  getGrant(id: number): Grant | undefined {
    return this.grantById.get(id);
  }

  setGrant(grant: Grant): void {
    const held = this.grantById.get(grant.id);
    if (held == null || grant.version >= held.version) {
      this.grantById.set(grant.id, grant);
    }
  }

  getRevocation(id: number): Revocation | undefined {
    return this.revocationById.get(id);
  }

  setRevocation(revocation: Revocation): void {
    const held = this.revocationById.get(revocation.id);
    if (held == null || revocation.version >= held.version) {
      this.revocationById.set(revocation.id, revocation);
    }
  }

  hasPermission(id: number): boolean {
    return this.permissionById.has(id);
  }

  setPermission(permission: Permission): void {
    this.permissionById.set(permission.id, permission);

    let byOperandTypeByClass =
      this.permissionIdByOperandTypeByClassByOperation.get(permission.operation);
    if (byOperandTypeByClass == null) {
      byOperandTypeByClass = new MapMap();
      this.permissionIdByOperandTypeByClassByOperation.set(
        permission.operation,
        byOperandTypeByClass
      );
    }

    byOperandTypeByClass.set(permission.cls, permission.operandType, permission.id);
  }

  getPermission(
    cls: Class,
    operandType: OperandType,
    operation: Operations
  ): number {
    return (
      this.permissionIdByOperandTypeByClassByOperation
        .get(operation)
        ?.get(cls, operandType) ?? 0
    );
  }
}
