import {
  IDiff,
  IObject,
  IStrategy,
  IUnit,
} from '@allors/system/workspace/domain';
import {
  AssociationType,
  Class,
  Composite,
  MethodType,
  Origin,
  RoleType,
  UnitTags,
} from '@allors/system/workspace/meta';

import { DatabaseOriginState } from './originstate/database-origin-state';
import { frozenEmptyArray } from '../collections/frozen-empty-array';
import { isNewId, Session } from './session';

export abstract class Strategy implements IStrategy {
  DatabaseOriginState: DatabaseOriginState;

  rangeId: number;
  private _object: IObject;

  constructor(public session: Session, public cls: Class, public id: number) {
    this.rangeId = id;
  }

  get isDeleted() {
    return this.id === 0;
  }

  get version(): number {
    return this.DatabaseOriginState.version;
  }

  get isNew(): boolean {
    return isNewId(this.id);
  }

  get object(): IObject {
    return (this._object ??=
      this.session.workspace.database.configuration.objectFactory.create(this));
  }

  delete(): void {
    if (!this.isNew) {
      throw new Error('Existing database objects can not be deleted');
    }

    for (const roleType of this.cls.roleTypes) {
      if (roleType.objectType.isUnit) {
        this.setUnitRole(roleType, null);
      } else if (roleType.isOne) {
        this.setCompositeRole(roleType, null);
      } else {
        this.setCompositesRole(roleType, null);
      }
    }

    for (const associationType of this.cls.associationTypes) {
      const roleType = associationType.roleType;
      if (associationType.isOne) {
        const association = this.getCompositeAssociation(associationType);
        if (roleType.isOne) {
          association?.strategy.setCompositeRole(roleType, null);
        } else {
          association?.strategy.removeCompositesRole(roleType, this.object);
        }
      } else {
        const association = this.getCompositesAssociation(associationType);
        if (roleType.isOne) {
          association?.forEach((v) =>
            v.strategy.setCompositeRole(roleType, null)
          );
        } else {
          association?.forEach((v) =>
            v.strategy.removeCompositesRole(roleType, this.object)
          );
        }
      }
    }

    this.session.onDelete(this);

    this.id = 0;
  }

  reset(): void {
    this.DatabaseOriginState?.reset();
  }

  diff(): IDiff[] {
    const diffs: IDiff[] = [];
    this.DatabaseOriginState.diff(diffs);
    return diffs;
  }

  get hasChanges(): boolean {
    return this.DatabaseOriginState?.hasChanges;
  }

  hasChanged(roleType: RoleType): boolean {
    switch (roleType.origin) {
      case Origin.Session:
        return false;
      case Origin.Database:
        return this.canRead(roleType)
          ? this.DatabaseOriginState?.hasChanged(roleType) ?? false
          : false;
      default:
        throw new Error('Unknown origin');
    }
  }

  restoreRole(roleType: RoleType) {
    switch (roleType.origin) {
      case Origin.Session:
        return;
      case Origin.Database:
        return this.canRead(roleType)
          ? this.DatabaseOriginState?.restoreRole(roleType)
          : false;
      default:
        throw new Error('Unknown origin');
    }
  }

  existRole(roleType: RoleType): boolean {
    if (roleType.objectType.isUnit) {
      return this.getUnitRole(roleType) != null;
    }

    if (roleType.isOne) {
      return this.getCompositeRole(roleType) != null;
    }

    return this.getCompositesRole(roleType)?.length > 0;
  }

  getRole(roleType: RoleType): unknown {
    if (roleType == null) {
      throw new Error('Argument null');
    }

    if (roleType.objectType.isUnit) {
      return this.getUnitRole(roleType);
    }

    if (roleType.isOne) {
      return this.getCompositeRole(roleType);
    }

    return this.getCompositesRole(roleType);
  }

  getUnitRole(roleType: RoleType): IUnit {
    if (roleType.relationType.isDerived) {
      const rule = this.session.resolve(this, roleType);
      if (rule != null) {
        return rule.derive(this.object) as IUnit;
      }
    }

    switch (roleType.origin) {
      case Origin.Session:
        return (
          this.session.sessionOriginState.getUnitRole(this.object, roleType) ??
          null
        );
      case Origin.Database:
        return (
          (this.canRead(roleType)
            ? this.DatabaseOriginState?.getUnitRole(roleType)
            : null) ?? null
        );
      default:
        throw new Error('Unknown origin');
    }
  }

