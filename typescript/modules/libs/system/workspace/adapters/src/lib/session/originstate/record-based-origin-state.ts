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
import { CompositesDelta } from './composites-delta';

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

  getCompositeRole(roleType: RoleType): IObject {
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
    this.assertStrategy(obj, role, roleType);
    return obj;
  }

  getCompositeRoleId(roleType: RoleType): number {
    if (
      this.changedRoleByRelationType != null &&
      this.changedRoleByRelationType.has(roleType.relationType)
    ) {
      return (
        (this.changedRoleByRelationType.get(roleType.relationType) as IObject)
          ?.id ?? 0
      );
    }

    return (this.record?.getRole(roleType) as number) ?? 0;
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

  getCompositesRoleIds(roleType: RoleType): IRange<number> {
    return this.compositesIds(
      this.compositesDelta(roleType),
      this.record,
      roleType
    );
  }

  getCompositesRole(roleType: RoleType): IRange<IObject> {
    const delta = this.compositesDelta(roleType);
    if (delta != null) {
      let objects = this.recordCompositesRole(roleType, delta.removed);
      for (const added of this.objectRanges.enumerate(delta.added)) {
        objects = this.objectRanges.add(objects, added);
      }

      return objects ?? (frozenEmptyArray as IObject[]);
    }

    if (
      this.cachedRoleByRelationType != null &&
      this.cachedRoleByRelationType.has(roleType.relationType)
    ) {
      return this.cachedRoleByRelationType.get(
        roleType.relationType
      ) as IRange<IObject>;
    }

    const objects =
      this.recordCompositesRole(roleType, undefined) ??
      (frozenEmptyArray as IObject[]);

    this.cachedRoleByRelationType ??= new Map();
    this.cachedRoleByRelationType.set(roleType.relationType, objects);

    return objects;
  }

  addCompositesRole(roleType: RoleType, roleToAdd: IObject) {
    if (this.hasCompositesRole(roleType, roleToAdd)) {
      return;
    }

    const associationType = roleType.associationType;
    const previousAssociation = associationType.isOne
      ? this.session.getCompositeAssociation(roleToAdd, associationType)
      : null;

    const delta = this.changedCompositesDelta(roleType);
    if (
      this.recordRanges.has(
        this.recordCompositesRoleIds(roleType),
        roleToAdd.id
      )
    ) {
      delta.removed = this.recordRanges.remove(delta.removed, roleToAdd.id);
    } else {
      delta.added = this.objectRanges.add(delta.added, roleToAdd);
    }

    this.setChangedRole(roleType, delta);

    if (associationType.isMany) {
      return;
    }

    //  OneToMany
    if (previousAssociation != null && previousAssociation !== this.object) {
      previousAssociation.strategy.removeCompositesRole(roleType, roleToAdd);
    }
  }

  removeCompositesRole(roleType: RoleType, roleToRemove: IObject) {
    if (!this.hasCompositesRole(roleType, roleToRemove)) {
      return;
    }

    const delta = this.changedCompositesDelta(roleType);
    if (this.objectRanges.has(delta.added, roleToRemove)) {
      delta.added = this.objectRanges.remove(delta.added, roleToRemove);
    } else {
      delta.removed = this.recordRanges.add(delta.removed, roleToRemove.id);
    }

    this.setChangedRole(roleType, delta);
  }

  setCompositesRole(roleType: RoleType, role: IRange<IObject>) {
    if (this.sameCompositesRole(roleType, role)) {
      return;
    }

    const associationType = roleType.associationType;
    const previousIds = this.getCompositesRoleIds(roleType);
    const addedRoles = (role ?? []).filter(
      (v) => !this.recordRanges.has(previousIds, v.id)
    );
    const previousAssociations = associationType.isOne
      ? addedRoles.map((v) =>
          this.session.getCompositeAssociation(v, associationType)
        )
      : [];

    const recordIds = this.recordCompositesRoleIds(roleType);
    const added = (role ?? []).filter(
      (v) => !this.recordRanges.has(recordIds, v.id)
    );
    const roleIds = this.recordRanges.importFrom(role?.map((v) => v.id));
    this.setChangedRole(roleType, {
      added: added.length > 0 ? this.objectRanges.importFrom(added) : undefined,
      removed: this.recordRanges.difference(recordIds, roleIds),
    } as CompositesDelta);

    if (associationType.isMany) {
      return;
    }

    //  OneToMany
    addedRoles.forEach((addedRole, i) => {
      const previousAssociation = previousAssociations[i];
      if (previousAssociation != null && previousAssociation !== this.object) {
        previousAssociation.strategy.removeCompositesRole(roleType, addedRole);
      }
    });
  }

  checkpoint(changeSet: ChangeSet) {
    const sameRecord =
      this.previousRecord == null ||
      this.record == null ||
      this.record.version == this.previousRecord.version;
    const previousRecord = sameRecord ? this.record : this.previousRecord;

    //  Same record: only the changed roles; different record: every role
    const relationTypes: RelationType[] = sameRecord
      ? [...(this.changedRoleByRelationType?.keys() ?? [])]
      : [...this.roleTypes].map((v) => v.relationType);

    for (const relationType of relationTypes) {
      const roleType = relationType.roleType;
      const hasCurrent =
        this.changedRoleByRelationType?.has(relationType) ?? false;
      const hasPrevious =
        this.previousChangedRoleByRelationType?.has(relationType) ?? false;
      const current = hasCurrent
        ? this.changedRoleByRelationType.get(relationType)
        : this.record?.getRole(roleType);
      const previous = hasPrevious
        ? this.previousChangedRoleByRelationType.get(relationType)
        : previousRecord?.getRole(roleType);

      if (roleType.objectType.isUnit) {
        changeSet.diffUnit(this.object, relationType, current, previous);
      } else if (roleType.isOne) {
        changeSet.diffComposite(
          this.object,
          relationType,
          hasCurrent ? (current as IObject)?.id ?? 0 : (current as number) ?? 0,
          hasPrevious
            ? (previous as IObject)?.id ?? 0
            : (previous as number) ?? 0
        );
      } else {
        changeSet.diffComposites(
          this.object,
          relationType,
          this.compositesIds(
            hasCurrent ? (current as CompositesDelta) : null,
            this.record,
            roleType
          ),
          this.compositesIds(
            hasPrevious ? (previous as CompositesDelta) : null,
            previousRecord,
            roleType
          )
        );
      }
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
        const originalRoleId = (original as number) ?? 0;
        const changedRole = changed as IObject;
        const diff: ICompositeDiff = {
          relationType,
          assocation: this.object,
          originalRoleId,
          changedRoleId: changedRole?.id ?? 0,
          originalRole: originalRoleId
            ? this.session.getObject(originalRoleId)
            : null,
          changedRole,
        };

        diffs.push(diff);
      } else {
        const originalRoleIds = this.compositesIds(null, this.record, roleType);
        const changedRoleIds = this.compositesIds(
          changed as CompositesDelta,
          this.record,
          roleType
        );
        const diff: ICompositesDiff = {
          relationType,
          assocation: this.object,
          originalRoleIds: originalRoleIds ?? (frozenEmptyArray as number[]),
          changedRoleIds: changedRoleIds ?? (frozenEmptyArray as number[]),
          originalRoles: this.heldObjects(originalRoleIds),
          changedRoles: this.heldObjects(changedRoleIds),
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
        const equal =
          original instanceof Date && newOriginal instanceof Date
            ? original.getTime() === newOriginal.getTime()
            : original === newOriginal;
        if (!equal) {
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
      const roleId = this.getCompositeRoleId(roleType);
      return roleId !== 0 && roleId === forRole.id;
    }

    return this.hasCompositesRole(roleType, forRole);
  }

  protected abstract onChange();

  private setChangedRole(roleType: RoleType, role: unknown) {
    this.changedRoleByRelationType ??= new Map<RelationType, unknown>();
    this.changedRoleByRelationType.set(roleType.relationType, role);
    this.onChange();
  }

  private assertStrategy(object: IObject, id: number, roleType: RoleType) {
    if (object == null) {
      const role = `${roleType.associationType.objectType.singularName}.${
        roleType.isMany ? roleType.pluralName : roleType.singularName
      }`;
      throw new Error(
        `Object ${id} in role ${role} is not in this session. Include the role in the Pull, or pull the object, before reading it.`
      );
    }
  }

  private compositesDelta(roleType: RoleType): CompositesDelta {
    return this.changedRoleByRelationType?.has(roleType.relationType)
      ? (this.changedRoleByRelationType.get(
          roleType.relationType
        ) as CompositesDelta)
      : null;
  }

  private changedCompositesDelta(roleType: RoleType): CompositesDelta {
    const delta = this.compositesDelta(roleType);
    return { added: delta?.added, removed: delta?.removed };
  }

  private recordCompositesRoleIds(roleType: RoleType): IRange<number> {
    return this.record?.getRole(roleType) as IRange<number>;
  }

  private recordCompositesRole(
    roleType: RoleType,
    removed: IRange<number>
  ): IRange<IObject> {
    const ids = this.recordRanges.difference(
      this.recordCompositesRoleIds(roleType),
      removed
    );

    if (ids == null) {
      return undefined;
    }

    return this.objectRanges.importFrom(
      ids.map((v) => {
        const obj = this.session.getObject(v);
        this.assertStrategy(obj, v, roleType);
        return obj;
      })
    );
  }

  private hasCompositesRole(roleType: RoleType, role: IObject): boolean {
    const delta = this.compositesDelta(roleType);
    if (delta != null) {
      if (this.objectRanges.has(delta.added, role)) {
        return true;
      }

      if (this.recordRanges.has(delta.removed, role.id)) {
        return false;
      }
    }

    return this.recordRanges.has(
      this.recordCompositesRoleIds(roleType),
      role.id
    );
  }

  private compositesIds(
    delta: CompositesDelta,
    record: IRecord,
    roleType: RoleType
  ): IRange<number> {
    let ids = record?.getRole(roleType) as IRange<number>;
    if (delta != null) {
      ids = this.recordRanges.difference(ids, delta.removed);
      for (const added of this.objectRanges.enumerate(delta.added)) {
        ids = this.recordRanges.add(ids, added.id);
      }
    }

    return ids?.length > 0 ? ids : undefined;
  }

  private heldObjects(ids: IRange<number>): IObject[] {
    return (ids ?? [])
      .map((v) => this.session.getObject(v))
      .filter((v) => v != null);
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
    const roleIds =
      role?.length > 0
        ? this.recordRanges.importFrom(role.map((v) => v.id))
        : undefined;
    return this.recordRanges.equals(
      this.getCompositesRoleIds(roleType),
      roleIds
    );
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
