import { IObject, IUnit } from '@allors/system/workspace/domain';
import { PropertyType, RoleType } from '@allors/system/workspace/meta';

import { IRange, Ranges } from '../../collections/ranges/ranges';
import { ChangeSet } from '../change-set';
import { PropertyByObjectByPropertyType } from './property-by-object-by-property-type';

export class SessionOriginState {
  private propertyByObjectByPropertyType: PropertyByObjectByPropertyType;

  public constructor(private ranges: Ranges<IObject>) {
    this.propertyByObjectByPropertyType = new PropertyByObjectByPropertyType(
      ranges
    );
  }

  public checkpoint(changeSet: ChangeSet) {
    changeSet.addSessionStateChanges(
      this.propertyByObjectByPropertyType.checkpoint()
    );
  }

  public getUnitRole(object: IObject, propertyType: PropertyType): IUnit {
    return this.propertyByObjectByPropertyType.get(
      object,
      propertyType
    ) as IUnit;
  }

  public setUnitRole(association: IObject, roleType: RoleType, role: IUnit) {
    this.propertyByObjectByPropertyType.set(association, roleType, role);
  }

  public getCompositeRole(
    object: IObject,
    propertyType: PropertyType
  ): IObject {
    return this.propertyByObjectByPropertyType.get(
      object,
      propertyType
    ) as IObject;
  }

  public setCompositeRole(
    association: IObject,
    roleType: RoleType,
    newRole: IObject
  ) {
    if (newRole == null) {
      if (roleType.associationType.isOne) {
        const roleId = this.getCompositeRole(association, roleType);
        if (roleId == null) {
          return;
        }

        this.removeCompositeRoleOne2One(association, roleType, roleId);
      } else {
        const roleId = this.getCompositeRole(association, roleType);
        if (roleId == null) {
          return;
        }

        this.removeCompositeRoleMany2One(association, roleType, roleId);
      }
    } else if (roleType.associationType.isOne) {
      this.setCompositeRoleOne2One(association, roleType, newRole);
    } else {
      this.setCompositeRoleMany2One(association, roleType, newRole);
    }
  }

  public getCompositesRole(
    object: IObject,
    propertyType: PropertyType
  ): IRange<IObject> {
    return this.propertyByObjectByPropertyType.get(
      object,
      propertyType
    ) as IRange<IObject>;
  }

  public addCompositesRole(
    association: IObject,
    roleType: RoleType,
    item: IObject
  ) {
    if (roleType.associationType.isOne) {
      this.addCompositesRoleOne2Many(association, roleType, item);
    } else {
      this.addCompositesRoleMany2Many(association, roleType, item);
    }
  }

  public removeCompositesRole(
    association: IObject,
    roleType: RoleType,
    item: IObject
  ) {
    if (roleType.associationType.isOne) {
      this.removeCompositesRoleOne2Many(association, roleType, item);
    } else {
      this.removeCompositesRoleMany2Many(association, roleType, item);
    }
  }

  public setCompositesRole(
    association: IObject,
    roleType: RoleType,
    newRole: IRange<IObject>
  ) {
    const previousRole = this.getCompositesRole(association, roleType);

    const addedRoles = this.ranges.difference(newRole, previousRole);
    const removedRoles = this.ranges.difference(previousRole, newRole);

    for (const addedRole of this.ranges.enumerate(addedRoles)) {
      this.addCompositesRole(association, roleType, addedRole);
    }

    for (const removedRole of this.ranges.enumerate(removedRoles)) {
      this.removeCompositesRole(association, roleType, removedRole);
    }
  }

  private setCompositeRoleOne2One(
    association: IObject,
    roleType: RoleType,
    role: IObject
  ) {
    /*  [if exist]        [then remove]        set
     *
     *  RA ----- R         RA --x-- R       RA    -- R       RA    -- R
     *                ->                +        -        =       -
     *   A ----- PR         A --x-- PR       A --    PR       A --    PR
     */
    const associationType = roleType.associationType;
    const previousRoleId = this.propertyByObjectByPropertyType.get(
      association,
      roleType
    ) as IObject;

    // R = PR
    if (role == previousRoleId) {
      return;
    }

    // A --x-- PR
    if (previousRoleId != null) {
      this.propertyByObjectByPropertyType.set(
        previousRoleId,
        associationType,
        null
      );
    }

    const roleAssociation = this.propertyByObjectByPropertyType.get(
      role,
      associationType
    ) as IObject;

    // RA --x-- R
    if (roleAssociation != null) {
      this.propertyByObjectByPropertyType.set(roleAssociation, roleType, null);
    }

    // A <---- R
    this.propertyByObjectByPropertyType.set(role, associationType, association);

    // A ----> R
    this.propertyByObjectByPropertyType.set(association, roleType, role);
  }

