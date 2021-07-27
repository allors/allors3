import { IRecord } from '../../IRecord';
import { ChangeSet } from '../ChangeSet';
import { Strategy } from '../Strategy';
import { Workspace } from '../../workspace/Workspace';
import { Session } from '../Session';
import { add, has, remove, difference, enumerate, IRange } from '../../collections/Range';
import { Class, RelationType, RoleType } from '@allors/workspace/meta/system';
import { IUnit } from '@allors/workspace/domain/system';

export abstract class RecordBasedOriginState {
  public abstract strategy: Strategy;

  protected hasChanges(): boolean {
    return this.record == null || this.changedRoleByRelationType?.size > 0;
  }

  protected abstract roleTypes: Set<RoleType>;

  protected abstract record: IRecord;

  protected previousRecord: IRecord;

  public changedRoleByRelationType: Map<RelationType, unknown>;

  private previousChangedRoleByRelationType: Map<RelationType, unknown>;

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
      const previousAssociation = this.session.getCompositeAssociation(role, associationType);
      this.setChangedRole(roleType, role);
      if (associationType.isOne && previousAssociation != null) {
        //  OneToOne
        previousAssociation.setRole(roleType, null);
      }
    } else {
      this.setChangedRole(roleType, role);
    }
  }

  public getCompositesRole(roleType: RoleType): IRange {
    return this.getRole(roleType) as IRange;
  }

  public addCompositesRole(roleType: RoleType, roleToAdd: number) {
    const associationType = roleType.associationType;

    const previousRole = this.getCompositesRole(roleType);

    if (has(previousRole, roleToAdd)) {
      return;
    }

    const role = add(previousRole, roleToAdd);
    this.setChangedRole(roleType, role);
    if (associationType.isMany) {
      return;
    }

    //  OneToMany
    const previousAssociation = this.session.getCompositeAssociation(roleToAdd, associationType);
    previousAssociation?.setRole(roleType, null);
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
    for (const addedRole of enumerate(difference(role, previousRole))) {
      const previousAssociation = this.session.getCompositeAssociation(addedRole, associationType);
      previousAssociation?.setRole(roleType, null);
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

        if (this.previousChangedRoleByRelationType?.has(relationType)) {
          const previous = this.previousChangedRoleByRelationType.get(relationType);
          const current = this.changedRoleByRelationType?.has(relationType) ? this.changedRoleByRelationType.get(relationType) : this.record.getRole(roleType);
          changeSet.diff(this.strategy, relationType, current, previous);
        } else {
          const previous = this.previousRecord?.getRole(roleType);
          const current = this.changedRoleByRelationType?.has(relationType) ? this.changedRoleByRelationType?.get(relationType) : this.record.getRole(roleType);
          changeSet.diff(this.strategy, relationType, current, previous);
        }
      });
    }

    this.previousRecord = this.record;
    this.previousChangedRoleByRelationType = this.changedRoleByRelationType;
  }

  // TODO: Koen
  public diff(diffs: IDiff[]) {
    if (this.changedRoleByRelationType == null) {
      return;
    }

    for (let kvp in this.ChangedRoleByRelationType) {
      let relationType = kvp.Key;
      let roleType = relationType.RoleType;
      let changed = kvp.Value;
      let original = this.Record?.GetRole(roleType);
      if (roleType.ObjectType.IsUnit) {
        diffs.Add(new UnitDiff(relationType, this.Strategy));
      } else if (roleType.IsOne) {
        diffs.Add(new CompositeDiff(relationType, this.Strategy));
      } else {
        diffs.Add(new CompositesDiff(relationType, this.Strategy));
      }
    }
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
