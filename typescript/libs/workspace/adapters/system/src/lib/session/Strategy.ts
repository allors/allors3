import { IDiff, IObject, IStrategy, IUnit } from '@allors/workspace/domain/system';
import { DatabaseOriginState } from './originstate/DatabaseOriginState';
import { WorkspaceOriginState } from './originstate/WorkspaceOriginState';
import { isNewId, Session } from './Session';
import { importFrom, enumerate, IRange, has } from '../collections/Range';
import { AssociationType, Class, Composite, MethodType, Origin, RoleType } from '@allors/workspace/meta/system';
import { WorkspaceInitialVersion } from '../Version';

export abstract class Strategy implements IStrategy {
  DatabaseOriginState: DatabaseOriginState;
  WorkspaceOriginState: WorkspaceOriginState;

  private _object: IObject;

  constructor(public session: Session, public cls: Class, public id: number) {
    if (this.cls.origin !== Origin.Session) {
      this.WorkspaceOriginState = new WorkspaceOriginState(this, this.session.workspace.getRecord(this.id));
    }
  }

  get version(): number {
    switch (this.cls.origin) {
      case Origin.Session:
        return WorkspaceInitialVersion;
      case Origin.Workspace:
        return this.WorkspaceOriginState.Version;
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
    switch (this.cls.origin) {
      case Origin.Session:
        return this.session.sessionOriginState.getUnitRole(this.id, roleType);
      case Origin.Workspace:
        return this.WorkspaceOriginState?.getUnitRole(roleType);
      case Origin.Database:
        return this.canRead(roleType) ? this.DatabaseOriginState?.getUnitRole(roleType) : null;
      default:
        throw new Error('Unknown origin');
    }
  }

  getCompositeRole<T extends IObject>(roleType: RoleType): T {
    let roleId: number;
    switch (this.cls.origin) {
      case Origin.Session:
        roleId = this.session.sessionOriginState.getCompositeRole(this.id, roleType) as number;
        break;
      case Origin.Workspace:
        roleId = this.WorkspaceOriginState?.getCompositeRole(roleType);
        break;
      case Origin.Database:
        roleId = this.canRead(roleType) ? this.DatabaseOriginState?.getCompositeRole(roleType) : null;
        break;
      default:
        throw new Error('Unknown origin');
    }

    return this.session.instantiate<T>(roleId);
  }

  getCompositesRole<T extends IObject>(roleType: RoleType): T[] {
    let roleIds: IRange;

    switch (this.cls.origin) {
      case Origin.Session:
        roleIds = this.session.sessionOriginState.getCompositesRole(this.id, roleType) as IRange;
        break;
      case Origin.Workspace:
        roleIds = this.WorkspaceOriginState?.getCompositesRole(roleType);
        break;
      case Origin.Database:
        roleIds = this.canRead(roleType) ? this.DatabaseOriginState?.getCompositesRole(roleType) : null;
        break;
      default:
        throw new Error('Unknown origin');
    }

    return this.session.instantiate<T>(roleIds);
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
        this.session.sessionOriginState.setUnitRole(this.id, roleType, value);
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
        this.session.sessionOriginState.setCompositeRole(this.id, roleType, value?.id);
        break;
      case Origin.Workspace:
        this.WorkspaceOriginState?.setCompositeRole(roleType, value?.id);
        break;
      case Origin.Database:
        if (this.canWrite(roleType)) {
          this.DatabaseOriginState?.setCompositeRole(roleType, value?.id);
        }

        break;
      default:
        throw new Error('Unsupported Origin');
    }
  }

  setCompositesRole(roleType: RoleType, role: ReadonlyArray<IObject>) {
    this.assertComposites(role);

    const roleIds = importFrom(role?.map((v) => v.id));

    switch (roleType.origin) {
      case Origin.Session:
        this.session.sessionOriginState.setCompositesRole(this.id, roleType, roleIds);
        break;

      case Origin.Workspace:
        this.WorkspaceOriginState?.setCompositesRole(roleType, roleIds);

        break;

      case Origin.Database:
        if (this.canWrite(roleType)) {
          this.DatabaseOriginState?.setCompositesRole(roleType, roleIds);
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
        this.session.sessionOriginState.addCompositesRole(this.id, roleType, value.id);
        break;
      case Origin.Workspace:
        this.WorkspaceOriginState.addCompositesRole(roleType, value.id);
        break;
      case Origin.Database:
        if (this.canWrite(roleType)) {
          this.DatabaseOriginState.addCompositesRole(roleType, value.id);
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
        this.session.sessionOriginState.addCompositesRole(this.id, roleType, value.id);
        break;
      case Origin.Workspace:
        this.WorkspaceOriginState.removeCompositesRole(roleType, value.id);
        break;
      case Origin.Database:
        if (this.canWrite(roleType)) {
          this.DatabaseOriginState.removeCompositesRole(roleType, value.id);
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
      return this.session.getCompositeAssociation(this.id, associationType)?.object as T;
    }

    const association = this.session.sessionOriginState.getCompositeRole(this.id, associationType);
    return association != null ? this.session.instantiate(association) : null;
  }

  getCompositesAssociation<T extends IObject>(associationType: AssociationType): T[] {
    const composites: T[] = [];

    if (associationType.origin != Origin.Session) {
      composites.push(...this.session.getCompositesAssociation(this.id, associationType).map((v) => v.object as T));
    }

    const association = this.session.sessionOriginState.getCompositesRole(this.id, associationType);

    for (const id of enumerate(association)) {
      composites.push(this.session.instantiate(id));
    }

    return composites;
  }

  canRead(roleType: RoleType): boolean {
    return this.DatabaseOriginState?.canRead(roleType) ?? true;
  }

  canWrite(roleType: RoleType): boolean {
    return this.DatabaseOriginState?.canWrite(roleType) ?? true;
  }

  canExecute(methodType: MethodType): boolean {
    return this.DatabaseOriginState?.canExecute(methodType) ?? false;
  }

  isCompositeAssociationForRole(roleType: RoleType, forRoleId: number): boolean {
    const role = this.session.sessionOriginState.getCompositeRole(this.id, roleType);

    switch (roleType.origin) {
      case Origin.Session:
        return role === forRoleId;
      case Origin.Workspace:
        return this.WorkspaceOriginState?.isAssociationForRole(roleType, forRoleId) ?? false;
      case Origin.Database:
        return this.DatabaseOriginState?.isAssociationForRole(roleType, forRoleId) ?? false;
      default:
        throw new Error('Unsupported Origin');
    }
  }

  isCompositesAssociationForRole(roleType: RoleType, forRoleId: number): boolean {
    switch (roleType.origin) {
      case Origin.Session:
        return has(this.session.sessionOriginState.getCompositesRole(this.id, roleType), forRoleId);
      case Origin.Workspace:
        return this.WorkspaceOriginState?.isAssociationForRole(roleType, forRoleId) ?? false;
      case Origin.Database:
        return this.DatabaseOriginState?.isAssociationForRole(roleType, forRoleId) ?? false;
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
    // TODO:
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
