import { RoleType, AssociationType, MetaPopulation } from '@allors/meta/system';
import { ChangeSet } from './ChangeSet';
import { except, forEach } from '../iterators';

import { Composite, Association, Role } from './Types'

export class Relations {
  private readonly roleByAssociationByRoleType: Map<RoleType, Map<Composite, Role>>;
  private readonly associationByRoleByAssociationType: Map<AssociationType, Map<Composite, Association>>;

  private changedRoleByAssociationByRoleType: Map<RoleType, Map<Composite, Role>>;
  private changedAssociationByRoleByAssociationType: Map<AssociationType, Map<Composite, Association>>;

  constructor(private readonly meta: MetaPopulation) {
    this.roleByAssociationByRoleType = new Map();
    this.associationByRoleByAssociationType = new Map();

    this.changedRoleByAssociationByRoleType = new Map();
    this.changedAssociationByRoleByAssociationType = new Map();
  }

  Snapshot(): ChangeSet {
    this.changedRoleByAssociationByRoleType.forEach((changedRoleByAssociation, roleType) => {
      const roleByAssociation = this.roleByAssociation(roleType);

      changedRoleByAssociation.forEach((changedRole, association) => {
        const originalRole = roleByAssociation.get(association);
        if (this.areSame(roleType.isOne, originalRole, changedRole)) {
          changedRoleByAssociation.delete(association);
        } else {
          roleByAssociation.set(association, changedRole);
        }
      });

      if (roleByAssociation.size == 0) {
        this.changedRoleByAssociationByRoleType.delete(roleType);
      }
    });

    this.changedAssociationByRoleByAssociationType.forEach((changedAssociationByRole, associationType) => {
      const associationByRole = this.associationByRole(associationType);
      changedAssociationByRole.forEach((changedAssociation, role) => {
        const originalAssociation = associationByRole.get(role);
        if (this.areSame(associationType.isOne, originalAssociation, changedAssociation)) {
          changedAssociationByRole.delete(role);
        } else {
          associationByRole.set(role, changedAssociation);
        }
      });
    });

    const snapshot = new ChangeSet(this.meta, this.changedRoleByAssociationByRoleType, this.changedAssociationByRoleByAssociationType);

    this.changedRoleByAssociationByRoleType = new Map();
    this.changedAssociationByRoleByAssociationType = new Map();

    return snapshot;
  }

  getRole(association: string, roleType: RoleType): any {
    const changedRole = this.changedRoleByAssociationByRoleType.get(roleType)?.get(association);
    if (changedRole !== undefined) {
      return changedRole;
    }

    return this.roleByAssociation(roleType).get(association);
  }

  setRole(association: string, roleType: RoleType, role: any): void {
    // TODO: var normalizedRole = roleType.Normalize(role);
    const normalizedRole = role;

    if (roleType.objectType.isUnit) {
      // Role
      this.changedRoleByAssociation(roleType).set(association, normalizedRole);
    } else {
      const associationType = roleType.associationType;
      const previousRole = this.getRole(association, roleType);
      if (roleType.isOne) {
        const roleObject = normalizedRole;
        const previousAssociation = this.getAssociation(roleObject, associationType);

        // Role
        const changedRoleByAssociation = this.changedRoleByAssociation(roleType);
        changedRoleByAssociation[association] = roleObject;

        // Association
        const changedAssociationByRole = this.changedAssociationByRole(associationType);
        if (associationType.isOne) {
          // One to One
          const previousAssociationObject = previousAssociation;
          if (previousAssociationObject != null) {
            changedRoleByAssociation.set(previousAssociationObject, null);
          }

          if (previousRole != null) {
            const previousRoleObject = previousRole;
            changedAssociationByRole.set(previousRoleObject, null);
          }

          changedAssociationByRole.set(roleObject, association);
        } else {
          // TODO: Optimize
          const newAssociation = new Set(previousAssociation);
          newAssociation.delete(roleObject);
          changedAssociationByRole.set(roleObject, newAssociation);
        }
      } else {
        const roles = (normalizedRole as Set<string>) ?? new Set<string>();
        const previousRoles = (previousRole as Set<string>) ?? new Set<string>();

        // Use Diff (Add/Remove)
        forEach(except(roles, previousRoles), (role) => {
          this.addRole(association, roleType, role);
        });

        forEach(except(previousRoles, roles), (role) => {
          this.removeRole(association, roleType, role);
        });
      }
    }
  }

