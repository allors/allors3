import { Class, RelationType, RoleType } from '@allors/workspace/meta/system';
import { DatabaseRecord as SystemDatabaseRecord, has, IRange } from '@allors/workspace/adapters/system';
import { SyncResponseObject, SyncResponseRole } from '@allors/protocol/json/system';
import { Database } from './Database';
import { ResponseContext } from './Security/ResponseContext';
import { unitFromJson } from '../json/fromJson';

export class DatabaseRecord extends SystemDatabaseRecord {
  accessControlIds: IRange;
  deniedPermissionIds: IRange;

  private _roleByRelationType?: Map<RelationType, unknown>;
  private syncResponseRoles?: SyncResponseRole[];

  constructor(public readonly database: Database, cls: Class, id: number, public version: number) {
    super(cls, id, version);
  }

  static fromResponse(database: Database, ctx: ResponseContext, syncResponseObject: SyncResponseObject): DatabaseRecord {
    const obj = new DatabaseRecord(database, database.configuration.metaPopulation.metaObjectByTag.get(syncResponseObject.t) as Class, syncResponseObject.i, syncResponseObject.v);
    obj.syncResponseRoles = syncResponseObject.r;
    obj.accessControlIds = ctx.checkForMissingAccessControls(syncResponseObject.a);
    obj.deniedPermissionIds = ctx.checkForMissingPermissions(syncResponseObject.d);
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
    if (this.accessControlIds == null) {
      return false;
    }

    if (has(this.deniedPermissionIds, permission)) {
      return false;
    }

    if (this.accessControlIds == null) {
      return false;
    }

    return this.accessControlIds.some((v) => has(this.database.accessControlById.get(v).permissionIds, permission));
  }
}
