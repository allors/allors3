import { IRecord } from '../../IRecord';
import { ChangeSet } from '../ChangeSet';
import { Strategy } from '../Strategy';
import { Workspace } from '../../workspace/Workspace';
import { Session } from '../Session';
import { add, has, remove, difference, enumerate, IRange } from '../../collections/Range';
import { Class, RelationType, RoleType } from '@allors/workspace/meta/system';
import { IObject, IUnit } from '@allors/workspace/domain/system';

export abstract class RecordBasedOriginState {
  protected abstract record: IRecord;

  protected previousRecord: IRecord;

  protected abstract roleTypes: Set<RoleType>;

  public changedRoleByRelationType: Map<RelationType, unknown>;

  private previousChangedRoleByRelationType: Map<RelationType, unknown>;

  protected constructor(public strategy: Strategy) {}

  protected hasChanges(): boolean {
    return this.record == null || this.changedRoleByRelationType?.size > 0;
  }

  public getUnitRole(roleType: RoleType): IUnit {
    return this.getRole(roleType) as IUnit;
  }

  public setUnitRole(roleType: RoleType, role: IUnit) {
    this.setChangedRole(roleType, role);
  }

  public getCompositeRole(roleType: RoleType): number {
    return this.getRole(roleType) as number;
  }

  public setCompositeRole(roleType: RoleType, role?: number) {
    const previousRole = this.getCompositeRole(roleType);

    if (previousRole == role) {
      return;
    }

    const associationType = roleType.associationType;
    if (associationType.isOne && role != null) {
      const previousAssociationObject = this.session.getCompositeAssociation(role, associationType);
      this.setChangedRole(roleType, role);
      if (associationType.isOne && previousAssociationObject != null) {
        //  OneToOne
        previousAssociationObject.strategy.setRole(roleType, null);
      }
    } else {
      this.setChangedRole(roleType, role);
    }
  }

  public getCompositesRole(roleType: RoleType): IRange {
    return this.getRole(roleType) as IRange;
  }

  public addCompositesRole(roleType: RoleType, roleToAdd: number) {
    const previousRole = this.getCompositesRole(roleType);

    if (has(previousRole, roleToAdd)) {
      return;
    }

    const role = add(previousRole, roleToAdd);
    this.setChangedRole(roleType, role);
    const associationType = roleType.associationType;
    if (associationType.isMany) {
      return;
    }

    //  OneToMany
    const previousAssociationObject = this.session.getCompositeAssociation(roleToAdd, associationType);
    previousAssociationObject?.strategy.setRole(roleType, null);
  }

  public removeCompositesRole(roleType: RoleType, roleToRemove: number) {
    const previousRole = this.getCompositesRole(roleType);

    if (!has(previousRole, roleToRemove)) {
      return;
    }

    const role = remove(previousRole, roleToRemove);
    this.setChangedRole(roleType, role);
  }

  public setCompositesRole(roleType: RoleType, role: IRange) {
    const previousRole = this.getCompositesRole(roleType);

    this.setChangedRole(roleType, role);

    const associationType = roleType.associationType;
    if (associationType.isMany) {
      return;
    }

    //  OneToMany
    const addedRoles = difference(role, previousRole);
    for (const addedRole of enumerate(addedRoles)) {
      const previousAssociationObject = this.session.getCompositeAssociation(addedRole, associationType);
      previousAssociationObject?.strategy.setRole(roleType, null);
    }
  }

  public checkpoint(changeSet: ChangeSet) {
    //  Same record
    if (this.previousRecord == null || this.record == null || this.record.version == this.previousRecord.version) {
      //  No previous changed roles
      if (this.previousChangedRoleByRelationType == null) {
        if (this.changedRoleByRelationType != null) {
          //  Changed roles
          this.changedRoleByRelationType.forEach((current, relationType) => {
            const previous = this.record?.getRole(relationType.roleType);
            changeSet.diff(this.strategy, relationType, current, previous);
          });
        }
      }

      //  Previous changed roles
      this.changedRoleByRelationType.forEach((current, relationType) => {
        const previous = this.previousChangedRoleByRelationType.get(relationType);
        changeSet.diff(this.strategy, relationType, current, previous);
      });
    } else {
      //  Different record
      this.roleTypes.forEach((roleType) => {
        const relationType = roleType.relationType;
        let previous = null;
        let current = null;
        if (this.previousChangedRoleByRelationType?.has(relationType)) {
          const previous = this.previousChangedRoleByRelationType.get(relationType);
          if (this.changedRoleByRelationType?.has(relationType)) {
            const current = this.changedRoleByRelationType.get(relationType);
            changeSet.diff(this.strategy, relationType, current, previous);
          } else {
            current = this.record.getRole(roleType);
            changeSet.diff(this.strategy, relationType, current, previous);
          }
        } else {
          previous = this.previousRecord?.getRole(roleType);
          if (this.changedRoleByRelationType?.has(relationType)) {
            const current = this.changedRoleByRelationType?.get(relationType);
            changeSet.diff(this.strategy, relationType, current, previous);
          } else {
            current = this.record.getRole(roleType);
            changeSet.diff(this.strategy, relationType, current, previous);
          }
        }
      });
    }

    this.previousRecord = this.record;
    this.previousChangedRoleByRelationType = this.changedRoleByRelationType;
  }

  public isAssociationForRole(roleType: RoleType, forRole: number): boolean {
    if (roleType.isOne) {
      const compositeRole = this.getCompositeRole(roleType);
      return compositeRole == forRole;
    }

    const compositesRole = this.getCompositesRole(roleType);
    return has(compositesRole, forRole);
  }

  protected abstract onChange();

  private getRole(roleType: RoleType): unknown {
    if (this.changedRoleByRelationType != null && this.changedRoleByRelationType.has(roleType.relationType)) {
      return this.changedRoleByRelationType.get(roleType.relationType);
    }

    return this.record?.getRole(roleType);
  }

  private setChangedRole(roleType: RoleType, role: unknown) {
    this.changedRoleByRelationType ??= new Map<RelationType, unknown>();
    this.changedRoleByRelationType.set(roleType.relationType, role);
    this.onChange();
  }

  //#region Proxy
  protected get id(): number {
    return this.strategy.id;
  }

  protected get class(): Class {
    return this.strategy.cls;
  }

  protected get session(): Session {
    return this.strategy.session;
  }

  protected get workspace(): Workspace {
    return this.strategy.session.workspace;
  }
  //#endregion
}
