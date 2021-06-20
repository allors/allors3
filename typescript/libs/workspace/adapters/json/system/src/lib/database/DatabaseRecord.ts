import { SyncResponseObject, SyncResponseRole } from '@allors/protocol/json/system';
import { Class, RelationType, RoleType } from '@allors/workspace/meta/system';
import { fromString } from '@allors/workspace/domain/system';
import { has, Numbers } from '../collections/Numbers';
import { Database } from './Database';
import { ResponseContext } from './Security/ResponseContext';
import { IRecord } from '../IRecord';

export class DatabaseRecord implements IRecord {
  accessControlIds: Numbers;
  deniedPermissionIds: Numbers;

  private _roleByRelationType?: Map<RelationType, unknown>;
  private syncResponseRoles?: SyncResponseRole[];

  constructor(public readonly database: Database, public readonly cls: Class, public readonly id: number, public version: number) {}

  static fromResponse(database: Database, ctx: ResponseContext, syncResponseObject: SyncResponseObject): DatabaseRecord {
    const obj = new DatabaseRecord(database, database.metaPopulation.metaObjectByTag.get(syncResponseObject.t) as Class, syncResponseObject.i, syncResponseObject.v);
    obj.syncResponseRoles = syncResponseObject.r;
    obj.accessControlIds = ctx.checkForMissingAccessControls(syncResponseObject.a);
    obj.deniedPermissionIds = ctx.checkForMissingPermissions(syncResponseObject.d);
    return obj;
  }

  get roleByRelationType(): Map<RelationType, unknown> {
    if (this.syncResponseRoles != null) {
      const meta = this.database.metaPopulation;
      this._roleByRelationType = new Map(
        this.syncResponseRoles.map((v) => {
          const relationType = meta.metaObjectByTag.get(v.t) as RelationType;
          const roleType = relationType.roleType;
          const objectType = roleType.objectType;
          let role: unknown;

          if (objectType.isUnit) {
            role = fromString(objectType.tag, v.v);
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

    return !has(this.deniedPermissionIds, permission) && this.accessControlIds.some((v) => has(this.database.accessControlById[v].permissionIds, permission));
  }
}