  getCompositeRole<T extends IObject>(
    roleType: RoleType,
    skipMissing?: boolean
  ): T {
    if (roleType.relationType.isDerived) {
      const rule = this.session.resolve(this, roleType);
      if (rule != null) {
        return rule.derive(this.object) as T;
      }
    }

    switch (roleType.origin) {
      case Origin.Session:
        return (
          (this.session.sessionOriginState.getCompositeRole(
            this.object,
            roleType
          ) as T) ?? null
        );
      case Origin.Database:
        return this.canRead(roleType)
          ? (this.DatabaseOriginState?.getCompositeRole(
              roleType,
              skipMissing
            ) as T) ?? null
          : null;
      default:
        throw new Error('Unknown origin');
    }
  }

  getCompositesRole<T extends IObject>(
    roleType: RoleType,
    skipMissing?: boolean
  ): T[] {
    if (roleType.relationType.isDerived) {
      const rule = this.session.resolve(this, roleType);
      if (rule != null) {
        return rule.derive(this.object) as T[];
      }
    }

    switch (roleType.origin) {
      case Origin.Session:
        return (
          (this.session.sessionOriginState.getCompositesRole(
            this.object,
            roleType
          ) as T[]) ?? (frozenEmptyArray as T[])
        );
      case Origin.Database:
        return this.canRead(roleType)
          ? (this.DatabaseOriginState?.getCompositesRole(
              roleType,
              skipMissing
            ) as T[]) ?? (frozenEmptyArray as T[])
          : (frozenEmptyArray as T[]);
      default:
        throw new Error('Unknown origin');
    }
  }

  setRole(roleType: RoleType, value: unknown) {
    if (roleType.objectType.isUnit) {
      this.setUnitRole(roleType, value as IUnit);
    } else if (roleType.isOne) {
      this.setCompositeRole(roleType, value as any);
    } else {
      this.setCompositesRole(roleType, value as any);
    }
  }

  setUnitRole(roleType: RoleType, value: IUnit) {
    this.assertUnit(roleType, value);

    switch (roleType.origin) {
      case Origin.Session:
        this.session.sessionOriginState.setUnitRole(
          this.object,
          roleType,
          value
        );
        break;
      case Origin.Database:
        if (this.canWrite(roleType)) {
          this.DatabaseOriginState?.setUnitRole(roleType, value);
        }

        break;
      default:
        throw new Error('Unsupported Origin');
    }
  }

  setCompositeRole<T extends IObject>(roleType: RoleType, value: T) {
    this.assertComposite(value);

    if (value != null) {
      this.assertSameType(roleType, value);
      this.assertSameSession(value);
    }

    if (roleType.isMany) {
      throw new Error('Wrong multiplicity');
    }

    switch (roleType.origin) {
      case Origin.Session:
        this.session.sessionOriginState.setCompositeRole(
          this.object,
          roleType,
          value
        );
        break;
      case Origin.Database:
        if (this.canWrite(roleType)) {
          this.DatabaseOriginState?.setCompositeRole(roleType, value);
        }

        break;
      default:
        throw new Error('Unsupported Origin');
    }
  }

  setCompositesRole(roleType: RoleType, role: ReadonlyArray<IObject>) {
    this.assertComposites(role);

    switch (roleType.origin) {
      case Origin.Session:
        this.session.sessionOriginState.setCompositesRole(
          this.object,
          roleType,
          this.session.ranges.importFrom(role)
        );
        break;

      case Origin.Database:
        if (this.canWrite(roleType)) {
          this.DatabaseOriginState?.setCompositesRole(
            roleType,
            this.session.ranges.importFrom(role)
          );
        }

        break;
      default:
        throw new Error('Unsupported Origin');
    }
  }

  addCompositesRole<T extends IObject>(roleType: RoleType, value: T) {
    if (value == null) {
      return;
    }

    this.assertComposite(value);

    this.assertSameType(roleType, value);

    if (roleType.isOne) {
      throw new Error('wrong multiplicity');
    }

    switch (roleType.origin) {
      case Origin.Session:
        this.session.sessionOriginState.addCompositesRole(
          this.object,
          roleType,
          value
        );
        break;
      case Origin.Database:
        if (this.canWrite(roleType)) {
          this.DatabaseOriginState.addCompositesRole(roleType, value);
        }

        break;
      default:
        throw new Error('Unsupported Origin');
    }
  }

  removeCompositesRole<T extends IObject>(roleType: RoleType, value: T) {
    if (value == null) {
      return;
    }

    this.assertComposite(value);

    switch (roleType.origin) {
      case Origin.Session:
        this.session.sessionOriginState.addCompositesRole(
          this.object,
          roleType,
          value
        );
        break;
      case Origin.Database:
        if (this.canWrite(roleType)) {
          this.DatabaseOriginState.removeCompositesRole(roleType, value);
        }

        break;
      default:
        throw new Error('Unsupported Origin');
    }
  }

