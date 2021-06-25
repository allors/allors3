import { IObject, IStrategy, Method } from '@allors/workspace/domain/system';
import { DatabaseRecord } from '../database/DatabaseRecord';
import { DatabaseOriginState, InitialVersion, UnknownVersion } from './originstate/DatabaseOriginState';
import { WorkspaceOriginState } from './originstate/WorkspaceOriginState';
import { isNewId, Session } from './Session';
import { Numbers, enumerate } from '../collections/Numbers';
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
        return this.DatabaseOriginState.Version;
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

  public exist(roleType: RoleType): boolean {
    if (roleType.objectType.isUnit) {
      return this.getUnit(roleType) != null;
    }

    if (roleType.isOne) {
      return this.getComposite(roleType) != null;
    }

    return this.getComposites(roleType)?.length > 0;
  }

  public get(roleType: RoleType): any {
    if (roleType.objectType.isUnit) {
      return this.getUnit(roleType);
    }

    if (roleType.isOne) {
      return this.getComposite(roleType);
    }

    return this.getComposites(roleType);
  }

  public getUnit(roleType: RoleType): any {
    switch (this.cls.origin) {
      case Origin.Session:
        return this.session.getRole(this, roleType);
      case Origin.Workspace:
        return this.WorkspaceOriginState?.getRole(roleType);
      case Origin.Database:
        if (this.DatabaseOriginState) {
          if (this.DatabaseOriginState.Version === UnknownVersion) {
            throw new Error('Uknown version');
          }

          if (this.canRead(roleType)) {
            return this.DatabaseOriginState?.getRole(roleType);
          }
        }

        return null;
      default:
        throw new Error('Unknown origin');
    }
  }

  public getComposite<T extends IObject>(roleType: RoleType) {
    switch (this.cls.origin) {
      case Origin.Session:
        return this.session.getRole(this, roleType);
      case Origin.Workspace:
        return this.WorkspaceOriginState?.getRole(roleType);
      case Origin.Database:
        if (this.DatabaseOriginState) {
          if (this.DatabaseOriginState.Version === UnknownVersion) {
            throw new Error('Uknown version');
          }

          if (this.canRead(roleType)) {
            return this.DatabaseOriginState?.getRole(roleType);
          }
        }

        return null;
      default:
        throw new Error('Unknown origin');
    }
  }

  public getComposites<T extends IObject>(roleType: RoleType): T[] {
    let roles: IObject[];

    switch (this.cls.origin) {
      case Origin.Session:
        roles = this.session.getRole(this, roleType) as IObject[];
        break;
      case Origin.Workspace:
        roles = this.WorkspaceOriginState?.getRole(roleType) as IObject[];
        break;
      case Origin.Database:
        if (this.DatabaseOriginState) {
          if (this.DatabaseOriginState.Version === UnknownVersion) {
            throw new Error('Uknown version');
          }

          if (this.canRead(roleType)) {
            roles = this.DatabaseOriginState?.getRole(roleType) as IObject[];
          }
        }

        break;
      default:
        throw new Error('Unknown origin');
    }

    return roles as T[];
  }

  public set(roleType: RoleType, value: any) {
    if (roleType.objectType.isUnit) {
      this.setUnit(roleType, value);
    } else if (roleType.isOne) {
      this.setComposite(roleType, value);
    } else {
      this.setComposites(roleType, value);
    }
  }

  public setUnit(roleType: RoleType, value: any) {
    switch (roleType.origin) {
      case Origin.Session:
        this.session.sessionOriginState.SetUnitRole(this.id, roleType, value);
        break;
      case Origin.Workspace:
        this.WorkspaceOriginState?.SetUnitRole(roleType, value);
        break;
      case Origin.Database:
        if (this.canWrite(roleType)) {
          this.DatabaseOriginState?.SetUnitRole(roleType, value);
        }

        break;
      default:
        throw new Error('Unsupported Origin');
    }
  }

  public setComposite<T extends IObject>(roleType: RoleType, value: T) {
    switch (roleType.origin) {
      case Origin.Session:
        this.session.sessionOriginState.SetCompositeRole(this.id, roleType, value?.id);
        break;
      case Origin.Workspace:
        this.WorkspaceOriginState?.SetCompositeRole(roleType, value?.id);
        break;
      case Origin.Database:
        if (this.canWrite(roleType)) {
          this.DatabaseOriginState?.SetCompositeRole(roleType, value?.id);
        }

        break;
      default:
        throw new Error('Unsupported Origin');
    }
  }

  public setComposites(roleType: RoleType, role: IObject[]) {
    const roleNumbers = Numbers(role?.map((v) => v.id));

    switch (roleType.origin) {
      case Origin.Session:
        this.session.sessionOriginState.SetCompositesRole(this.id, roleType, roleNumbers);
        break;

      case Origin.Workspace:
        this.WorkspaceOriginState?.SetCompositesRole(roleType, roleNumbers);

        break;

      case Origin.Database:
        if (this.canWrite(roleType)) {
          this.DatabaseOriginState?.SetCompositesRole(roleType, roleNumbers);
        }

        break;
      default:
        throw new Error('Unsupported Origin');
    }
  }

  public add(roleType: RoleType, value: any) {
    switch (roleType.origin) {
      case Origin.Session:
        this.session.sessionOriginState.AddRole(this.id, roleType, value.Id);
        break;
      case Origin.Workspace:
        this.WorkspaceOriginState.AddCompositeRole(roleType, value.Id);
        break;
      case Origin.Database:
        if (this.canWrite(roleType)) {
          this.DatabaseOriginState.AddCompositeRole(roleType, value.Id);
        }

        break;
      default:
        throw new Error('Unsupported Origin');
        break;
    }
  }

  public remove(roleType: RoleType, value: any) {
    switch (roleType.origin) {
      case Origin.Session:
        this.session.sessionOriginState.AddRole(this.id, roleType, value.Id);
        break;
      case Origin.Workspace:
        this.WorkspaceOriginState.RemoveCompositeRole(roleType, value.Id);
        break;
      case Origin.Database:
        if (this.canWrite(roleType)) {
          this.DatabaseOriginState.RemoveCompositeRole(roleType, value.Id);
        }

        break;
      default:
        throw new Error('Unsupported Origin');
    }
  }

  public removeAll(roleType: RoleType) {
    if (roleType.objectType.isUnit) {
      this.setUnit(roleType, null);
    } else if (roleType.isOne) {
      this.setComposite(roleType, null);
    } else {
      this.setComposites(roleType, null);
    }
  }

  public getCompositeAssociation<T extends IObject>(associationType: AssociationType): T {
    if (associationType.origin != Origin.Session) {
      return this.session.getCompositeAssociation(this.id, associationType);
    }

    const association = this.session.sessionOriginState.Get(this.id, associationType);
    return association != null ? this.session.getOne(association) : null;
  }

  public getCompositesAssociation<T extends IObject>(associationType: AssociationType): T[] {
    const composites: T[] = [];

    if (associationType.origin != Origin.Session) {
      composites.push(...this.session.getCompositesAssociation<T>(this.id, associationType));
    }

    const association = this.session.sessionOriginState.Get(this.id, associationType);

    for (const id of enumerate(association)) {
      composites.push(this.session.getOne(id));
    }

    return composites;
  }

  public canRead(roleType: RoleType): boolean {
    return this.DatabaseOriginState?.CanRead(roleType) ?? true;
  }

  public canWrite(roleType: RoleType): boolean {
    return this.DatabaseOriginState?.CanWrite(roleType) ?? true;
  }

  public canExecute(methodType: MethodType): boolean {
    return this.DatabaseOriginState?.CanExecute(methodType) ?? false;
  }

  method(methodType: MethodType): Method {
    return {
      object: this.object,
      methodType,
    };
  }

  public isAssociationForRole(roleType: RoleType, forRoleId: number): boolean {
    const role = this.session.sessionOriginState.Get(this.id, roleType);

    switch (roleType.origin) {
      case Origin.Session:
        return role === forRoleId;
      case Origin.Workspace:
        return this.WorkspaceOriginState?.IsAssociationForRole(roleType, forRoleId) ?? false;
      case Origin.Database:
        return this.DatabaseOriginState?.IsAssociationForRole(roleType, forRoleId) ?? false;
      default:
        throw new Error('Unsupported Origin');
    }
  }

  public onDatabasePushNewId(newId: number) {
    this.id = newId;
  }

  public onDatabasePushResponse(databaseRecord: DatabaseRecord) {
    this.DatabaseOriginState.PushResponse(databaseRecord);
  }
}
