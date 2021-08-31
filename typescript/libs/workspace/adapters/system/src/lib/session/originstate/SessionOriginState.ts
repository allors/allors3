import { IUnit } from '@allors/workspace/domain/system';
import { PropertyType, RoleType } from '@allors/workspace/meta/system';
import { IRange, Ranges } from '../../collections/ranges/Ranges';
import { ChangeSet } from '../ChangeSet';
import { Strategy } from '../Strategy';
import { PropertyByObjectByPropertyType } from './PropertyByObjectByPropertyType';

export class SessionOriginState {
  private propertyByObjectByPropertyType: PropertyByObjectByPropertyType;

  public constructor(private ranges: Ranges<Strategy>) {
    this.propertyByObjectByPropertyType = new PropertyByObjectByPropertyType(ranges);
  }

  public checkpoint(changeSet: ChangeSet) {
    changeSet.addSessionStateChanges(this.propertyByObjectByPropertyType.checkpoint());
  }

  public getUnitRole(object: Strategy, propertyType: PropertyType): IUnit {
    return this.propertyByObjectByPropertyType.get(object, propertyType) as IUnit;
  }

  public setUnitRole(association: Strategy, roleType: RoleType, role: IUnit) {
    this.propertyByObjectByPropertyType.set(association, roleType, role);
  }

  public getCompositeRole(object: Strategy, propertyType: PropertyType): Strategy {
    return this.propertyByObjectByPropertyType.get(object, propertyType) as Strategy;
  }

