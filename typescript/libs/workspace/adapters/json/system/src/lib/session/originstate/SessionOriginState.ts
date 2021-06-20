import { PropertyType, RoleType } from '@allors/workspace/meta/system';
import { MapMap } from '../../collections/MapMap';

export class SessionOriginState {
  private propertyByObjectByPropertyType: MapMap<PropertyType, number, any>;

  public constructor() {
    this.propertyByObjectByPropertyType = new MapMap();
  }

  public Checkpoint(changeSet: ChangeSet) {
    changeSet.AddSessionStateChanges(this.propertyByObjectByPropertyType.checkpoint())
  }

  public Get(object: number, propertyType: IPropertyType): Object {}

  public SetUnitRole(association: number, roleType: RoleType, role: Object) {}

  public SetCompositeRole(association: number, roleType: RoleType, newRole: ?long) {
    if (newRole == null) {
      this.RemoveRole(association, roleType);
      return;
    }

    let associationType = roleType.AssociationType;
    //  Association
    let previousRole = <?long>this.propertyByObjectByPropertyType.Get(association, roleType);
    if (previousRole.HasValue) {
      this.propertyByObjectByPropertyType.Set(previousRole.Value, associationType, null);
    }

    if (associationType.IsOne) {
      //  OneToOne
      let previousAssociation = <?long>this.propertyByObjectByPropertyType.Get(newRole.Value, associationType);
      if (previousAssociation.HasValue) {
        this.propertyByObjectByPropertyType.Set(previousAssociation.Value, roleType, null);
      }
    }

    //  Role
    this.propertyByObjectByPropertyType.Set(association, roleType, newRole);
  }

  public SetCompositesRole(association: number, roleType: RoleType, newRole: Object) {
    if (newRole == null) {
      this.RemoveRole(association, roleType);
      return;
    }

    let previousRole = <number[]>this.Get(association, roleType);
    //  Use Diff (Add/Remove)
    let addedRoles = this.numbers.Except(newRole, previousRole);
    let removedRoles = this.numbers.Except(previousRole, newRole);
    for (let addedRole in this.numbers.Enumerate(addedRoles)) {
      this.AddRole(association, roleType, addedRole);
    }

    for (let removedRole in this.numbers.Enumerate(removedRoles)) {
      this.RemoveRole(association, roleType, removedRole);
    }
  }

  public AddRole(association: number, roleType: RoleType, roleToAdd: number) {
    let associationType = roleType.AssociationType;
    let previousRole = this.propertyByObjectByPropertyType.Get(association, roleType);
    if (this.numbers.Contains(previousRole, roleToAdd)) {
      return;
    }

    //  Role
    this.propertyByObjectByPropertyType.Set(association, roleType, this.numbers.Add(previousRole, roleToAdd));
    //  Association
    if (associationType.IsOne) {
      let previousRoleAssociations = this.propertyByObjectByPropertyType.Get(<number>previousRole, associationType);
      this.propertyByObjectByPropertyType.Set(<number>previousRole, associationType, this.numbers.Remove(previousRoleAssociations, association));
    }

    let roleAssociations = this.propertyByObjectByPropertyType.Get(roleToAdd, associationType);
    this.propertyByObjectByPropertyType.Set(roleToAdd, associationType, this.numbers.Add(roleAssociations, association));
  }

  private RemoveRole(association: number, roleType: RoleType, roleToRemove: number) {
    let associationType = roleType.AssociationType;
    let previousRole = this.propertyByObjectByPropertyType.Get(association, roleType);
    if (associationType.IsOne) {
      if (<?long>previousRole == roleToRemove) {
        return;
      }

      //  Role
      this.propertyByObjectByPropertyType.Set(association, roleType, null);
      //  Association
      let removedRole = this.numbers.Remove(previousRole, roleToRemove);
      this.propertyByObjectByPropertyType.Set(roleToRemove, associationType, removedRole);
    } else {
      if (!this.numbers.Contains(previousRole, roleToRemove)) {
        return;
      }

      //  Role
      let removedRole = this.numbers.Remove(previousRole, roleToRemove);
      this.propertyByObjectByPropertyType.Set(association, roleType, removedRole);
      //  Association
      let previousAssociations = this.propertyByObjectByPropertyType.Get(roleToRemove, associationType);
      let removedAssociations = this.numbers.Remove(previousAssociations, association);
      this.propertyByObjectByPropertyType.Set(roleToRemove, associationType, removedAssociations);
    }
  }

  private RemoveRole(association: number, roleType: RoleType) {
    if (roleType.ObjectType.IsUnit) {
      //  Role
      this.SetUnitRole(association, roleType, null);
    } else {
      let previousRole = this.Get(association, roleType);
      if (roleType.IsOne) {
        if (previousRole != null) {
          this.RemoveRole(association, roleType, <number>previousRole);
        }
      } else {
        for (let removeRole in this.numbers.Enumerate(previousRole)) {
          this.RemoveRole(association, roleType, removeRole);
        }
      }
    }
  }
}
