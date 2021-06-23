import { PropertyType, RoleType } from '@allors/workspace/system';
import { add, difference, enumerate, has, Numbers, remove } from '../../collections/Numbers';
import { ChangeSet } from '../ChangeSet';
import { PropertyByObjectByPropertyType } from './PropertyByObjectByPropertyType';

export class SessionOriginState {
  private propertyByObjectByPropertyType: PropertyByObjectByPropertyType;

  public constructor() {
    this.propertyByObjectByPropertyType = new PropertyByObjectByPropertyType();
  }

  public Checkpoint(changeSet: ChangeSet) {
    changeSet.AddSessionStateChanges(this.propertyByObjectByPropertyType.Checkpoint());
  }

  public Get(object: number, propertyType: PropertyType): any {
    return this.propertyByObjectByPropertyType.Get(object, propertyType);
  }

  public SetUnitRole(association: number, roleType: RoleType, role: any) {
    this.propertyByObjectByPropertyType.Set(association, roleType, role);
  }

  public SetCompositeRole(association: number, roleType: RoleType, newRole: number) {
    if (newRole === null) {
      this.RemoveRole(association, roleType);
      return;
    }

    const associationType = roleType.associationType;
    //  Association
    const previousRole = this.propertyByObjectByPropertyType.Get(association, roleType) as number;
    if (previousRole != null) {
      this.propertyByObjectByPropertyType.Set(previousRole, associationType, null);
    }

    if (associationType.isOne) {
      //  OneToOne
      const previousAssociation = this.propertyByObjectByPropertyType.Get(newRole, associationType) as number;
      if (previousAssociation != null) {
        this.propertyByObjectByPropertyType.Set(previousAssociation, roleType, null);
      }
    }

    //  Role
    this.propertyByObjectByPropertyType.Set(association, roleType, newRole);
  }

  public SetCompositesRole(association: number, roleType: RoleType, newRole: any) {
    if (newRole == null) {
      this.RemoveRole(association, roleType);
      return;
    }

    const previousRole = this.Get(association, roleType) as Numbers;

    //  Use Diff (Add/Remove)
    const addedRoles = difference(newRole, previousRole);
    const removedRoles = difference(previousRole, newRole);

    for (const addedRole of enumerate(addedRoles)) {
      this.AddRole(association, roleType, addedRole);
    }

    for (const removedRole of enumerate(removedRoles)) {
      this.RemoveRole(association, roleType, removedRole);
    }
  }

  public AddRole(association: number, roleType: RoleType, roleToAdd: number) {
    const associationType = roleType.associationType;
    const previousRole = this.propertyByObjectByPropertyType.Get(association, roleType);
    if (has(previousRole, roleToAdd)) {
      return;
    }

    //  Role
    this.propertyByObjectByPropertyType.Set(association, roleType, add(previousRole, roleToAdd));
    //  Association
    if (associationType.isOne) {
      const previousRoleAssociations = this.propertyByObjectByPropertyType.Get(<number>previousRole, associationType);
      this.propertyByObjectByPropertyType.Set(<number>previousRole, associationType, remove(previousRoleAssociations, association));
    }

    const roleAssociations = this.propertyByObjectByPropertyType.Get(roleToAdd, associationType);
    this.propertyByObjectByPropertyType.Set(roleToAdd, associationType, add(roleAssociations, association));
  }

  private RemoveRole(association: number, roleType: RoleType, roleToRemove: number) {
    const associationType = roleType.associationType;
    const previousRole = this.propertyByObjectByPropertyType.Get(association, roleType);
    if (associationType.isOne) {
      if (previousRole == (roleToRemove as number)) {
        return;
      }

      //  Role
      this.propertyByObjectByPropertyType.Set(association, roleType, null);
      //  Association
      const removedRole = remove(previousRole, roleToRemove);
      this.propertyByObjectByPropertyType.Set(roleToRemove, associationType, removedRole);
    } else {
      if (!has(previousRole, roleToRemove)) {
        return;
      }

      //  Role
      const removedRole = remove(previousRole, roleToRemove);
      this.propertyByObjectByPropertyType.Set(association, roleType, removedRole);
      //  Association
      const previousAssociations = this.propertyByObjectByPropertyType.Get(roleToRemove, associationType);
      const removedAssociations = remove(previousAssociations, association);
      this.propertyByObjectByPropertyType.Set(roleToRemove, associationType, removedAssociations);
    }
  }

  private RemoveRole(association: number, roleType: RoleType) {
    if (roleType.objectType.isUnit) {
      //  Role
      this.SetUnitRole(association, roleType, null);
    } else {
      const previousRole = this.Get(association, roleType);
      if (roleType.isOne) {
        if (previousRole != null) {
          this.RemoveRole(association, roleType, <number>previousRole);
        }
      } else {
        for (const removeRole of enumerate(previousRole)) {
          this.RemoveRole(association, roleType, removeRole);
        }
      }
    }
  }
}
