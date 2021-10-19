import { IDiff, IObject, IStrategy, IUnit } from '@allors/workspace/domain/system';
import { AssociationType, Class, Composite, MethodType, Origin, RoleType, UnitTags } from '@allors/workspace/meta/system';

import { DatabaseOriginState } from './originstate/database-origin-state';
import { WorkspaceOriginState } from './originstate/workspace-origin-state';
import { frozenEmptyArray } from '../collections/frozen-empty-array';
import { isNewId, Session } from './session';
import { WorkspaceInitialVersion } from '../version';
import { DateTime } from '../../../../../meta/apps/src/lib/generated/m.g';

export abstract class Strategy implements IStrategy {
  DatabaseOriginState: DatabaseOriginState;
  WorkspaceOriginState: WorkspaceOriginState;

  rangeId: number;
  private _object: IObject;

  constructor(public session: Session, public cls: Class, public id: number) {
    if (this.cls.origin !== Origin.Session) {
      this.WorkspaceOriginState = new WorkspaceOriginState(this, this.session.workspace.getRecord(this.id));
    }

    this.rangeId = id;
  }
  delete() {
    throw new Error('Method not implemented.');
  }

  get version(): number {
    switch (this.cls.origin) {
      case Origin.Session:
        return WorkspaceInitialVersion;
      case Origin.Workspace:
        return this.WorkspaceOriginState.version;
      case Origin.Database:
        return this.DatabaseOriginState.version;
      default:
        throw new Error('Unknown origin');
    }
  }

  get isNew(): boolean {
    return isNewId(this.id);
  }

  get object(): IObject {
    return (this._object ??= this.session.workspace.database.configuration.objectFactory.create(this));
  }

  reset(): void {
    this.WorkspaceOriginState?.reset();
    this.DatabaseOriginState?.reset();
  }

  diff(): IDiff[] {
    const diffs: IDiff[] = [];
    this.WorkspaceOriginState.diff(diffs);
    this.DatabaseOriginState.diff(diffs);
    return diffs;
  }

  get hasChanges(): boolean {
    return this.session.sessionOriginState.hasChanges(this) || this.WorkspaceOriginState?.hasChanges || this.DatabaseOriginState?.hasChanges;
  }

  hasChanged(roleType: RoleType): boolean {
    switch (roleType.origin) {
      case Origin.Session:
        return this.session.sessionOriginState.hasChanged(this, roleType);
      case Origin.Workspace:
        return this.WorkspaceOriginState?.hasChanged(roleType) ?? false;
      case Origin.Database:
        return this.canRead(roleType) ? this.DatabaseOriginState?.hasChanged(roleType) ?? false : false;
      default:
        throw new Error('Unknown origin');
    }
  }

