import { SyncResponseObject, SyncResponseRole } from '@allors/protocol/json/system';
import { Class, RelationType, RoleType } from '@allors/workspace/meta/system';
import { Database } from './Database';
import { AccessControl } from './Security/AccessControl';
import { Permission } from './Security/Permission';
import { ResponseContext } from './Security/ResponseContext';

export class DatabaseObject {
  version: number;

  private _roleByRelationType: Map<RelationType, unknown>;
  private _syncResponseRoles: SyncResponseRole[];

  private _accessControls: AccessControl[];
  private _accessControlIds: number[];

  private _deniedPermissions: Permission[];
  private _deniedPermissionIds: number[];

  constructor(public readonly database: Database, public readonly identity: number, public readonly cls: Class) {
    this.version = 0;
  }

  static fromResponse(database: Database, ctx: ResponseContext, syncResponseObject: SyncResponseObject): DatabaseObject {
    const obj = new DatabaseObject(database, syncResponseObject.i, database.metaPopulation.metaObjectByTag.get(syncResponseObject.t) as Class);
    obj.version = syncResponseObject.v;
    obj._syncResponseRoles = syncResponseObject.r;
    obj._accessControlIds = ctx.checkForMissingAccessControls(syncResponseObject.a);
    obj._deniedPermissionIds = ctx.checkForMissingPermissions(syncResponseObject.d);
    return obj;
  }

  get roleByRelationType(): Map<RelationType, unknown> {
    if (this._syncResponseRoles != null) {
      const meta = this.database.metaPopulation;
      this._roleByRelationType = new Map(
        this._syncResponseRoles.map((v) => {
          const relationType = meta.metaObjectByTag.get(v.t) as RelationType;
          const roleType = relationType.roleType;
          const objectType = roleType.objectType;
          let role: unknown;

          if (objectType.isUnit) {
            role = v.v;
            // TODO:
            // return UnitConvert.FromString(roleType.ObjectType.Tag, v.Value);
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

      this._syncResponseRoles = null;
    }

    return this._roleByRelationType;
  }

  get accessControls(): AccessControl[] {
    return (this._accessControls ??= this._accessControlIds?.map((v) => this.database.accessControlById.get(v)) ?? []);
  }

  get deniedPermissions(): Permission[] {
    return (this._deniedPermissions ??= this._deniedPermissionIds?.map((v) => this.database.permissionById.get(v)) ?? []);
  }

  getRole(roleType: RoleType): unknown {
    return this.roleByRelationType.get(roleType.relationType);
  }

  isPermitted(permission: Permission): boolean {
    return !!permission && !this.deniedPermissions.includes(permission) && !!this.accessControls.filter((v) => v.permissionIds.has(permission.id)).length;
  }

  updateDeniedPermissions(updatedDeniedPermissions: number[]) {
    this._deniedPermissionIds = updatedDeniedPermissions;
  }
}
