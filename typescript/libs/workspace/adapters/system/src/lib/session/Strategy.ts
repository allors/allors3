import { IObject, IStrategy, IUnit, Method } from '@allors/workspace/domain/system';
import { DatabaseRecord } from '../database/DatabaseRecord';
import { DatabaseOriginState, InitialVersion, UnknownVersion } from './originstate/DatabaseOriginState';
import { WorkspaceOriginState } from './originstate/WorkspaceOriginState';
import { isNewId, Session } from './Session';
import { importFrom, enumerate, IRange, has } from '../collections/Range';
import { AssociationType, Class, MethodType, Origin, RoleType } from '@allors/workspace/meta/system';

export abstract class Strategy implements IStrategy {
  DatabaseOriginState: DatabaseOriginState;
  WorkspaceOriginState: WorkspaceOriginState;

  private _object: IObject;

  constructor(public session: Session, public cls: Class, public id: number) {
    if (this.cls.origin !== Origin.Session) {
      this.WorkspaceOriginState = new WorkspaceOriginState(this, this.session.workspace.getRecord(this.id));
    }
  }

  public get version(): number {
    switch (this.cls.origin) {
      case Origin.Session:
        return InitialVersion;
      case Origin.Workspace:
        return this.WorkspaceOriginState.Version;
      case Origin.Database:
        return this.DatabaseOriginState.version;
      default:
        throw new Error('Unknown origin');
    }
  }

  public get isNew(): boolean {
    return isNewId(this.id);
  }

  public get object(): IObject {
    return (this._object ??= this.session.workspace.database.configuration.objectFactory.create(this));
  }

  public existRole(roleType: RoleType): boolean {
    if (roleType.objectType.isUnit) {
      return this.getUnitRole(roleType) != null;
    }

    if (roleType.isOne) {
      return this.getCompositeRole(roleType) != null;
    }

    return this.getCompositesRole(roleType)?.length > 0;
  }

  public getRole(roleType: RoleType): unknown {
    if (roleType.objectType.isUnit) {
      return this.getUnitRole(roleType);
    }

    if (roleType.isOne) {
      return this.getCompositeRole(roleType);
    }

    return this.getCompositesRole(roleType);
  }

  public getUnitRole(roleType: RoleType): IUnit {
    switch (this.cls.origin) {
      case Origin.Session:
        return this.session.getRole(this, roleType) as IUnit;
      case Origin.Workspace:
        return this.WorkspaceOriginState?.getUnitRole(roleType);
      case Origin.Database:
        if (this.DatabaseOriginState) {
          if (this.DatabaseOriginState.version === UnknownVersion) {
            throw new Error('Uknown version');
          }

          if (this.canRead(roleType)) {
            return this.DatabaseOriginState?.getUnitRole(roleType);
          }
        }

        return null;
      default:
        throw new Error('Unknown origin');
    }
  }

  public getCompositeRole<T extends IObject>(roleType: RoleType): T {
    let roleId: number;
    switch (this.cls.origin) {
      case Origin.Session:
        roleId = this.session.getRole(this, roleType) as number;
        break;
      case Origin.Workspace:
        roleId = this.WorkspaceOriginState?.getCompositeRole(roleType);
        break;
      case Origin.Database:
        if (this.DatabaseOriginState) {
          if (this.DatabaseOriginState.version === UnknownVersion) {
            throw new Error('Uknown version');
          }

          if (this.canRead(roleType)) {
            roleId = this.DatabaseOriginState?.getCompositeRole(roleType);
          }
        }

        break;
      default:
        throw new Error('Unknown origin');
    }

    return this.session.getOne<T>(roleId);
  }

  public getCompositesRole<T extends IObject>(roleType: RoleType): T[] {
    let roleIds: IRange;

    switch (this.cls.origin) {
      case Origin.Session:
        roleIds = this.session.getRole(this, roleType) as IRange;
        break;
      case Origin.Workspace:
        roleIds = this.WorkspaceOriginState?.getCompositesRole(roleType);
        break;
      case Origin.Database:
        if (this.DatabaseOriginState) {
          if (this.DatabaseOriginState.version === UnknownVersion) {
            throw new Error('Uknown version');
          }

          if (this.canRead(roleType)) {
            roleIds = this.DatabaseOriginState?.getCompositesRole(roleType);
          }
        }

        break;
      default:
        throw new Error('Unknown origin');
    }

    return this.session.getMany<T>(roleIds);
  }

  public setRole(roleType: RoleType, value: unknown) {
    if (roleType.objectType.isUnit) {
      this.setUnitRole(roleType, value as IUnit);
    } else if (roleType.isOne) {
      this.setCompositeRole(roleType, value as any);
    } else {
      this.setCompositesRole(roleType, value as any);
    }
  }

  public setUnitRole(roleType: RoleType, value: IUnit) {
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

  public setCompositeRole<T extends IObject>(roleType: RoleType, value: T) {
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

  public setCompositesRole(roleType: RoleType, role: ReadonlyArray<IObject>) {
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

  public addCompositesRole<T extends IObject>(roleType: RoleType, value: T) {
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

  public removeCompositesRole<T extends IObject>(roleType: RoleType, value: T) {
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

  public removeRole(roleType: RoleType) {
    if (roleType.objectType.isUnit) {
      this.setUnitRole(roleType, null);
    } else if (roleType.isOne) {
      this.setCompositeRole(roleType, null);
    } else {
      this.setCompositesRole(roleType, null);
    }
  }

  public getCompositeAssociation<T extends IObject>(associationType: AssociationType): T {
    if (associationType.origin != Origin.Session) {
      return this.session.getCompositeAssociation(this.id, associationType);
    }

    const association = this.session.sessionOriginState.getCompositeRole(this.id, associationType);
    return association != null ? this.session.getOne(association) : null;
  }

  public getCompositesAssociation<T extends IObject>(associationType: AssociationType): T[] {
    const composites: T[] = [];

    if (associationType.origin != Origin.Session) {
      composites.push(...this.session.getCompositesAssociation<T>(this.id, associationType));
    }

    const association = this.session.sessionOriginState.getCompositesRole(this.id, associationType);

    for (const id of enumerate(association)) {
      composites.push(this.session.getOne(id));
    }

    return composites;
  }

  public canRead(roleType: RoleType): boolean {
    return this.DatabaseOriginState?.canRead(roleType) ?? true;
  }

  public canWrite(roleType: RoleType): boolean {
    return this.DatabaseOriginState?.canWrite(roleType) ?? true;
  }

  public canExecute(methodType: MethodType): boolean {
    return this.DatabaseOriginState?.canExecute(methodType) ?? false;
  }

  method(methodType: MethodType): Method {
    return {
      object: this.object,
      methodType,
    };
  }

  public isCompositeAssociationForRole(roleType: RoleType, forRoleId: number): boolean {
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

  public isCompositesAssociationForRole(roleType: RoleType, forRoleId: number): boolean {
    switch (roleType.origin) {
      case Origin.Session:
        return has(this.session.sessionOriginState.getCompositesRole(this.id, roleType),  forRoleId);
      case Origin.Workspace:
        return this.WorkspaceOriginState?.isAssociationForRole(roleType, forRoleId) ?? false;
      case Origin.Database:
        return this.DatabaseOriginState?.isAssociationForRole(roleType, forRoleId) ?? false;
      default:
        throw new Error('Unsupported Origin');
    }
  }

  public onDatabasePushNewId(newId: number) {
    this.id = newId;
  }

  public onDatabasePushResponse(databaseRecord: DatabaseRecord) {
    this.DatabaseOriginState.pushResponse(databaseRecord);
  }
}