  restoreRole(roleType: RoleType) {
    switch (roleType.origin) {
      case Origin.Session:
        return this.session.sessionOriginState.restoreRole(this, roleType);
      case Origin.Workspace:
        return this.WorkspaceOriginState?.restoreRole(roleType);
      case Origin.Database:
        return this.canRead(roleType) ? this.DatabaseOriginState?.restoreRole(roleType) : false;
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
    switch (roleType.origin) {
      case Origin.Session:
        return this.session.sessionOriginState.getUnitRole(this, roleType) ?? null;
      case Origin.Workspace:
        return this.WorkspaceOriginState?.getUnitRole(roleType) ?? null;
      case Origin.Database:
        return (this.canRead(roleType) ? this.DatabaseOriginState?.getUnitRole(roleType) : null) ?? null;
      default:
        throw new Error('Unknown origin');
    }
  }

  getCompositeRole<T extends IObject>(roleType: RoleType, skipMissing?: boolean): T {
    switch (roleType.origin) {
      case Origin.Session:
        return (this.session.sessionOriginState.getCompositeRole(this, roleType)?.object as T) ?? null;
      case Origin.Workspace:
        return (this.WorkspaceOriginState?.getCompositeRole(roleType)?.object as T) ?? null;
      case Origin.Database:
        return this.canRead(roleType) ? (this.DatabaseOriginState?.getCompositeRole(roleType, skipMissing)?.object as T) ?? null : null;
      default:
        throw new Error('Unknown origin');
    }
  }

  getCompositesRole<T extends IObject>(roleType: RoleType, skipMissing?: boolean): T[] {
    switch (roleType.origin) {
      case Origin.Session:
        return this.session.sessionOriginState.getCompositesRole(this, roleType)?.map((v) => v.object as T) ?? (frozenEmptyArray as T[]);
      case Origin.Workspace:
        return (this.WorkspaceOriginState?.getCompositesRole(roleType)?.map((v) => v.object) as T[]) ?? (frozenEmptyArray as T[]);
      case Origin.Database:
        return this.canRead(roleType) ? (this.DatabaseOriginState?.getCompositesRole(roleType, skipMissing)?.map((v) => v.object) as T[]) ?? (frozenEmptyArray as T[]) : (frozenEmptyArray as T[]);
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
        this.session.sessionOriginState.setUnitRole(this, roleType, value);
        break;
      case Origin.Workspace:
        this.WorkspaceOriginState?.setUnitRole(roleType, value);
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
        this.session.sessionOriginState.setCompositeRole(this, roleType, value?.strategy as Strategy);
        break;
      case Origin.Workspace:
        this.WorkspaceOriginState?.setCompositeRole(roleType, value?.strategy as Strategy);
        break;
      case Origin.Database:
        if (this.canWrite(roleType)) {
          this.DatabaseOriginState?.setCompositeRole(roleType, value?.strategy as Strategy);
        }

        break;
      default:
        throw new Error('Unsupported Origin');
    }
  }

  setCompositesRole(roleType: RoleType, role: ReadonlyArray<IObject>) {
    this.assertComposites(role);

    const roleStrategies = role?.map((v) => v.strategy) as Strategy[];

    switch (roleType.origin) {
      case Origin.Session:
        this.session.sessionOriginState.setCompositesRole(this, roleType, this.session.ranges.importFrom(roleStrategies));
        break;

      case Origin.Workspace:
        this.WorkspaceOriginState?.setCompositesRole(roleType, roleStrategies);

        break;

      case Origin.Database:
        if (this.canWrite(roleType)) {
          this.DatabaseOriginState?.setCompositesRole(roleType, roleStrategies);
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
        this.session.sessionOriginState.addCompositesRole(this, roleType, value.strategy as Strategy);
        break;
      case Origin.Workspace:
        this.WorkspaceOriginState.addCompositesRole(roleType, value.strategy as Strategy);
        break;
      case Origin.Database:
        if (this.canWrite(roleType)) {
          this.DatabaseOriginState.addCompositesRole(roleType, value.strategy as Strategy);
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
        this.session.sessionOriginState.addCompositesRole(this, roleType, value.strategy as Strategy);
        break;
      case Origin.Workspace:
        this.WorkspaceOriginState.removeCompositesRole(roleType, value.strategy as Strategy);
        break;
      case Origin.Database:
        if (this.canWrite(roleType)) {
          this.DatabaseOriginState.removeCompositesRole(roleType, value.strategy as Strategy);
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

  getCompositeAssociation<T extends IObject>(associationType: AssociationType): T {
    if (associationType.origin != Origin.Session) {
      return (this.session.getCompositeAssociation(this, associationType)?.object as T) ?? null;
    }

    return (this.session.sessionOriginState.getCompositeRole(this, associationType)?.object as T) ?? null;
  }

  getCompositesAssociation<T extends IObject>(associationType: AssociationType): T[] {
    if (associationType.origin != Origin.Session) {
      return this.session.getCompositesAssociation(this, associationType).map((v) => v.object as T);
    }

    return this.session.sessionOriginState.getCompositesRole(this, associationType)?.map((v) => v.object as T) ?? (frozenEmptyArray as T[]);
  }

  canRead(roleType: RoleType): boolean {
    return roleType.origin === Origin.Database ? this.DatabaseOriginState?.canRead(roleType) ?? true : true;
  }

  canWrite(roleType: RoleType): boolean {
    return roleType.origin === Origin.Database ? this.DatabaseOriginState?.canWrite(roleType) ?? true : true;
  }

  canExecute(methodType: MethodType): boolean {
    return methodType.origin === Origin.Database ? this.DatabaseOriginState?.canExecute(methodType) ?? false : false;
  }

  isCompositeAssociationForRole(roleType: RoleType, forRole: Strategy): boolean {
    const role = this.session.sessionOriginState.getCompositeRole(this, roleType);

    switch (roleType.origin) {
      case Origin.Session:
        return role === forRole;
      case Origin.Workspace:
        return this.WorkspaceOriginState?.isAssociationForRole(roleType, forRole) ?? false;
      case Origin.Database:
        return this.DatabaseOriginState?.isAssociationForRole(roleType, forRole) ?? false;
      default:
        throw new Error('Unsupported Origin');
    }
  }

  isCompositesAssociationForRole(roleType: RoleType, forRole: Strategy): boolean {
    switch (roleType.origin) {
      case Origin.Session:
        return this.session.ranges.has(this.session.sessionOriginState.getCompositesRole(this, roleType), forRole);
      case Origin.Workspace:
        return this.WorkspaceOriginState?.isAssociationForRole(roleType, forRole) ?? false;
      case Origin.Database:
        return this.DatabaseOriginState?.isAssociationForRole(roleType, forRole) ?? false;
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
      throw new Error(`Types do not match: ${composite} and ${value.strategy.cls}`);
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
      case UnitTags.Float:
        error = isNaN(value as number);
        break;
      case UnitTags.Integer:
        error = !Number.isInteger(value as number);
        break;
      case UnitTags.DateTime:
        // No checks: 
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
