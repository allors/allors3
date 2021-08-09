import { IRecord } from '../../IRecord';
import { ChangeSet } from '../ChangeSet';
import { Strategy } from '../Strategy';
import { Workspace } from '../../workspace/Workspace';
import { Session } from '../Session';
import { add, has, remove, difference, enumerate, IRange, equals } from '../../collections/Range';
import { Class, RelationType, RoleType } from '@allors/workspace/meta/system';
import { ICompositeDiff, ICompositesDiff, IDiff, IUnit, IUnitDiff } from '@allors/workspace/domain/system';

export abstract class RecordBasedOriginState {
  abstract strategy: Strategy;

  protected hasChanges(): boolean {
    return this.record == null || this.changedRoleByRelationType?.size > 0;
  }

  protected abstract roleTypes: Set<RoleType>;

  protected abstract record: IRecord;

  protected previousRecord: IRecord;

  changedRoleByRelationType: Map<RelationType, unknown>;

  private previousChangedRoleByRelationType: Map<RelationType, unknown>;

  getUnitRole(roleType: RoleType): IUnit {
    return this.getRole(roleType) as IUnit;
  }

  setUnitRole(roleType: RoleType, role: IUnit) {
    this.setChangedRole(roleType, role);
  }

  getCompositeRole(roleType: RoleType): number {
    return this.getRole(roleType) as number;
  }

  setCompositeRole(roleType: RoleType, role?: number) {
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

  getCompositesRole(roleType: RoleType): IRange {
    return this.getRole(roleType) as IRange;
  }

  addCompositesRole(roleType: RoleType, roleToAdd: number) {
    const associationType = roleType.associationType;
    const previousAssociation = this.session.getCompositeAssociation(roleToAdd, associationType);

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
    previousAssociation?.setRole(roleType, null);
  }

  removeCompositesRole(roleType: RoleType, roleToRemove: number) {
    const previousRole = this.getCompositesRole(roleType);

    if (!has(previousRole, roleToRemove)) {
      return;
    }

    const role = remove(previousRole, roleToRemove);
    this.setChangedRole(roleType, role);
  }

  setCompositesRole(roleType: RoleType, role: IRange) {
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

  checkpoint(changeSet: ChangeSet) {
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
      } else {
        this.changedRoleByRelationType.forEach((current, relationType) => {
          const previous = this.previousChangedRoleByRelationType?.get(relationType);
          changeSet.diff(this.strategy, relationType, current, previous);
        });
      }

      //  Previous changed roles
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

  diff(diffs: IDiff[]) {
    if (this.changedRoleByRelationType == null) {
      return;
    }

    for (const [relationType, changed] of this.changedRoleByRelationType) {
      const roleType = relationType.roleType;
      const original = this.record?.getRole(roleType);

      if (roleType.objectType.isUnit) {
        const diff: IUnitDiff = {
          relationType,
          assocation: this.strategy,
          originalRole: original as IUnit,
          changedRole: changed as IUnit,
        };

        diffs.push(diff);
      } else if (roleType.isOne) {
        const diff: ICompositeDiff = {
          relationType,
          assocation: this.strategy,
          originalRoleId: original as number,
          changedRoleId: changed as number,
        };

        diffs.push(diff);
      } else {
        const diff: ICompositesDiff = {
          relationType,
          assocation: this.strategy,
          originalRoleIds: original as number[],
          changedRoleIds: changed as number[],
        };

        diffs.push(diff);
      }
    }
  }

  canMerge(newRecord: IRecord): boolean {
    if (this.changedRoleByRelationType == null) {
      return true;
    }

    for (const [relationType] of this.changedRoleByRelationType) {
      const roleType = relationType.roleType;
      const original = this.record?.getRole(roleType);
      const newOriginal = newRecord?.getRole(roleType);
      if (roleType.objectType.isUnit) {
        if (original !== newOriginal) {
          return false;
        }
      } else if (roleType.isOne) {
        if (original !== newOriginal) {
          return false;
        }
      } else if (!equals(original as number[], newOriginal as number[])) {
        return false;
      }
    }

    return true;
  }

  reset() {
    this.changedRoleByRelationType = null;
  }

  isAssociationForRole(roleType: RoleType, forRole: number): boolean {
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
