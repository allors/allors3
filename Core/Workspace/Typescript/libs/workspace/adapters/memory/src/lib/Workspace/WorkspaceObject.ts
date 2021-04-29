import { RelationType, RoleType } from '@allors/workspace/meta/system';
import { Database } from '../Database/Database';

export class WorkspaceObject {
  constructor(public database: Database, public identity: number, public cls: Class, public version: number, public roleByRelationType: Map<RelationType, any>) {}

  // public WorkspaceObject(WorkspaceObject originalWorkspaceObject, IReadOnlyDictionary<IRelationType, object> changedRoleByRoleType)
  // {
  //     this.Database = originalWorkspaceObject.Database;
  //     this.Identity = originalWorkspaceObject.Identity;
  //     this.Class = originalWorkspaceObject.Class;
  //     this.Version = ++originalWorkspaceObject.Version;

  //     this.roleByRelationType = this.Import(changedRoleByRoleType, originalWorkspaceObject.roleByRelationType).ToDictionary(v => v.Key, v => v.Value);
  // }

  getRole(roleType: RoleType): unknown {
    return this.roleByRelationType?.get(roleType.relationType);
  }

  import(changedRoleByRoleType: Map<RelationType, unknown>, originalRoleByRoleType?: Map<RelationType, object>): Map<RelationType, unknown> {
    // for (const roleType of this.cls.WorkspaceRoleTypes)
    // {
    //     var relationType = roleType.relationType;
    //     if (changedRoleByRoleType.has(relationType))
    //     {
    //       const role = changedRoleByRoleType.get(relationType);
    //         if (role != null)
    //         {
    //             if (roleType.ObjectType.IsUnit)
    //             {
    //                 yield return new KeyValuePair<IRelationType, object>(relationType, role);
    //             }
    //             else
    //             {
    //                 if (roleType.IsOne)
    //                 {
    //                     yield return new KeyValuePair<IRelationType, object>(relationType, ((Strategy)role).Id);
    //                 }
    //                 else
    //                 {
    //                     var roles = (Strategy[])role;
    //                     if (roles.Length > 0)
    //                     {
    //                         yield return new KeyValuePair<IRelationType, object>(relationType, roles.Select(v => v.Id).ToArray());
    //                     }
    //                 }
    //             }
    //         }
    //     }
    //     else if (originalRoleByRoleType?.TryGetValue(roleType.RelationType, out role) == true)
    //     {
    //         yield return new KeyValuePair<IRelationType, object>(relationType, role);
    //     }
    // }
  }
}