  private setCompositeRoleMany2One(
    association: IObject,
    roleType: RoleType,
    roleId: IObject
  ) {
    /*  [if exist]        [then remove]        set
     *
     *  RA ----- R         RA       R       RA    -- R       RA ----- R
     *                ->                +        -        =       -
     *   A ----- PR         A --x-- PR       A --    PR       A --    PR
     */
    const associationType = roleType.associationType;
    const previousRoleId = this.propertyByObjectByPropertyType.get(
      association,
      roleType
    ) as IObject;

    // R = PR
    if (roleId == previousRoleId) {
      return;
    }

    // A --x-- PR
    if (previousRoleId != null) {
      this.removeCompositeRoleMany2One(association, roleType, roleId);
    }

    // A <---- R
    let associationIds = this.getCompositesRole(roleId, associationType);
    associationIds = this.ranges.add(associationIds, association);
    this.propertyByObjectByPropertyType.set(
      roleId,
      associationType,
      associationIds
    );

    // A ----> R
    this.propertyByObjectByPropertyType.set(association, roleType, roleId);
  }

  private removeCompositeRoleOne2One(
    association: IObject,
    roleType: RoleType,
    role: IObject
  ) {
    /*                        delete
     *
     *   A ----- R    ->     A       R  =   A       R
     */

    // A <---- R
    this.propertyByObjectByPropertyType.set(
      role,
      roleType.associationType,
      null
    );

    // A ----> R
    this.propertyByObjectByPropertyType.set(association, roleType, null);
  }

  private removeCompositeRoleMany2One(
    association: IObject,
    roleType: RoleType,
    role: IObject
  ) {
    /*                        delete
     *  RA --                                RA --
     *       -        ->                 =        -
     *   A ----- R           A --x-- R             -- R
     */
    const associationType = roleType.associationType;

    // A <---- R
    let roleAssociations = this.getCompositesRole(role, associationType);
    roleAssociations = this.ranges.remove(roleAssociations, association);
    this.propertyByObjectByPropertyType.set(
      role,
      associationType,
      roleAssociations
    );

    // A ----> R
    this.propertyByObjectByPropertyType.set(association, roleType, null);
  }

  private addCompositesRoleOne2Many(
    association: IObject,
    roleType: RoleType,
    role: IObject
  ) {
    /*  [if exist]        [then remove]        set
     *
     *  RA ----- R         RA       R       RA    -- R       RA ----- R
     *                ->                +        -        =       -
     *   A ----- PR         A --x-- PR       A --    PR       A --    PR
     */
    const associationType = roleType.associationType;
    const previousRoleId = this.getCompositesRole(association, roleType);

    // R in PR
    if (this.ranges.has(previousRoleId, role)) {
      return;
    }

    // A --x-- PR
    const previousAssociationId = this.getCompositeRole(role, associationType);
    if (previousAssociationId != null) {
      this.removeCompositesRoleOne2Many(previousAssociationId, roleType, role);
    }

    // A <---- R
    this.propertyByObjectByPropertyType.set(role, associationType, role);

    // A ----> R
    let roleIds = this.getCompositesRole(association, roleType);
    roleIds = this.ranges.add(roleIds, role);
    this.propertyByObjectByPropertyType.set(association, roleType, roleIds);
  }

  private addCompositesRoleMany2Many(
    association: IObject,
    roleType: RoleType,
    role: IObject
  ) {
    /*  [if exist]        [no remove]         set
     *
     *  RA ----- R         RA       R       RA    -- R       RA ----- R
     *                ->                +        -        =       -
     *   A ----- PR         A       PR       A --    PR       A --    PR
     */
    const associationType = roleType.associationType;
    const previousRoleIds = this.getCompositesRole(association, roleType);

    // R in PR
    if (this.ranges.has(previousRoleIds, role)) {
      return;
    }

    // A <---- R
    let associationIds = this.getCompositesRole(role, associationType);
    associationIds = this.ranges.add(associationIds, association);
    this.propertyByObjectByPropertyType.set(
      role,
      associationType,
      associationIds
    );

    // A ----> R
    let roleIds = this.getCompositesRole(association, roleType);
    roleIds = this.ranges.add(roleIds, role);
    this.propertyByObjectByPropertyType.set(association, roleType, roleIds);
  }

  private removeCompositesRoleOne2Many(
    association: IObject,
    roleType: RoleType,
    role: IObject
  ) {
    const associationType = roleType.associationType;
    const previousRoleIds = this.getCompositesRole(association, roleType);

    // R not in PR
    if (!this.ranges.has(previousRoleIds, role)) {
      return;
    }

    // A <---- R
    this.propertyByObjectByPropertyType.set(role, associationType, null);

    // A ----> R
    let roleIds = this.getCompositesRole(association, roleType);
    roleIds = this.ranges.add(roleIds, role);
    this.propertyByObjectByPropertyType.set(association, roleType, roleIds);
  }

  private removeCompositesRoleMany2Many(
    association: IObject,
    roleType: RoleType,
    role: IObject
  ) {
    const associationType = roleType.associationType;
    const previousRoleIds = this.getCompositesRole(association, roleType);

    // R not in PR
    if (!this.ranges.has(previousRoleIds, role)) {
      return;
    }

    // A <---- R
    let associationIds = this.getCompositesRole(role, associationType);
    associationIds = this.ranges.remove(associationIds, association);
    this.propertyByObjectByPropertyType.set(
      role,
      associationType,
      associationIds
    );

    // A ----> R
    let roleIds = this.getCompositesRole(association, roleType);
    roleIds = this.ranges.remove(roleIds, role);
    this.propertyByObjectByPropertyType.set(association, roleType, roleIds);
  }
}
