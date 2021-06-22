import { Class, RelationType, RoleType } from '@allors/workspace/meta/system';
import { IRecord } from '../../IRecord';
import { ChangeSet } from '../ChangeSet';
import { Strategy } from '../Strategy';
import { Workspace } from '../../workspace/Workspace';
import { Session } from '../Session';
import { add, has, remove, difference, enumerate } from '../../collections/Numbers';

export abstract class RecordBasedOriginState {
  public abstract Strategy: Strategy;

  protected HasChanges(): boolean {
    return this.Record == null || this.ChangedRoleByRelationType?.size > 0;
  }

  protected abstract RoleTypes: RoleType[];

  protected abstract Record: IRecord;

  protected PreviousRecord: IRecord;

  public ChangedRoleByRelationType: Map<RelationType, any>;

  private PreviousChangedRoleByRelationType: Map<RelationType, any>;

  public getRole(roleType: RoleType): any {
    if (this.ChangedRoleByRelationType != null && this.ChangedRoleByRelationType.has(roleType.relationType)) {
      return this.ChangedRoleByRelationType.get(roleType.relationType);
    }

    return this.Record?.getRole(roleType);
  }

  public SetUnitRole(roleType: RoleType, role: any) {
    this.SetChangedRole(roleType, role);
  }

  public SetCompositeRole(roleType: RoleType, role?: number) {
    const previousRole = this.getRole(roleType) as number;
    if (previousRole == role) {
      return;
    }

    const associationType = roleType.associationType;
    if (associationType.isOne && role != null) {
      const previousAssociationObject = this.Session.getAssociation(role, associationType).FirstOrDefault();
      this.SetChangedRole(roleType, role);
      if (associationType.isOne && previousAssociationObject != null) {
        //  OneToOne
        previousAssociationObject?.Strategy.Set(roleType, null);
      }
    } else {
      this.SetChangedRole(roleType, role);
    }
  }

  public AddCompositeRole(roleType: RoleType, roleToAdd: number) {
    const previousRole = this.getRole(roleType);
    if (has(previousRole, roleToAdd)) {
      return;
    }

    const role = add(previousRole, roleToAdd);
    this.SetChangedRole(roleType, role);
    const associationType = roleType.associationType;
    if (associationType.isMany) {
      return;
    }

    //  OneToMany
    const previousAssociationObject = this.Session.getAssociation(roleToAdd, associationType).FirstOrDefault();
    previousAssociationObject?.Strategy.Set(roleType, null);
  }

  public RemoveCompositeRole(roleType: RoleType, roleToRemove: number) {
    const previousRole = this.getRole(roleType);
    if (!has(previousRole, roleToRemove)) {
      return;
    }

    const role = remove(previousRole, roleToRemove);
    this.SetChangedRole(roleType, role);
  }

  public SetCompositesRole(roleType: RoleType, role: any) {
    const previousRole = this.getRole(roleType);
    this.SetChangedRole(roleType, role);
    const associationType = roleType.associationType;
    if (associationType.isMany) {
      return;
    }

    //  OneToMany
    const addedRoles = difference(role, previousRole);
    for (const addedRole of enumerate(addedRoles)) {
      const previousAssociationObject = this.Session.getAssociation(addedRole, associationType).FirstOrDefault();
      previousAssociationObject?.Strategy.Set(roleType, null);
    }
  }

  public Checkpoint(changeSet: ChangeSet) {
    //  Same record
    if (this.PreviousRecord == null || this.Record == null || this.Record.version == this.PreviousRecord.version) {
      //  No previous changed roles
      if (this.PreviousChangedRoleByRelationType == null) {
        if (this.ChangedRoleByRelationType != null) {
          //  Changed roles
          this.ChangedRoleByRelationType.forEach((current, relationType) => {
            const previous = this.Record?.getRole(relationType.roleType);
            changeSet.Diff(this.Strategy, relationType, current, previous);
          });
        }
      }

      //  Previous changed roles
      this.ChangedRoleByRelationType.forEach((current, relationType) => {
        const previous = this.PreviousChangedRoleByRelationType.get(relationType);
        changeSet.Diff(this.Strategy, relationType, current, previous);
      });
    } else {
      //  Different record
      this.RoleTypes.forEach((roleType) => {
        const relationType = roleType.relationType;
        let previous = null;
        let current = null;
        if (this.PreviousChangedRoleByRelationType?.has(relationType)) {
          const previous = this.PreviousChangedRoleByRelationType.get(relationType);
          if (this.ChangedRoleByRelationType?.has(relationType)) {
            const current = this.ChangedRoleByRelationType.get(relationType);
            changeSet.Diff(this.Strategy, relationType, current, previous);
          } else {
            current = this.Record.getRole(roleType);
            changeSet.Diff(this.Strategy, relationType, current, previous);
          }
        } else {
          previous = this.PreviousRecord?.getRole(roleType);
          if (this.ChangedRoleByRelationType?.has(relationType)) {
            const current = this.ChangedRoleByRelationType?.get(relationType);
            changeSet.Diff(this.Strategy, relationType, current, previous);
          } else {
            current = this.Record.getRole(roleType);
            changeSet.Diff(this.Strategy, relationType, current, previous);
          }
        }
      });
    }

    this.PreviousRecord = this.Record;
    this.PreviousChangedRoleByRelationType = this.ChangedRoleByRelationType;
  }

  public IsAssociationForRole(roleType: RoleType, forRole: number): boolean {
    if (roleType.objectType.isUnit) {
      return false;
    }

    const role = this.getRole(roleType);
    if (roleType.isOne) {
      return role == forRole;
    }

    return has(role, forRole);
  }

  protected abstract OnChange();

  private SetChangedRole(roleType: RoleType, role: any) {
    this.ChangedRoleByRelationType ??= new Map<RelationType, any>();
    this.ChangedRoleByRelationType.set(roleType.relationType, role);
    this.OnChange();
  }

  protected get Id(): number {
    return this.Strategy.id;
  }

  protected get Class(): Class {
    return this.Strategy.class;
  }

  protected get Session(): Session {
    return this.Strategy.Session;
  }

  protected get Workspace(): Workspace {
    return this.Strategy.Workspace;
  }
}
