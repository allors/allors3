import { Class, RelationType, RoleType } from '@allors/system/workspace/meta';
import {
  ICompositeDiff,
  ICompositesDiff,
  IDiff,
  IObject,
  IUnit,
  IUnitDiff,
} from '@allors/system/workspace/domain';

import { IRecord } from '../../irecord';
import { Workspace } from '../../workspace/workspace';
import { frozenEmptyArray } from '../../collections/frozen-empty-array';
import { IRange, Ranges } from '../../collections/ranges/ranges';

import { ChangeSet } from '../change-set';
import { Session } from '../session';

export abstract class RecordBasedOriginState {
  abstract object: IObject;

  protected abstract roleTypes: Set<RoleType>;

  protected abstract record: IRecord;

  protected abstract cachedRoleByRelationType: Map<
    RelationType,
    IRange<IObject>
  >;

  protected previousRecord: IRecord;

  changedRoleByRelationType: Map<RelationType, unknown>;

  private previousChangedRoleByRelationType: Map<RelationType, unknown>;

  getUnitRole(roleType: RoleType): IUnit {
    if (
      this.changedRoleByRelationType != null &&
      this.changedRoleByRelationType.has(roleType.relationType)
    ) {
      return this.changedRoleByRelationType.get(roleType.relationType) as IUnit;
    }

    return this.record?.getRole(roleType) as IUnit;
  }

  setUnitRole(roleType: RoleType, role: IUnit) {
    this.setChangedRole(roleType, role);
  }

  getCompositeRole(roleType: RoleType, skipMissing?: boolean): IObject {
    if (
      this.changedRoleByRelationType != null &&
      this.changedRoleByRelationType.has(roleType.relationType)
    ) {
      return this.changedRoleByRelationType.get(
        roleType.relationType
      ) as IObject;
    }

    const role = this.record?.getRole(roleType) as number;

    if (role == null) {
      return null;
    }

    const obj = this.session.getObject(role);

    if (!skipMissing) {
      this.assertStrategy(obj);
    }

    return obj;
  }

  setCompositeRole(roleType: RoleType, role?: IObject) {
    if (this.sameCompositeRole(roleType, role)) {
      return;
    }

    const associationType = roleType.associationType;
    if (associationType.isOne && role != null) {
      const previousAssociation = this.session.getCompositeAssociation(
        role,
        associationType
      );
      this.setChangedRole(roleType, role);

      if (associationType.isOne && previousAssociation != null) {
        //  OneToOne
        previousAssociation.strategy.setRole(roleType, null);
      }
    } else {
      this.setChangedRole(roleType, role);
    }
  }

  getCompositesRole(
    roleType: RoleType,
    skipMissing?: boolean
  ): IRange<IObject> {
    if (
      this.changedRoleByRelationType != null &&
      this.changedRoleByRelationType.has(roleType.relationType)
    ) {
      return this.changedRoleByRelationType.get(
        roleType.relationType
      ) as IRange<IObject>;
    }

    const role = this.record?.getRole(roleType) as IRange<number>;

    if (role == null) {
      return frozenEmptyArray as IObject[];
    }

    if (skipMissing) {
      return role
        .map((v) => {
          const obj = this.session.getObject(v);
          if (!skipMissing) {
            this.assertStrategy(obj);
          }
          return obj;
        })
        .filter((v) => v != null);
    }

    if (
      this.cachedRoleByRelationType != null &&
      this.cachedRoleByRelationType.has(roleType.relationType)
    ) {
      return this.cachedRoleByRelationType.get(
        roleType.relationType
      ) as IRange<IObject>;
    }

    const objects = role.map((v) => {
      const obj = this.session.getObject(v);
      this.assertStrategy(obj);
      return obj;
    });

    this.cachedRoleByRelationType ??= new Map();
    this.cachedRoleByRelationType.set(roleType.relationType, objects);

    return objects;
  }

  addCompositesRole(roleType: RoleType, roleToAdd: IObject) {
    const associationType = roleType.associationType;
    const previousAssociation = this.session.getCompositeAssociation(
      roleToAdd,
      associationType
    );

    let role = this.getCompositesRole(roleType);

    if (this.objectRanges.has(role, roleToAdd)) {
      return;
    }

    role = this.objectRanges.add(role, roleToAdd);
    this.setChangedRole(roleType, role);

    if (associationType.isMany) {
      return;
    }

    //  OneToMany
    previousAssociation?.strategy.setRole(roleType, null);
  }

  removeCompositesRole(roleType: RoleType, roleToRemove: IObject) {
    let role = this.getCompositesRole(roleType);

    if (!this.objectRanges.has(role, roleToRemove)) {
      return;
    }

    role = this.objectRanges.remove(role, roleToRemove);
    this.setChangedRole(roleType, role);
  }

