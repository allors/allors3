import { RoleType, AssociationType, MetaPopulation } from '@allors/meta/system';
import { some } from '../iterators';
import { Association, Composite, Role } from './Types';

export class ChangeSet {
  readonly roleByAssociationByRoleType: Map<RoleType, Map<Composite, Role>>;
  readonly associationByRoleByRoleType: Map<AssociationType, Map<Composite, Association>>;

  constructor(
    public readonly meta: MetaPopulation,
    roleByAssociationByRoleType: Map<RoleType, Map<Composite, Role>>,
    associationByRoleByAssociationType: Map<AssociationType, Map<Composite, Association>>
  ) {
    this.roleByAssociationByRoleType = roleByAssociationByRoleType;
    this.associationByRoleByRoleType = associationByRoleByAssociationType;
  }

  hasChanges(): boolean {
    return (
      some(this.roleByAssociationByRoleType.values(), (v) => v.Value.Count > 0) ||
      some(this.associationByRoleByRoleType.values(), (v) => v.Value.Count > 0)
    );
  }

  ChangedRoles(roleType: RoleType): Map<Composite, Role> {
    const changedRelations = this.roleByAssociationByRoleType.get(roleType);
    return changedRelations;
  }
}
