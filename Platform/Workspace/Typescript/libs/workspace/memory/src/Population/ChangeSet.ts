import { RoleType, AssociationType, MetaPopulation } from '@allors/meta/system';

export class ChangeSet {
  readonly roleByAssociationByRoleType: Map<RoleType, Map<string, any>>;
  readonly associationByRoleByRoleType: Map<AssociationType, Map<string, any>>;

  constructor(
    public readonly meta: MetaPopulation,
    roleByAssociationByRoleType: Map<RoleType, Map<string, any>>,
    associationByRoleByAssociationType: Map<AssociationType, Map<string, any>>
  ) {
    this.roleByAssociationByRoleType = roleByAssociationByRoleType;
    this.associationByRoleByRoleType = associationByRoleByAssociationType;
  }

  hasChanges(): boolean {
    function some(it, pred) {
      for (const v of it) {
        if (pred(v)) return true;
      }

      return false;
    }

    return (
      some(this.roleByAssociationByRoleType.values(), (v) => v.Value.Count > 0) ||
      some(this.associationByRoleByRoleType.values(), (v) => v.Value.Count > 0)
    );
  }

  ChangedRoles(roleType: RoleType): Map<any, any> {
    const changedRelations = this.roleByAssociationByRoleType.get(roleType);
    return changedRelations;
  }
}