  public setCompositeRole(association: Strategy, roleType: RoleType, newRole: Strategy) {
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

  public getCompositesRole(object: Strategy, propertyType: PropertyType): IRange<Strategy> {
    return this.propertyByObjectByPropertyType.get(object, propertyType) as IRange<Strategy>;
  }

  public addCompositesRole(association: Strategy, roleType: RoleType, item: Strategy) {
    if (roleType.associationType.isOne) {
      this.addCompositesRoleOne2Many(association, roleType, item);
    } else {
      this.addCompositesRoleMany2Many(association, roleType, item);
    }
  }

  public removeCompositesRole(association: Strategy, roleType: RoleType, item: Strategy) {
    if (roleType.associationType.isOne) {
      this.removeCompositesRoleOne2Many(association, roleType, item);
    } else {
      this.removeCompositesRoleMany2Many(association, roleType, item);
    }
  }

  public setCompositesRole(association: Strategy, roleType: RoleType, newRole: IRange<Strategy>) {
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

  private setCompositeRoleOne2One(associationId: Strategy, roleType: RoleType, roleId: Strategy) {
    /*  [if exist]        [then remove]        set
     *
     *  RA ----- R         RA --x-- R       RA    -- R       RA    -- R
     *                ->                +        -        =       -
     *   A ----- PR         A --x-- PR       A --    PR       A --    PR
     */
    const associationType = roleType.associationType;
    const previousRoleId = this.propertyByObjectByPropertyType.get(associationId, roleType) as Strategy;

    // R = PR
    if (roleId == previousRoleId) {
      return;
    }

    // A --x-- PR
    if (previousRoleId != null) {
      this.propertyByObjectByPropertyType.set(previousRoleId, associationType, null);
    }

    const roleAssociation = this.propertyByObjectByPropertyType.get(roleId, associationType) as Strategy;

    // RA --x-- R
    if (roleAssociation != null) {
      this.propertyByObjectByPropertyType.set(roleAssociation, roleType, null);
    }

    // A <---- R
    this.propertyByObjectByPropertyType.set(roleId, associationType, associationId);

    // A ----> R
    this.propertyByObjectByPropertyType.set(associationId, roleType, roleId);
  }

  private setCompositeRoleMany2One(associationId: Strategy, roleType: RoleType, roleId: Strategy) {
    /*  [if exist]        [then remove]        set
     *
     *  RA ----- R         RA       R       RA    -- R       RA ----- R
     *                ->                +        -        =       -
     *   A ----- PR         A --x-- PR       A --    PR       A --    PR
     */
    const associationType = roleType.associationType;
    const previousRoleId = this.propertyByObjectByPropertyType.get(associationId, roleType) as Strategy;

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
    associationIds = this.ranges.add(associationIds, associationId);
    this.propertyByObjectByPropertyType.set(roleId, associationType, associationIds);

    // A ----> R
    this.propertyByObjectByPropertyType.set(associationId, roleType, roleId);
  }

  private removeCompositeRoleOne2One(associationId: Strategy, roleType: RoleType, roleId: Strategy) {
    /*                        delete
     *
     *   A ----- R    ->     A       R  =   A       R
     */

    // A <---- R
    this.propertyByObjectByPropertyType.set(roleId, roleType.associationType, null);

    // A ----> R
    this.propertyByObjectByPropertyType.set(associationId, roleType, null);
  }

  private removeCompositeRoleMany2One(associationId: Strategy, roleType: RoleType, roleId: Strategy) {
    /*                        delete
     *  RA --                                RA --
     *       -        ->                 =        -
     *   A ----- R           A --x-- R             -- R
     */
    const associationType = roleType.associationType;

    // A <---- R
    let roleAssociations = this.getCompositesRole(roleId, associationType);
    roleAssociations = this.ranges.remove(roleAssociations, associationId);
    this.propertyByObjectByPropertyType.set(roleId, associationType, roleAssociations);

    // A ----> R
    this.propertyByObjectByPropertyType.set(associationId, roleType, null);
  }

  private addCompositesRoleOne2Many(associationId: Strategy, roleType: RoleType, roleId: Strategy) {
    /*  [if exist]        [then remove]        set
     *
     *  RA ----- R         RA       R       RA    -- R       RA ----- R
     *                ->                +        -        =       -
     *   A ----- PR         A --x-- PR       A --    PR       A --    PR
     */
    const associationType = roleType.associationType;
    const previousRoleId = this.getCompositesRole(associationId, roleType);

    // R in PR
    if (this.ranges.has(previousRoleId, roleId)) {
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
    roleIds = this.ranges.add(roleIds, roleId);
    this.propertyByObjectByPropertyType.set(associationId, roleType, roleIds);
  }

  private addCompositesRoleMany2Many(associationId: Strategy, roleType: RoleType, roleId: Strategy) {
    /*  [if exist]        [no remove]         set
     *
     *  RA ----- R         RA       R       RA    -- R       RA ----- R
     *                ->                +        -        =       -
     *   A ----- PR         A       PR       A --    PR       A --    PR
     */
    const associationType = roleType.associationType;
    const previousRoleIds = this.getCompositesRole(associationId, roleType);

    // R in PR
    if (this.ranges.has(previousRoleIds, roleId)) {
      return;
    }

    // A <---- R
    let associationIds = this.getCompositesRole(roleId, associationType);
    associationIds = this.ranges.add(associationIds, associationId);
    this.propertyByObjectByPropertyType.set(roleId, associationType, associationIds);

    // A ----> R
    let roleIds = this.getCompositesRole(associationId, roleType);
    roleIds = this.ranges.add(roleIds, roleId);
    this.propertyByObjectByPropertyType.set(associationId, roleType, roleIds);
  }

  private removeCompositesRoleOne2Many(associationId: Strategy, roleType: RoleType, roleId: Strategy) {
    const associationType = roleType.associationType;
    const previousRoleIds = this.getCompositesRole(associationId, roleType);

    // R not in PR
    if (!this.ranges.has(previousRoleIds, roleId)) {
      return;
    }

    // A <---- R
    this.propertyByObjectByPropertyType.set(roleId, associationType, null);

    // A ----> R
    let roleIds = this.getCompositesRole(associationId, roleType);
    roleIds = this.ranges.add(roleIds, roleId);
    this.propertyByObjectByPropertyType.set(associationId, roleType, roleIds);
  }

  private removeCompositesRoleMany2Many(associationId: Strategy, roleType: RoleType, roleId: Strategy) {
    const associationType = roleType.associationType;
    const previousRoleIds = this.getCompositesRole(associationId, roleType);

    // R not in PR
    if (!this.ranges.has(previousRoleIds, roleId)) {
      return;
    }

    // A <---- R
    let associationIds = this.getCompositesRole(roleId, associationType);
    associationIds = this.ranges.remove(associationIds, associationId);
    this.propertyByObjectByPropertyType.set(roleId, associationType, associationIds);

    // A ----> R
    let roleIds = this.getCompositesRole(associationId, roleType);
    roleIds = this.ranges.remove(roleIds, roleId);
    this.propertyByObjectByPropertyType.set(associationId, roleType, roleIds);
  }
}
