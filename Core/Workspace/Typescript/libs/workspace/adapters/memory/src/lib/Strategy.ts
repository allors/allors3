import { IObject, IStrategy, UnitTypes } from '@allors/workspace/domain/system';
import {
  AssociationType,
  MethodType,
  RoleType,
} from '@allors/workspace/meta/system';

export class Strategy implements IStrategy {
  private _obj: IObject;

  private workspaceState: WorkspaceState;
  private databaseState: DatabaseState;

  // constructor(Session session, IClass @class, long identity)
  // {
  //     this.Session = session;
  //     this.Id = identity;
  //     this.Class = @class;

  //     if (!this.Class.HasSessionOrigin)
  //     {
  //         this.workspaceState = new WorkspaceState(this);
  //     }

  //     if (this.Class.HasDatabaseOrigin)
  //     {
  //         this.databaseState = new DatabaseState(this);
  //     }
  // }

  // constructor(Session session, DatabaseObject databaseObject)
  // {
  //     this.Session = session;
  //     this.Id = databaseObject.Identity;
  //     this.Class = databaseObject.Class;

  //     this.workspaceState = new WorkspaceState(this);
  //     this.databaseState = new DatabaseState(this, databaseObject);
  // }

  session: Session;

  class: IClass;

  id: number;

  get object(): IObject {
    return (this._obj ??= this.session.Workspace.ObjectFactory.Create(this));
  }

  get hasDatabaseChanges(): boolean {
    return this.databaseState.HasDatabaseChanges;
  }

  get databaseVersion(): number {
    return this.databaseState.Version;
  }

  diff(): IRelationType[] {
    // if (this.workspaceState != null)
    // {
    //     foreach (var diff in this.workspaceState.Diff())
    //     {
    //         yield return diff;
    //     }
    // }
    // if (this.databaseState == null)
    // {
    //     yield break;
    // }
    // foreach (var diff in this.databaseState.Diff())
    // {
    //     yield return diff;
    // }
  }

  exist(roleType: RoleType): boolean {
    // if (roleType.ObjectType.IsUnit)
    // {
    //     return this.GetUnit(roleType) != null;
    // }
    // if (roleType.IsOne)
    // {
    //     return this.GetComposite<IObject>(roleType) != null;
    // }
    // return this.GetComposites<IObject>(roleType).Any();
  }

  get(roleType: RoleType): unknown {
    // if (roleType.ObjectType.IsUnit)
    // {
    //     return this.GetUnit(roleType);
    // }
    // if (roleType.IsOne)
    // {
    //     return this.GetComposite<IObject>(roleType);
    // }
    // return this.GetComposites<IObject>(roleType);
  }

  getUnit(roleType: RoleType): UnitTypes {
    // roleType.Origin switch
    // {
    //     Origin.Session => this.Session.GetRole(this, roleType),
    //     Origin.Workspace => this.workspaceState?.GetRole(roleType),
    //     Origin.Database => this.databaseState?.GetRole(roleType),
    //     _ => throw new ArgumentException("Unsupported Origin")
    // };
  }

  getComposite<T>(roleType: RoleType): T {
    // roleType.Origin switch
    // {
    //     Origin.Session => (T)this.Session.GetRole(this, roleType),
    //     Origin.Workspace => (T)this.workspaceState?.GetRole(roleType),
    //     Origin.Database => (T)this.databaseState?.GetRole(roleType),
    //     _ => throw new ArgumentException("Unsupported Origin")
    // };
  }

  getComposites<T>(roleType: RoleType): T[] {
    // var roles = roleType.Origin switch
    // {
    //     Origin.Session => this.Session.GetRole(this, roleType),
    //     Origin.Workspace => this.workspaceState?.GetRole(roleType),
    //     Origin.Database => this.databaseState?.GetRole(roleType),
    //     _ => throw new ArgumentException("Unsupported Origin")
    // };
    // if (roles != null)
    // {
    //     foreach (var role in (IObject[])roles)
    //     {
    //         yield return (T)role;
    //     }
    // }
  }

  set(roleType: RoleType, value: unknown): void {
    // if (roleType.ObjectType.IsUnit)
    // {
    //     this.SetUnit(roleType, value);
    // }
    // else
    // {
    //     if (roleType.IsOne)
    //     {
    //         this.SetComposite(roleType, (IObject)value);
    //     }
    //     else
    //     {
    //         this.SetComposites(roleType, (IEnumerable<IObject>)value);
    //     }
    // }
  }