  setCompositesRole(roleType: RoleType, role: IRange<IObject>) {
    if (this.sameCompositesRole(roleType, role)) {
      return;
    }

    const previousRole = this.getCompositesRole(roleType);

    this.setChangedRole(roleType, role);

    const associationType = roleType.associationType;
    if (associationType.isMany) {
      return;
    }

    //  OneToMany
    for (const addedRole of this.objectRanges.enumerate(
      this.objectRanges.difference(role, previousRole)
    )) {
      const previousAssociation = this.session.getCompositeAssociation(
        addedRole,
        associationType
      );

      if (previousAssociation != this.object) {
        previousAssociation?.strategy.setRole(roleType, null);
      }
    }
  }

  checkpoint(changeSet: ChangeSet) {
    //  Same record
    if (
      this.previousRecord == null ||
      this.record == null ||
      this.record.version == this.previousRecord.version
    ) {
      this.changedRoleByRelationType?.forEach((current, relationType) => {
        if (
          this.previousChangedRoleByRelationType != null &&
          this.previousChangedRoleByRelationType.has(relationType)
        ) {
          const previous =
            this.previousChangedRoleByRelationType.get(relationType);

          if (relationType.roleType.objectType.isUnit) {
            changeSet.diffUnit(this.object, relationType, current, previous);
          } else if (relationType.roleType.isOne) {
            changeSet.diffCompositeStrategyStrategy(
              this.object,
              relationType,
              current as IObject,
              previous as IObject
            );
          } else {
            changeSet.diffCompositesStrategyStrategy(
              this.object,
              relationType,
              current as IRange<IObject>,
              previous as IRange<IObject>
            );
          }
        } else {
          const previous = this.record?.getRole(relationType.roleType);

          if (relationType.roleType.objectType.isUnit) {
            changeSet.diffUnit(this.object, relationType, current, previous);
          } else if (relationType.roleType.isOne) {
            changeSet.diffCompositeStrategyRecord(
              this.object,
              relationType,
              current as IObject,
              previous as number
            );
          } else {
            changeSet.diffCompositesStrategyRecord(
              this.object,
              relationType,
              current as IRange<IObject>,
              previous as IRange<number>
            );
          }
        }
      });
      //  Previous changed roles
    } else {
      //  Different record
      this.roleTypes.forEach((roleType) => {
        const relationType = roleType.relationType;

        if (this.previousChangedRoleByRelationType?.has(relationType)) {
          const previous =
            this.previousChangedRoleByRelationType.get(relationType);
          const current = this.changedRoleByRelationType?.has(relationType)
            ? this.changedRoleByRelationType.get(relationType)
            : this.record.getRole(roleType);

          if (relationType.roleType.objectType.isUnit) {
            changeSet.diffUnit(this.object, relationType, current, previous);
          } else if (relationType.roleType.isOne) {
            changeSet.diffCompositeStrategyStrategy(
              this.object,
              relationType,
              current as IObject,
              previous as IObject
            );
          } else {
            changeSet.diffCompositesStrategyStrategy(
              this.object,
              relationType,
              current as IRange<IObject>,
              previous as IRange<IObject>
            );
          }
        } else {
          const previous = this.previousRecord?.getRole(roleType);
          const current = this.changedRoleByRelationType?.has(relationType)
            ? this.changedRoleByRelationType?.get(relationType)
            : this.record.getRole(roleType);

          if (relationType.roleType.objectType.isUnit) {
            changeSet.diffUnit(this.object, relationType, current, previous);
          } else if (relationType.roleType.isOne) {
            changeSet.diffCompositeRecordRecord(
              this.object,
              relationType,
              current as number,
              previous as number
            );
          } else {
            changeSet.diffCompositesRecordRecord(
              this.object,
              relationType,
              current as IRange<number>,
              previous as IRange<number>
            );
          }
        }
      });
    }

    this.previousRecord = this.record;
    this.previousChangedRoleByRelationType = new Map(
      this.changedRoleByRelationType
    );
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
          assocation: this.object,
          originalRole: original as IUnit,
          changedRole: changed as IUnit,
        };

