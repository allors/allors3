import { IChangeSet, IStrategy } from '@allors/workspace/system';
import { AssociationType, PropertyType, RelationType, RoleType } from '@allors/workspace/system';
import { Session } from './Session';
import { Strategy } from './Strategy';

export class ChangeSet implements IChangeSet {
  associationsByRoleType: Map<RoleType, Set<IStrategy>>;

  rolesByAssociationType: Map<AssociationType, Set<IStrategy>>;

  public constructor(public Session: Session, public Created: Set<IStrategy>, public Instantiated: Set<IStrategy>) {
    this.associationsByRoleType = new Map();
    this.rolesByAssociationType = new Map();
  }

  public AddSessionStateChanges(sessionStateChangeSet: Map<PropertyType, Map<number, any>>) {
    sessionStateChangeSet.forEach((map, propertyType) => {
      const ids = map.keys;

      const strategies = new Set<IStrategy>(this.Session.GetStrategies(ids));

      if (propertyType.isAssociationType) {
        this.rolesByAssociationType.set(propertyType as AssociationType, strategies);
      } else if (propertyType.isRoleType) {
        this.associationsByRoleType.set(propertyType as RoleType, strategies);
      }
    });
  }

  public Diff(association: Strategy, relationType: RelationType, current: any, previous: any) {
    const roleType = relationType.roleType;
    if (roleType.objectType.isUnit) {
      if (current !== previous) {
        this.AddAssociation(relationType, association);
      }
    } else if (roleType.isOne) {
      if (current === previous) {
        return;
      }

      if (previous != null) {
        this.AddRole(relationType, this.Session.getStrategy(<number>previous));
      }

      if (current != null) {
        this.AddRole(relationType, this.Session.getStrategy(<number>current));
      }

      this.AddAssociation(relationType, association);
    } else {
      const numbers = this.Session.workspace.Numbers;
      let hasChange = false;
      const addedRoles = numbers.Except(current, previous);
      for (const v in numbers.Enumerate(addedRoles)) {
        this.AddRole(relationType, this.Session.getStrategy(v));
        hasChange = true;
      }

      const removedRoles = numbers.Except(previous, current);
      for (const v in numbers.Enumerate(removedRoles)) {
        this.AddRole(relationType, this.Session.getStrategy(v));
        hasChange = true;
      }

      if (hasChange) {
        this.AddAssociation(relationType, association);
      }
    }
  }

  private AddAssociation(relationType: RelationType, association: Strategy) {
    const roleType = relationType.roleType;

    let associations: Set<IStrategy>;
    if (!this.associationsByRoleType.has(roleType)) {
      associations = new Set();
      this.associationsByRoleType.set(roleType, associations);
    }

    associations.add(association);
  }

  private AddRole(relationType: RelationType, role: Strategy) {
    const associationType = relationType.AssociationType;

    let roles: Set<IStrategy>;
    if (!this.rolesByAssociationType.has(associationType)) {
      roles = new Set();
      this.rolesByAssociationType.set(associationType, roles);
    }

    roles.add(role);
  }
}