  setUnit(roleType: RoleType, value: UnitTypes): void {
    // switch (roleType.Origin)
    // {
    //     case Origin.Session:
    //         this.Session.SessionState.SetUnitRole(this, roleType, value);
    //         break;
    //     case Origin.Workspace:
    //         this.workspaceState?.SetUnitRole(roleType, value);
    //         break;
    //     case Origin.Database:
    //         this.databaseState?.SetUnitRole(roleType, value);
    //         break;
    //     default:
    //         throw new ArgumentException("Unsupported Origin");
    // }
  }

  setComposite<T>(roleType: RoleType, value: T): void {
    // switch (roleType.Origin)
    // {
    //     case Origin.Session:
    //         this.Session.SessionState.SetCompositeRole(this, roleType, value);
    //         break;
    //     case Origin.Workspace:
    //         this.workspaceState?.SetCompositeRole(roleType, value);
    //         break;
    //     case Origin.Database:
    //         this.databaseState?.SetCompositeRole(roleType, value);
    //         break;
    //     default:
    //         throw new ArgumentException("Unsupported Origin");
    // }
  }

  setComposites<T>(roleType: RoleType, value: T[]): void {
    // switch (roleType.Origin)
    // {
    //     case Origin.Session:
    //         this.Session.SessionState.SetCompositesRole(this, roleType, value);
    //         break;
    //     case Origin.Workspace:
    //         this.workspaceState?.SetCompositesRole(roleType, value);
    //         break;
    //     case Origin.Database:
    //         this.databaseState?.SetCompositesRole(roleType, value);
    //         break;
    //     default:
    //         throw new ArgumentException("Unsupported Origin");
    // }
  }

  add<T>(roleType: RoleType, value: T): void {
    //     if (!this.GetComposites<IObject>(roleType).Contains(value))
    //     {
    //         var roles = this.GetComposites<IObject>(roleType).Append(value).ToArray();
    //         this.Set(roleType, roles);
    //     }
    //
  }

  remove<T>(roleType: RoleType, value: T): void {
    // if (!this.GetComposites<IObject>(roleType).Contains(value))
    // {
    //     return;
    // }
    // var roles = this.GetComposites<IObject>(roleType).Where(v => !v.Equals(value)).ToArray();
    // this.Set(roleType, roles);
  }

  remove(roleType: RoleType): void {
    // if (roleType.ObjectType.IsUnit)
    // {
    //     this.SetUnit(roleType, null);
    // }
    // else
    // {
    //     if (roleType.IsOne)
    //     {
    //         this.SetComposite(roleType, (IObject)null);
    //     }
    //     else
    //     {
    //         this.SetComposites(roleType, (IEnumerable<IObject>)null);
    //     }
    // }
  }

  getCompositeAssociation<T>(associationType: AssociationType): T {
    // if (associationType.Origin != Origin.Session)
    // {
    //     return this.Session.GetAssociation<T>(this, associationType).FirstOrDefault();
    // }
    // this.Session.SessionState.GetAssociation(this, associationType, out var association);
    // var id = (long?)association;
    // return id != null ? this.Session.Get<T>(id) : default;
  }

  getCompositesAssociation<T>(associationType: AssociationType): T[] {
    // if (associationType.Origin != Origin.Session)
    // {
    //     return this.Session.GetAssociation<T>(this, associationType);
    // }
    // this.Session.SessionState.GetAssociation(this, associationType, out var association);
    // var ids = (IEnumerable<long>)association;
    // return ids?.Select(v => this.Session.Get<T>(v)).ToArray() ?? Array.Empty<T>();
  }

  canRead(roleType: RoleType): boolean {
    return this.databaseState?.canRead(roleType) ?? true;
  }

  canWrite(roleType: RoleType): boolean {
    return this.databaseState?.canWrite(roleType) ?? true;
  }

  canExecute(methodType: MethodType): boolean {
    return this.databaseState?.canExecute(methodType) ?? false;
  }

  reset(): void {
    this.workspaceState?.Reset();
    this.databaseState?.Reset();
  }

  databasePushNew(): PushRequestNewObject {
    return this.databaseState.PushNew();
  }

  databasePushExisting(): PushRequestObject {
    return this.databaseState.PushExisting();
  }

  databasePushResponse(databaseObject: DatabaseObject): void {
    this.id = databaseObject.Identity;
    this.databaseState.PushResponse(databaseObject);
  }

  workspacePush(): void {
    this.workspaceState.Push();
  }

  isAssociationForRole(roleType: RoleType, role: Strategy): boolean {
    // return                 roleType.Origin switch
    // {
    //     Origin.Session => this.Session.SessionState.IsAssociationForRole(this, roleType, role),
    //     Origin.Workspace => this.workspaceState?.IsAssociationForRole(roleType, role) ?? false,
    //     Origin.Database => this.databaseState?.IsAssociationForRole(roleType, role) ?? false,
    //     _ => throw new ArgumentException("Unsupported Origin")
    // };
  }
}
