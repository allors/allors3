import { IObject, IStrategy } from '@allors/workspace/domain/system';
import { AssociationType, Class, MethodType, Origin, RoleType } from '@allors/workspace/meta/system';
import { DatabaseRecord } from '../database/DatabaseRecord';
import { DatabaseOriginState, InitialVersion, UnknownVersion } from './originstate/DatabaseOriginState';
import { WorkspaceOriginState } from './originstate/WorkspaceOriginState';
import { isNewId, Session } from './Session';
import { Numbers, enumerate } from '../collections/Numbers';

export class Strategy implements IStrategy {
  DatabaseOriginState: DatabaseOriginState;
  WorkspaceOriginState: WorkspaceOriginState;

  private _object: IObject;

  constructor(public session: Session, public cls: Class, public id: number) {
    if (this.cls.origin !== Origin.Session) {
      this.WorkspaceOriginState = new WorkspaceOriginState(this, this.session.workspace.getRecord(this.id));
    }
  }

  static fromDatabaseRecord(session: Session, databaseRecord: DatabaseRecord) {
    return new Strategy(session, databaseRecord.cls, databaseRecord.id);
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

    return this.getComposites(roleType).Any();
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
        return this.session.GetRole(this, roleType);
      case Origin.Workspace:
        return this.WorkspaceOriginState?.GetRole(roleType);
      case Origin.Database:
        if (this.DatabaseOriginState) {
          if (this.DatabaseOriginState.Version === UnknownVersion) {
            throw new Error('Uknown version');
          }

          if (this.canRead(roleType)) {
            return this.DatabaseOriginState?.GetRole(roleType);
          }
        }

        return null;
      default:
        throw new Error('Unknown origin');
    }
  }

  public getComposite(roleType: RoleType) {
    switch (this.cls.origin) {
      case Origin.Session:
        return this.session.GetRole(this, roleType);
      case Origin.Workspace:
        return this.WorkspaceOriginState?.GetRole(roleType);
      case Origin.Database:
        if (this.DatabaseOriginState) {
          if (this.DatabaseOriginState.Version === UnknownVersion) {
            throw new Error('Uknown version');
          }

          if (this.canRead(roleType)) {
            return this.DatabaseOriginState?.GetRole(roleType);
          }
        }

        return null;
      default:
        throw new Error('Unknown origin');
    }
  }

  public *getComposites<T>(roleType: RoleType): Generator<T, void, unknown> {
    let roles: IObject[];

    switch (this.cls.origin) {
      case Origin.Session:
        roles = this.session.GetRole(this, roleType) as IObject[];
        break;
      case Origin.Workspace:
        roles = this.WorkspaceOriginState?.GetRole(roleType) as IObject[];
        break;
      case Origin.Database:
        if (this.DatabaseOriginState) {
          if (this.DatabaseOriginState.Version === UnknownVersion) {
            throw new Error('Uknown version');
          }

          if (this.canRead(roleType)) {
            roles = this.DatabaseOriginState?.GetRole(roleType) as IObject[];
          }
        }

        break;
      default:
        throw new Error('Unknown origin');
    }

    if (roles != null) {
      for (const role of roles) {
        yield role;
      }
    }
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

  public getCompositeAssociation(associationType: AssociationType) {
    if (associationType.origin != Origin.Session) {
      return this.session.GetCompositeAssociation(this.id, associationType);
    }

    let association = this.session.sessionOriginState.Get(this.id, associationType);
    return association != null ? this.session.getOne(association) : null;
  }

  public *getCompositesAssociation<T>(associationType: AssociationType):  Generator<T, void, unknown> {
    if (associationType.origin != Origin.Session) {
      return this.session.GetCompositesAssociation(this.id, associationType);
    }

    const association = this.session.sessionOriginState.Get(this.id, associationType);

    for (const id of enumerate(association)) {
      yield this.session.getOne(id);
    }
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
