import { IUnit } from '@allors/workspace/domain/system';
import { PropertyType, RoleType } from '@allors/workspace/meta/system';
import { add, difference, enumerate, has, IRange, remove } from '../../collections/Range';
import { ChangeSet } from '../ChangeSet';
import { PropertyByObjectByPropertyType } from './PropertyByObjectByPropertyType';

export class SessionOriginState {
  private propertyByObjectByPropertyType: PropertyByObjectByPropertyType;

  public constructor() {
    this.propertyByObjectByPropertyType = new PropertyByObjectByPropertyType();
  }

  public checkpoint(changeSet: ChangeSet) {
    changeSet.addSessionStateChanges(this.propertyByObjectByPropertyType.checkpoint());
  }

  public getUnitRole(object: number, propertyType: PropertyType): IUnit {
    return this.propertyByObjectByPropertyType.get(object, propertyType) as IUnit;
  }

  public setUnitRole(association: number, roleType: RoleType, role: IUnit) {
    this.propertyByObjectByPropertyType.set(association, roleType, role);
  }

  public getCompositeRole(object: number, propertyType: PropertyType): number {
    return this.propertyByObjectByPropertyType.get(object, propertyType) as number;
  }

  public setCompositeRole(association: number, roleType: RoleType, newRole: number) {
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

  public getCompositesRole(object: number, propertyType: PropertyType): IRange {
    return this.propertyByObjectByPropertyType.get(object, propertyType) as IRange;
  }

  public addCompositesRole(association: number, roleType: RoleType, item: number) {
    if (roleType.associationType.isOne) {
      this.addCompositesRoleOne2Many(association, roleType, item);
    } else {
      this.addCompositesRoleMany2Many(association, roleType, item);
    }
  }

  public removeCompositesRole(association: number, roleType: RoleType, item: number) {
    if (roleType.associationType.isOne) {
      this.removeCompositesRoleOne2Many(association, roleType, item);
    } else {
      this.removeCompositesRoleMany2Many(association, roleType, item);
    }
  }

  public setCompositesRole(association: number, roleType: RoleType, newRole: IRange) {
    const previousRole = this.getCompositesRole(association, roleType);

    const addedRoles = difference(newRole, previousRole);
    const removedRoles = difference(previousRole, newRole);

    for (const addedRole of enumerate(addedRoles)) {
      this.addCompositesRole(association, roleType, addedRole);
    }

    for (const removedRole of enumerate(removedRoles)) {
      this.removeCompositesRole(association, roleType, removedRole);
    }
  }

  private setCompositeRoleOne2One(associationId: number, roleType: RoleType, roleId: number) {
    /*  [if exist]        [then remove]        set
     *
     *  RA ----- R         RA --x-- R       RA    -- R       RA    -- R
     *                ->                +        -        =       -
     *   A ----- PR         A --x-- PR       A --    PR       A --    PR
     */
    const associationType = roleType.associationType;
    const previousRoleId = this.propertyByObjectByPropertyType.get(associationId, roleType) as number;

    // R = PR
    if (roleId == previousRoleId) {
      return;
    }

    // A --x-- PR
    if (previousRoleId != null) {
      this.propertyByObjectByPropertyType.set(previousRoleId, associationType, null);
    }

    const roleAssociation = this.propertyByObjectByPropertyType.get(roleId, associationType) as number;

    // RA --x-- R
    if (roleAssociation != null) {
      this.propertyByObjectByPropertyType.set(roleAssociation, roleType, null);
    }

    // A <---- R
    this.propertyByObjectByPropertyType.set(roleId, associationType, associationId);

    // A ----> R
    this.propertyByObjectByPropertyType.set(associationId, roleType, roleId);
  }

  private setCompositeRoleMany2One(associationId: number, roleType: RoleType, roleId: number) {
    /*  [if exist]        [then remove]        set
     *
     *  RA ----- R         RA       R       RA    -- R       RA ----- R
     *                ->                +        -        =       -
     *   A ----- PR         A --x-- PR       A --    PR       A --    PR
     */
    const associationType = roleType.associationType;
    const previousRoleId = this.propertyByObjectByPropertyType.get(associationId, roleType) as number;

    // R = PR
    if (roleId == previousRoleId) {
      return;
    }

    // A --x-- PR
    if (previousRoleId != null) {
      this.removeCompositeRoleMany2One(associationId, roleType, roleId);
    }

    // A <---- R
    let associationIds = this.getCompositesRole(roleId, associationType);
    associationIds = add(associationIds, associationId);
    this.propertyByObjectByPropertyType.set(roleId, associationType, associationIds);

    // A ----> R
    this.propertyByObjectByPropertyType.set(associationId, roleType, roleId);
  }

  private removeCompositeRoleOne2One(associationId: number, roleType: RoleType, roleId: number) {
    /*                        delete
     *
     *   A ----- R    ->     A       R  =   A       R
     */

    // A <---- R
    this.propertyByObjectByPropertyType.set(roleId, roleType.associationType, null);

    // A ----> R
    this.propertyByObjectByPropertyType.set(associationId, roleType, null);
  }

  private removeCompositeRoleMany2One(associationId: number, roleType: RoleType, roleId: number) {
    /*                        delete
     *  RA --                                RA --
     *       -        ->                 =        -
     *   A ----- R           A --x-- R             -- R
     */
    const associationType = roleType.associationType;

    // A <---- R
    let roleAssociations = this.getCompositesRole(roleId, associationType);
    roleAssociations = remove(roleAssociations, associationId);
    this.propertyByObjectByPropertyType.set(roleId, associationType, roleAssociations);

    // A ----> R
    this.propertyByObjectByPropertyType.set(associationId, roleType, null);
  }

  private addCompositesRoleOne2Many(associationId: number, roleType: RoleType, roleId: number) {
    /*  [if exist]        [then remove]        set
     *
     *  RA ----- R         RA       R       RA    -- R       RA ----- R
     *                ->                +        -        =       -
     *   A ----- PR         A --x-- PR       A --    PR       A --    PR
     */
    const associationType = roleType.associationType;
    const previousRoleId = this.getCompositesRole(associationId, roleType);

    // R in PR
    if (has(previousRoleId, roleId)) {
      return;
    }

    // A --x-- PR
    const previousAssociationId = this.getCompositeRole(roleId, associationType);
    if (previousAssociationId != null) {
      this.removeCompositesRoleOne2Many(previousAssociationId, roleType, roleId);
    }

    // A <---- R
    this.propertyByObjectByPropertyType.set(roleId, associationType, roleId);

    // A ----> R
    let roleIds = this.getCompositesRole(associationId, roleType);
    roleIds = add(roleIds, roleId);
    this.propertyByObjectByPropertyType.set(associationId, roleType, roleIds);
  }

  private addCompositesRoleMany2Many(associationId: number, roleType: RoleType, roleId: number) {
    /*  [if exist]        [no remove]         set
     *
     *  RA ----- R         RA       R       RA    -- R       RA ----- R
     *                ->                +        -        =       -
     *   A ----- PR         A       PR       A --    PR       A --    PR
     */
    const associationType = roleType.associationType;
    const previousRoleIds = this.getCompositesRole(associationId, roleType);

    // R in PR
    if (has(previousRoleIds, roleId)) {
      return;
    }

    // A <---- R
    let associationIds = this.getCompositesRole(roleId, associationType);
    associationIds = add(associationIds, associationId);
    this.propertyByObjectByPropertyType.set(roleId, associationType, associationIds);

    // A ----> R
    let roleIds = this.getCompositesRole(associationId, roleType);
    roleIds = add(roleIds, roleId);
    this.propertyByObjectByPropertyType.set(associationId, roleType, roleIds);
  }

  private removeCompositesRoleOne2Many(associationId: number, roleType: RoleType, roleId: number) {
    const associationType = roleType.associationType;
    const previousRoleIds = this.getCompositesRole(associationId, roleType);

    // R not in PR
    if (!has(previousRoleIds, roleId)) {
      return;
    }

    // A <---- R
    this.propertyByObjectByPropertyType.set(roleId, associationType, null);

    // A ----> R
    let roleIds = this.getCompositesRole(associationId, roleType);
    roleIds = add(roleIds, roleId);
    this.propertyByObjectByPropertyType.set(associationId, roleType, roleIds);
  }

  private removeCompositesRoleMany2Many(associationId: number, roleType: RoleType, roleId: number) {
    const associationType = roleType.associationType;
    const previousRoleIds = this.getCompositesRole(associationId, roleType);

    // R not in PR
    if (!has(previousRoleIds, roleId)) {
      return;
    }

    // A <---- R
    let associationIds = this.getCompositesRole(roleId, associationType);
    associationIds = remove(associationIds, associationId);
    this.propertyByObjectByPropertyType.set(roleId, associationType, associationIds);

    // A ----> R
    let roleIds = this.getCompositesRole(associationId, roleType);
    roleIds = remove(roleIds, roleId);
    this.propertyByObjectByPropertyType.set(associationId, roleType, roleIds);
  }
}