  addRole(association: string, roleType: RoleType, role: string) {
    var associationType = roleType.associationType;
    const previousAssociation = this.getAssociation(role, associationType);
    // Role
    var changedRoleByAssociation = this.changedRoleByAssociation(roleType);
    const previousRole = this.getRole(association, roleType);
    var roleArray = previousRole;
    roleArray = NullableArraySet.Add(roleArray, role);
    changedRoleByAssociation[association] = roleArray;
    // Association
    var changedAssociationByRole = this.ChangedAssociationByRole(associationType);
    if (associationType.IsOne)
    {
        // One to Many
        var previousAssociationObject = (DynamicObject)previousAssociation;
        if (previousAssociationObject != null)
        {
            this.GetRole(previousAssociationObject, roleType, out var previousAssociationRole);
            changedRoleByAssociation[previousAssociationObject] = NullableArraySet.Remove(previousAssociationRole, role);
        }
        changedAssociationByRole[role] = association;
    }
    else
    {
        // Many to Many
        changedAssociationByRole[role] = NullableArraySet.Add(previousAssociation, association);
    }
  }

  removeRole(association: string, roleType: RoleType, role: string): void {
    // var associationType = roleType.AssociationType;
    // this.GetAssociation(role, associationType, out var previousAssociation);
    // this.GetRole(association, roleType, out var previousRole);
    // if (previousRole != null)
    // {
    //     // Role
    //     var changedRoleByAssociation = this.ChangedRoleByAssociation(roleType);
    //     changedRoleByAssociation[association] = NullableArraySet.Remove(previousRole, role);
    //     // Association
    //     var changedAssociationByRole = this.ChangedAssociationByRole(associationType);
    //     if (associationType.IsOne)
    //     {
    //         // One to Many
    //         changedAssociationByRole[role] = null;
    //     }
    //     else
    //     {
    //         // Many to Many
    //         changedAssociationByRole[role] = NullableArraySet.Add(previousAssociation, association);
    //     }
    // }
  }

  getAssociation(role: string, associationType: AssociationType): any {
    const changedAssociation = this.changedAssociationByRoleByAssociationType.get(associationType)?.get(role);
    if (changedAssociation !== undefined) {
      return changedAssociation;
    }

    return this.associationByRole(associationType).get(role);
  }

  private areSame(isOne: boolean, a: any, b: any): boolean {
    if (isOne) {
      return a === b;
    } else {
      if (a == null && b == null) {
        return true;
      } else {
        if (a == null || b == null) {
          return false;
        } else {
          return a.size === b.size && [...a].every((value) => b.has(value));
        }
      }
    }
  }

  private associationByRole(asscociationType: AssociationType) {
    let associationByRole = this.associationByRoleByAssociationType.get(asscociationType);

    if (associationByRole === undefined) {
      associationByRole = new Map();
      this.associationByRoleByAssociationType.set(asscociationType, associationByRole);
    }

    return associationByRole;
  }

  private roleByAssociation(roleType: RoleType) {
    let roleByAssociation = this.roleByAssociationByRoleType.get(roleType);

    if (roleByAssociation === undefined) {
      roleByAssociation = new Map();
      this.roleByAssociationByRoleType.set(roleType, roleByAssociation);
    }

    return roleByAssociation;
  }

  private changedAssociationByRole(associationType: AssociationType) {
    let changedAssociationByRole = this.changedAssociationByRoleByAssociationType.get(associationType);

    if (changedAssociationByRole === undefined) {
      changedAssociationByRole = new Map();
      this.changedAssociationByRoleByAssociationType.set(associationType, changedAssociationByRole);
    }

    return changedAssociationByRole;
  }

  private changedRoleByAssociation(roleType: RoleType) {
    let changedRoleByAssociation = this.changedRoleByAssociationByRoleType.get(roleType);

    if (changedRoleByAssociation === undefined) {
      changedRoleByAssociation = new Map();
      this.changedRoleByAssociationByRoleType.set(roleType, changedRoleByAssociation);
    }

    return changedRoleByAssociation;
  }
}
