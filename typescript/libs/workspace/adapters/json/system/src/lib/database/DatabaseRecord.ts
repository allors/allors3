import { Class, RelationType, RoleType } from '@allors/workspace/meta/system';
import { DatabaseRecord as SystemDatabaseRecord, IRange } from '@allors/workspace/adapters/system';
import { SyncResponseObject, SyncResponseRole } from '@allors/protocol/json/system';
import { DatabaseConnection } from './DatabaseConnection';
import { ResponseContext } from './Security/ResponseContext';
import { unitFromJson } from '../json/fromJson';

export class DatabaseRecord extends SystemDatabaseRecord {
  grants: IRange<number>;
  revocations: IRange<number>;

  private _roleByRelationType?: Map<RelationType, unknown>;
  private syncResponseRoles?: SyncResponseRole[];

  constructor(public readonly database: DatabaseConnection, cls: Class, id: number, public version: number) {
    super(cls, id, version);
  }

  static fromResponse(database: DatabaseConnection, ctx: ResponseContext, syncResponseObject: SyncResponseObject): DatabaseRecord {
    const obj = new DatabaseRecord(database, database.configuration.metaPopulation.metaObjectByTag.get(syncResponseObject.c) as Class, syncResponseObject.i, syncResponseObject.v);
    obj.syncResponseRoles = syncResponseObject.ro;
    obj.grants = ctx.checkForMissingGrants(syncResponseObject.g);
    obj.revocations = ctx.checkForMissingRevocations(syncResponseObject.r);
    return obj;
  }

  get roleByRelationType(): Map<RelationType, unknown> {
    if (this.syncResponseRoles != null) {
      const metaPopulation = this.database.configuration.metaPopulation;
      this._roleByRelationType = new Map(
        this.syncResponseRoles.map((v) => {
          const relationType = metaPopulation.metaObjectByTag.get(v.t) as RelationType;
          const roleType = relationType.roleType;
          const objectType = roleType.objectType;

          let role: unknown;

          if (objectType.isUnit) {
            role = unitFromJson(objectType.tag, v.v);
          } else {
            if (roleType.isOne) {
              role = v.o;
            } else {
              role = v.c;
            }
          }

          return [relationType, role];
        })
      );

      delete this.syncResponseRoles;
    }

    return this._roleByRelationType;
  }

  getRole(roleType: RoleType): unknown {
    return this.roleByRelationType?.get(roleType.relationType);
  }

  isPermitted(permission: number): boolean {
    if (this.grants == null) {
      return false;
    }

    if (this.database.ranges.has(this.revocations, permission)) {
      return false;
    }

    if (this.grants == null) {
      return false;
    }

    return this.grants.some((v) => this.database.ranges.has(this.database.grantById.get(v).permissionIds, permission));
  }
}