        diffs.push(diff);
      } else if (roleType.isOne) {
        const diff: ICompositeDiff = {
          relationType,
          assocation: this.object,
          originalRole:
            original != null
              ? this.session.getObject(original as number)
              : null,
          changedRole: changed as IObject,
        };

        diffs.push(diff);
      } else {
        const diff: ICompositesDiff = {
          relationType,
          assocation: this.object,
          originalRoles:
            original != null
              ? (original as IRange<number>)?.map((v) =>
                  this.session.getObject(v)
                ) ?? frozenEmptyArray
              : frozenEmptyArray,
          changedRoles: [...(changed as Set<IObject>)],
        };

        diffs.push(diff);
      }
    }
  }

  get hasChanges(): boolean {
    return this.record == null || this.changedRoleByRelationType?.size > 0;
  }

  hasChanged(roleType: RoleType): boolean {
    return this.changedRoleByRelationType?.has(roleType.relationType) ?? false;
  }

  restoreRole(roleType: RoleType): void {
    this.changedRoleByRelationType?.delete(roleType.relationType);
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
      } else if (
        !this.recordRanges.equals(original as number[], newOriginal as number[])
      ) {
        return false;
      }
    }

    return true;
  }

  reset() {
    this.changedRoleByRelationType = null;
  }

  isAssociationForRole(roleType: RoleType, forRole: IObject): boolean {
    if (roleType.isOne) {
      const compositeRole = this.getCompositeRoleIfInstantiated(roleType);
      return compositeRole == forRole;
    }
    const composites = this.getCompositesRoleIfInstantiated(roleType);
    if (composites != null) {
      for (const role of composites) {
        if (role === forRole) {
          return true;
        }
      }
    }

    return false;
  }

  protected abstract onChange();

  private setChangedRole(roleType: RoleType, role: unknown) {
    this.changedRoleByRelationType ??= new Map<RelationType, unknown>();
    this.changedRoleByRelationType.set(roleType.relationType, role);
    this.onChange();
  }

  private assertStrategy(object: IObject) {
    if (object == null) {
      throw new Error('Object is not present in session.');
    }
  }

  private getCompositeRoleIfInstantiated(roleType: RoleType): IObject {
    if (
      this.changedRoleByRelationType != null &&
      this.changedRoleByRelationType.has(roleType.relationType)
    ) {
      return this.changedRoleByRelationType.get(
        roleType.relationType
      ) as IObject;
    }

    const role = this.record?.getRole(roleType) as number;

    if (role == null) {
      return null;
    }

    const obj = this.session.getObject(role);
    return obj;
  }

  private getCompositesRoleIfInstantiated(roleType: RoleType): IRange<IObject> {
    if (
      this.changedRoleByRelationType != null &&
      this.changedRoleByRelationType.has(roleType.relationType)
    ) {
      return this.changedRoleByRelationType.get(
        roleType.relationType
      ) as IRange<IObject>;
    }

    const role = this.record?.getRole(roleType) as IRange<number>;

    if (role == null) {
      return frozenEmptyArray as IObject[];
    }

    const strategies = role
      .map((v) => {
        const strategy = this.session.getObject(v);
        return strategy;
      })
      .filter((v) => v != null);

    return strategies;
  }

  private sameCompositeRole(roleType: RoleType, role: IObject): boolean {
    if (
      this.changedRoleByRelationType != null &&
      this.changedRoleByRelationType.has(roleType.relationType)
    ) {
      return role === this.changedRoleByRelationType.get(roleType.relationType);
    }

    const changedRoleId = this.record?.getRole(roleType) as number;

    if (role == null) {
      return changedRoleId == null;
    }

    if (changedRoleId == null) {
      return false;
    }

    return role.id == changedRoleId;
  }

  private sameCompositesRole(
    roleType: RoleType,
    role: IRange<IObject>
  ): boolean {
    if (
      this.changedRoleByRelationType != null &&
      this.changedRoleByRelationType.has(roleType.relationType)
    ) {
      return this.objectRanges.equals(
        role,
        this.changedRoleByRelationType.get(
          roleType.relationType
        ) as IRange<IObject>
      );
    }

    const roleIds = this.record?.getRole(roleType) as IRange<number>;

    if (role == null) {
      return roleIds == null;
    }

    if (roleIds == null) {
      return false;
    }

    if (role.length != roleIds.length) {
      return false;
    }

    for (let i = 0; i < role.length; i++) {
      if (role[i].id !== roleIds[i]) {
        return false;
      }
    }

    return true;
  }

  //#region Proxy
  protected get id(): number {
    return this.object.id;
  }

  protected get class(): Class {
    return this.object.strategy.cls;
  }

  protected get session(): Session {
    return this.object.strategy.session as Session;
  }

  protected get workspace(): Workspace {
    return this.object.strategy.session.workspace as Workspace;
  }

  protected get objectRanges(): Ranges<IObject> {
    return this.session.ranges;
  }

  protected get recordRanges(): Ranges<number> {
    return this.workspace.ranges;
  }
  //#endregion
}