  removeRole(roleType: RoleType) {
    if (roleType.objectType.isUnit) {
      this.setUnitRole(roleType, null);
    } else if (roleType.isOne) {
      this.setCompositeRole(roleType, null);
    } else {
      this.setCompositesRole(roleType, null);
    }
  }

  getCompositeAssociation<T extends IObject>(
    associationType: AssociationType
  ): T {
    if (associationType.origin != Origin.Session) {
      return (
        (this.session.getCompositeAssociation(
          this.object,
          associationType
        ) as T) ?? null
      );
    }

    return (
      (this.session.sessionOriginState.getCompositeRole(
        this.object,
        associationType
      ) as T) ?? null
    );
  }

  getCompositesAssociation<T extends IObject>(
    associationType: AssociationType
  ): T[] {
    if (associationType.origin != Origin.Session) {
      return this.session.getCompositesAssociation(
        this.object,
        associationType
      ) as T[];
    }

    return (
      (this.session.sessionOriginState.getCompositesRole(
        this.object,
        associationType
      ) as T[]) ?? (frozenEmptyArray as T[])
    );
  }

  canRead(roleType: RoleType): boolean {
    return roleType.origin === Origin.Database
      ? this.DatabaseOriginState?.canRead(roleType) ?? true
      : true;
  }

  canWrite(roleType: RoleType): boolean {
    return roleType.origin === Origin.Database
      ? this.DatabaseOriginState?.canWrite(roleType) ?? true
      : true;
  }

  canExecute(methodType: MethodType): boolean {
    return this.DatabaseOriginState?.canExecute(methodType);
  }

  isCompositeAssociationForRole(roleType: RoleType, forRole: IObject): boolean {
    const role = this.session.sessionOriginState.getCompositeRole(
      this.object,
      roleType
    );

    switch (roleType.origin) {
      case Origin.Session:
        return role === forRole;
      case Origin.Database:
        return (
          this.DatabaseOriginState?.isAssociationForRole(roleType, forRole) ??
          false
        );
      default:
        throw new Error('Unsupported Origin');
    }
  }

  isCompositesAssociationForRole(
    roleType: RoleType,
    forRole: IObject
  ): boolean {
    switch (roleType.origin) {
      case Origin.Session:
        return this.session.ranges.has(
          this.session.sessionOriginState.getCompositesRole(
            this.object,
            roleType
          ),
          forRole
        );
      case Origin.Database:
        return (
          this.DatabaseOriginState?.isAssociationForRole(roleType, forRole) ??
          false
        );
      default:
        throw new Error('Unsupported Origin');
    }
  }

  onDatabasePushNewId(newId: number) {
    this.id = newId;
  }

  onDatabasePushed() {
    this.DatabaseOriginState.onPushed();
  }

  assertSameType(roleType: RoleType, value: IObject) {
    const composite = roleType.objectType as Composite;
    if (!composite.isAssignableFrom(value.strategy.cls)) {
      throw new Error(
        `Types do not match: ${composite} and ${value.strategy.cls}`
      );
    }
  }

  assertSameSession(value: IObject) {
    if (this.session != value.strategy.session) {
      throw new Error('Sessions do not match');
    }
  }

  assertUnit(roleType: RoleType, value: unknown) {
    if (value == null) {
      return;
    }

    let error = false;

    switch (roleType.objectType.tag) {
      case UnitTags.Binary:
      case UnitTags.Decimal:
      case UnitTags.String:
      case UnitTags.Unique:
        error = typeof value !== 'string';
        break;
      case UnitTags.Boolean:
        error = typeof value !== 'boolean';
        break;
      case UnitTags.DateTime:
        error = !(value instanceof Date);
        break;
      case UnitTags.Float:
        error = isNaN(value as number);
        break;
      case UnitTags.Integer:
        error = !Number.isInteger(value as number);
        break;
    }

    if (error) {
      throw new Error(`value is not a ${roleType.objectType.singularName}`);
    }
  }

  assertComposite(value: IObject) {
    if (value == null) {
      return;
    }

    if (this.session != value.strategy.session) {
      throw new Error('Strategy is from a different session');
    }
  }

  assertComposites(inputs: ReadonlyArray<IObject>) {
    if (inputs == null) {
      return;
    }

    inputs.forEach((v) => this.assertComposite(v));
  }

  toString() {
    return JSON.stringify(this);
  }

  toJSON() {
    return {
      id: this.id,
    };
  }
}
