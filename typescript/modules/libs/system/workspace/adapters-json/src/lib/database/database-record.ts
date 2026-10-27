import { Class, RelationType, RoleType } from '@allors/system/workspace/meta';
import {
  DatabaseRecord as SystemDatabaseRecord,
  DefaultNumberRanges,
} from '@allors/system/workspace/adapters';
import { SyncResponseObject } from '@allors/system/common/protocol-json';
import { DatabaseConnection } from './database-connection';
import { AccessEntry, ResponseContext } from './security/response-context';
import { unitFromJson } from '../json/from-json';

export class DatabaseRecord extends SystemDatabaseRecord {
  private static readonly ranges = new DefaultNumberRanges();
  readonly grants: readonly number[];
  readonly revocations: readonly number[];
  readonly key: string;

  private constructor(
    cls: Class,
    id: number,
    version: number,
    private readonly grantEntries: readonly AccessEntry[],
    private readonly revocationEntries: readonly AccessEntry[],
    private readonly roleByRelationType: Map<RelationType, unknown>
  ) {
    super(cls, id, version);
    this.grants = Object.freeze(grantEntries.map((v) => v.id));
    this.revocations = Object.freeze(revocationEntries.map((v) => v.id));
    this.key = ResponseContext.payloadKey(
      id,
      version,
      grantEntries.map((v) => [v.id, v.version]),
      revocationEntries.map((v) => [v.id, v.version])
    );
    Object.freeze(this);
  }

  static fromResponse(
    database: DatabaseConnection,
    wire: SyncResponseObject,
    grants: AccessEntry[],
    revocations: AccessEntry[]
  ): DatabaseRecord {
    const meta = database.configuration.metaPopulation.metaObjectByTag;
    const cls = meta.get(wire.c) as Class;
    if (cls?.kind !== 'Class') {
      throw new Error('Sync object references an unknown class.');
    }
    const roles = new Map<RelationType, unknown>();
    for (const role of ResponseContext.array(wire.ro, 'Sync roles')) {
      const relation = meta.get(role.t) as RelationType;
      const roleType = relation?.roleType;
      if (
        !roleType ||
        !cls.databaseOriginRoleTypes.has(roleType) ||
        roles.has(relation)
      ) {
        throw new Error('Sync contains an unknown or duplicate role.');
      }
      let value: unknown;
      if (roleType.objectType.isUnit) {
        if (role.o != null || role.c != null) {
          throw new Error(
            'Sync unit roles cannot contain relationship members.'
          );
        }
        value = unitFromJson(roleType.objectType.tag, role.v);
        if (value instanceof Date) {
          if (!Number.isFinite(value.getTime())) {
            throw new Error('Sync contains an invalid date.');
          }
          value = new Date(value.getTime());
        }
      } else if (roleType.isOne) {
        if (role.v != null || role.c != null) {
          throw new Error(
            'Sync to-one roles cannot contain unit or collection members.'
          );
        }
        value = role.o == null || role.o === 0 ? undefined : role.o;
        if (value != null) {
          ResponseContext.id(value as number);
        }
      } else {
        if (role.v != null || role.o != null) {
          throw new Error(
            'Sync to-many roles cannot contain unit or object members.'
          );
        }
        value = Object.freeze(ResponseContext.ids(role.c));
      }
      roles.set(relation, value);
    }
    return new DatabaseRecord(
      cls,
      wire.i,
      wire.v,
      Object.freeze([...grants]),
      Object.freeze([...revocations]),
      roles
    );
  }

  getRole(roleType: RoleType): unknown {
    const value = this.roleByRelationType.get(roleType.relationType);
    return value instanceof Date ? new Date(value.getTime()) : value;
  }

  samePayload(other: DatabaseRecord): boolean {
    if (
      this.cls !== other.cls ||
      this.roleByRelationType.size !== other.roleByRelationType.size
    ) {
      return false;
    }
    for (const [relation, value] of this.roleByRelationType) {
      if (!other.roleByRelationType.has(relation)) {
        return false;
      }
      const otherValue = other.roleByRelationType.get(relation);
      if (value instanceof Date && otherValue instanceof Date) {
        if (value.getTime() !== otherValue.getTime()) {
          return false;
        }
      } else if (Array.isArray(value) && Array.isArray(otherValue)) {
        if (!ResponseContext.sameIds(value, otherValue)) {
          return false;
        }
      } else if (value !== otherValue) {
        return false;
      }
    }
    return true;
  }

  isPermitted(permission: number): boolean {
    return (
      permission != null &&
      !this.revocationEntries.some((entry) =>
        DatabaseRecord.ranges.has(entry.permissionIds as number[], permission)
      ) &&
      this.grantEntries.some((entry) =>
        DatabaseRecord.ranges.has(entry.permissionIds as number[], permission)
      )
    );
  }
}
