import { RoleType } from '@allors/workspace/meta/system';

export class DatabaseObject {
  deniedPermissions: Permission[];
  accessControls: AccessControl[];

  roleByRelationType: Map<RelationType, unknown>;
  syncResponseRoles: SyncResponseRole[];

  constructor(public readonly database: Database, public readonly identity: number, public readonly cls: Class) {
    this.version = 0;
  }

  // internal DatabaseObject(Database database, ResponseContext ctx, SyncResponseObject syncResponseObject)
  // {
  //     this.Database = database;
  //     this.Identity = syncResponseObject.Id;
  //     this.Class = (IClass)this.Database.MetaPopulation.FindByTag(syncResponseObject.ObjectType);
  //     this.Version = syncResponseObject.Version;
  //     this.syncResponseRoles = syncResponseObject.Roles;
  //     this.AccessControlIds = syncResponseObject.AccessControls != null ? (ISet<long>)new HashSet<long>(ctx.CheckForMissingAccessControls(syncResponseObject.AccessControls)) : EmptySet<long>.Instance;
  //     this.DeniedPermissionIds = syncResponseObject.DeniedPermissions != null ? (ISet<long>)new HashSet<long>(ctx.CheckForMissingPermissions(syncResponseObject.DeniedPermissions)): EmptySet<long>.Instance;
  // }

  version: number;

  accessControlIds: Set<number>;

  deniedPermissionIds: Set<number>;

  get roleByRelationType(): Map<RelationType, object> {
    // if (this.syncResponseRoles != null)
    // {
    //     var meta = this.database.metaPopulation;
    //     var metaPopulation = this.database.MetaPopulation;
    //     this.roleByRelationType = this.syncResponseRoles.ToDictionary(
    //         v => (IRelationType)meta.FindByTag(v.RoleType),
    //         v =>
    //         {
    //             var roleType = ((IRelationType)metaPopulation.FindByTag(v.RoleType)).RoleType;
    //             var objectType = roleType.ObjectType;
    //             if (objectType.IsUnit)
    //             {
    //                 return UnitConvert.FromString(roleType.ObjectType.Tag, v.Value);
    //             }
    //             if (roleType.IsOne)
    //             {
    //                 return v.Object;
    //             }
    //             return v.Collection;
    //         });
    //     this.syncResponseRoles = null;
    // }
    // return this.roleByRelationType;
  }

  get AccessControls(): AccessControl[] {
    // return this.accessControls switch
    // {
    //     null when this.AccessControlIds == null => Array.Empty<AccessControl>(),
    //     null => this.AccessControlIds
    //         .Select(v => this.Database.AccessControlById[v])
    //         .ToArray(),
    //     _ => this.accessControls
    // };
  }

  get deniedPermissions(): Permission[] {
    // this.deniedPermissions = this.deniedPermissions switch
    // {
    //     null when this.DeniedPermissionIds == null => Array.Empty<Permission>(),
    //     null => this.DeniedPermissionIds
    //         .Select(v => this.Database.PermissionById[v])
    //         .ToArray(),
    //     _ => this.deniedPermissions
    // };
  }

  getRole(roleType: RoleType): unknown {
    // object @object = null;
    // _ = this.RoleByRelationType?.TryGetValue(roleType.RelationType, out @object);
    // return @object;
  }

  isPermitted(permission: Permission): boolean {
    permission != null && !this.DeniedPermissions.Contains(permission) && this.AccessControls.Any((v) => v.PermissionIds.Any((w) => w == permission.Id));
  }

  updateDeniedPermissions(updatedDeniedPermissions: number[]) {
    if (this.deniedPermissions == null) {
      this.deniedPermissionIds = [];
    } else {
      // if (!this.deniedPermissionIds.SetEquals(updatedDeniedPermissions))
      // {
      //     this.DeniedPermissionIds = new HashSet<long>(updatedDeniedPermissions);
      // }
    }
  }
}
