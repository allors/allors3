import { RoleType, ObjectType, AssociationType, MetaPopulation } from '@allors/meta/system';

import { ChangeSet } from './ChangeSet';

export class Population {
  private readonly roleByAssociationByRoleType: Map<RoleType, Map<string, any>>;
  private readonly associationByRoleByAssociationType: Map<AssociationType, Map<string, any>>;

  private changedRoleByAssociationByRoleType: Map<RoleType, Map<string, any>>;
  private changedAssociationByRoleByAssociationType: Map<AssociationType, Map<string, any>>;

  private objects: string[];

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

  addObject(newObject: string): void {
    this.objects.push(newObject);
  }

  getRole(association: string, roleType: RoleType): any {
    const changedRole = this.changedRoleByAssociationByRoleType.get(roleType)?.get(association);
    if (changedRole !== undefined) {
      return changedRole;
    }

    return this.roleByAssociation(roleType).get(association);
  }

  // setRole(association: string,  roleType: RoleType, role: any): void
  // {
  //   // TODO: var normalizedRole = roleType.Normalize(role);
  //   const normalizedRole = role;
    
  //     if (roleType.isUnit)
  //     {
  //         // Role
  //         this.changedRoleByAssociation(roleType).set(association, normalizedRole);
  //     }
  //     else
  //     {
  //         var associationType = roleType.associationType;
  //         this.GetRole(association, roleType, out object previousRole);
  //         if (roleType.IsOne)
  //         {
  //             var roleObject = (DynamicObject)normalizedRole;
  //             this.GetAssociation(roleObject, associationType, out var previousAssociation);

  //             // Role
  //             var changedRoleByAssociation = this.ChangedRoleByAssociation(roleType);
  //             changedRoleByAssociation[association] = roleObject;

  //             // Association
  //             var changedAssociationByRole = this.ChangedAssociationByRole(associationType);
  //             if (associationType.IsOne)
  //             {
  //                 // One to One
  //                 var previousAssociationObject = (DynamicObject)previousAssociation;
  //                 if (previousAssociationObject != null)
  //                 {
  //                     changedRoleByAssociation[previousAssociationObject] = null;
  //                 }

  //                 if (previousRole != null)
  //                 {
  //                     var previousRoleObject = (DynamicObject)previousRole;
  //                     changedAssociationByRole[previousRoleObject] = null;
  //                 }

  //                 changedAssociationByRole[roleObject] = association;
  //             }
  //             else
  //             {
  //                 changedAssociationByRole[roleObject] = NullableArraySet.Remove(previousAssociation, roleObject);
  //             }
  //         }
  //         else
  //         {
  //             var roles = ((IEnumerable<DynamicObject>)normalizedRole)?.ToArray() ?? Array.Empty<DynamicObject>();
  //             var previousRoles = (DynamicObject[])previousRole ?? Array.Empty<DynamicObject>();

  //             // Use Diff (Add/Remove)
  //             var addedRoles = roles.Except(previousRoles);
  //             var removedRoles = previousRoles.Except(roles);

  //             foreach (var addedRole in addedRoles)
  //             {
  //                 this.AddRole(association, roleType, addedRole);
  //             }

  //             foreach (var removeRole in removedRoles)
  //             {
  //                 this.RemoveRole(association, roleType, removeRole);
  //             }
  //         }
  //     }
  // }

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

  private associationByRole(asscociationType: AssociationType): Map<string, object> {
    let associationByRole = this.associationByRoleByAssociationType.get(asscociationType);

    if (associationByRole === undefined) {
      associationByRole = new Map();
      this.associationByRoleByAssociationType.set(asscociationType, associationByRole);
    }

    return associationByRole;
  }

  private roleByAssociation(roleType: RoleType): Map<any, object> {
    let roleByAssociation = this.roleByAssociationByRoleType.get(roleType);

    if (roleByAssociation === undefined) {
      roleByAssociation = new Map();
      this.roleByAssociationByRoleType.set(roleType, roleByAssociation);
    }

    return roleByAssociation;
  }

  private changedAssociationByRole(associationType: AssociationType): Map<string, any> {
    let changedAssociationByRole = this.changedAssociationByRoleByAssociationType.get(associationType);

    if (changedAssociationByRole === undefined) {
      changedAssociationByRole = new Map();
      this.changedAssociationByRoleByAssociationType.set(associationType, changedAssociationByRole);
    }

    return changedAssociationByRole;
  }

  private changedRoleByAssociation(roleType: RoleType): Map<string, any> {
    let changedRoleByAssociation = this.changedRoleByAssociationByRoleType.get(roleType);

    if (changedRoleByAssociation === undefined) {
      changedRoleByAssociation = new Map();
      this.changedRoleByAssociationByRoleType.set(roleType, changedRoleByAssociation);
    }

    return changedRoleByAssociation;
  }
}
