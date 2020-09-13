import { RoleType, AssociationType, MetaPopulation } from '@allors/meta/system';
import { ChangeSet } from './ChangeSet';
import { forEach } from '../combinators';

import { Composite, Association, Role } from './Types';

function isComposite(arg: Role | Association): arg is Composite {
  return arg == null || typeof arg === 'string' || arg instanceof String;
}

function isComposites(arg: Role | Association): arg is Composite[] {
  return arg == null || arg instanceof Array;
}

function areSame(isOne: boolean, a: any, b: any): boolean {
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
        if (areSame(roleType.isOne, originalRole, changedRole)) {
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
        if (areSame(associationType.isOne, originalAssociation, changedAssociation)) {
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

  getRole(association: Composite, roleType: RoleType): Role {
    const changedRole = this.changedRoleByAssociationByRoleType.get(roleType)?.get(association);
    if (changedRole !== undefined) {
      return changedRole;
    }

    return this.roleByAssociation(roleType).get(association);
  }

  setRole(association: Composite, roleType: RoleType, role: Role): void {
    if (roleType.objectType.isUnit) {
      // Role
      this.changedRoleByAssociation(roleType).set(association, role);
    } else {
      const associationType = roleType.associationType;
      const previousRole = this.getRole(association, roleType);

      if (roleType.isOne) {
        if (!isComposite(role)) {
          throw new Error(`${role} is not a Composite`);
        }
        if (!isComposite(previousRole)) {
          throw new Error(`${previousRole} is not a Composite`);
        }
        const previousAssociation = this.getAssociation(role, associationType);

        // Role
        const changedRoleByAssociation = this.changedRoleByAssociation(roleType);
        changedRoleByAssociation[association] = role;

        // Association
        const changedAssociationByRole = this.changedAssociationByRole(associationType);
        if (associationType.isOne) {
          // One to One
          if (!isComposite(previousAssociation)) {
            throw new Error(`${role} is not a Composite`);
          }

          if (previousAssociation != null) {
            changedRoleByAssociation.set(previousAssociation, null);
          }

          if (previousRole != null) {
            changedAssociationByRole.set(previousRole, null);
          }

          changedAssociationByRole.set(role, association);
        } else {
          // Many to One
          if (!isComposites(previousAssociation)) {
            throw new Error(`${previousAssociation} are not a Composites`);
          }

          changedAssociationByRole.set(
            role,
            previousAssociation?.filter((v) => v !== role)
          ) ?? null;
        }
      } else {
        if (!isComposites(role)) {
          throw new Error(`${role} is not a Composite`);
        }
        if (!isComposites(previousRole)) {
          throw new Error(`${previousRole} is not a Composite`);
        }

        // Use Diff (Add/Remove)
        const addedRoles = role.filter((v) => !previousRole.includes(v));
        if (addedRoles?.length > 0) {
          forEach(addedRoles, (v) => {
            this.addRole(association, roleType, v);
          });
        }

        const removedRoles = previousRole.filter((v) => !role.includes(v));
        if (removedRoles?.length > 0) {
          forEach(removedRoles, (v) => {
            this.removeRole(association, roleType, v);
          });
        }
      }
    }
  }

  addRole(association: Composite, roleType: RoleType, role: Composite) {
    const previousRole = this.getRole(association, roleType);
    if (!isComposites(previousRole)) {
      throw new Error(`${previousRole} is not a Composites`);
    }

    if (previousRole?.some((v) => v === role)) {
      return;
    }

    const associationType = roleType.associationType;
    const previousAssociation = this.getAssociation(role, associationType);

    // Role
    const changedRoleByAssociation = this.changedRoleByAssociation(roleType);
    const roleArray = previousRole != null ? previousRole.concat(role) : [role];
    changedRoleByAssociation[association] = roleArray;

    // Association
    const changedAssociationByRole = this.changedAssociationByRole(associationType);
    if (associationType.isOne) {
      // One to Many
      if (!isComposite(previousAssociation)) {
        throw new Error(`${previousAssociation} is not a Composite`);
      }

      if (previousAssociation != null) {
        const previousAssociationRole = this.getRole(previousAssociation, roleType);
        if (!isComposites(previousAssociationRole)) {
          throw new Error(`${previousAssociationRole} are not a Composites`);
        }

        changedRoleByAssociation[previousAssociation] = previousAssociationRole?.filter((v) => v !== role) ?? null;
      }

      changedAssociationByRole[role] = association;
    } else {
      if (!isComposites(previousAssociation)) {
        throw new Error(`${previousAssociation} are not a Composites`);
      }

      // Many to Many
      changedAssociationByRole[role] = previousAssociation != null ? previousAssociation.concat(association) : [association];
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

  getAssociation(role: Role, associationType: AssociationType): Association {
    const changedAssociation = this.changedAssociationByRoleByAssociationType.get(associationType)?.get(role);
    if (changedAssociation !== undefined) {
      return changedAssociation;
    }

    return this.associationByRole(associationType).get(role);
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
