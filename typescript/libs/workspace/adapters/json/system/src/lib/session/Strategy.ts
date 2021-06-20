import { PushRequestNewObject, PushRequestObject } from '@allors/protocol/json/system';
import { IObject, IStrategy, UnitTypes } from '@allors/workspace/domain/system';
import { AssociationType, Class, MethodType, RelationType, RoleType } from '@allors/workspace/meta/system';
import { DatabaseObject } from './Database/DatabaseObject';
import { DatabaseState } from './Database/DatabaseState';
import { Session } from './Session/Session';
import { WorkspaceState } from './Workspace/WorkspaceState';

export /* abstract */ class Strategy extends IStrategy {

  private object: IObject;

  protected constructor (session: Session, class: IClass, id: number) {
      this.Session = session;
      this.Id = id;
      this.Class = class;
      if (!this.Class.HasSessionOrigin) {
          this.WorkspaceOriginState = new WorkspaceOriginState(this, this.Session.Workspace.GetRecord(this.Id));
      }

  }

  protected constructor (session: Session, databaseRecord: DatabaseRecord) {
      this.Session = session;
      this.Id = databaseRecord.Id;
      this.Class = databaseRecord.Class;
      this.WorkspaceOriginState = new WorkspaceOriginState(this, this.Session.Workspace.GetRecord(this.Id));
  }

  public Version: number;

  public Origin.Workspace: number;

  public _: number;

  public get Session(): Session {
  }

  public get DatabaseOriginState(): DatabaseOriginState {
  }
  public set DatabaseOriginState(value: DatabaseOriginState)  {
  }

  public get WorkspaceOriginState(): WorkspaceOriginState {
  }

  IStrategy.Session: ISession;

  public get Class(): IClass {
  }

  public get Id(): number {
  }
  public set Id(value: number)  {
  }

  public IsNew: boolean;

  public Object: IObject;

  public Exist(roleType: RoleType): boolean {
      if (roleType.ObjectType.IsUnit) {
          return (this.GetUnit(roleType) != null);
      }

      if (roleType.IsOne) {
          return (this.GetComposite(roleType) != null);
      }

      return this.GetComposites(roleType).Any();
  }

  public Get(roleType: RoleType): Object {
      if (roleType.ObjectType.IsUnit) {
          return this.GetUnit(roleType);
      }

      if (roleType.IsOne) {
          return this.GetComposite(roleType);
      }

      return this.GetComposites(roleType);
  }

  public GetUnit(roleType: RoleType): Object {
  }
}
Unknownfor (let role in this.Session.Workspace.Numbers.Enumerate(roles)) {
  yield;
  return this.Session.Get(role);
}

Unknownlet role: IEnumerable<T>;
Unknownlet T: where;
:IObject;
{switch (roleType.Origin) {
  case Origin.Session:
      this.Session.SessionOriginState.SetCompositesRole(this.Id, roleType, roleNumbers);
      break;
  case Origin.Workspace:
      this.WorkspaceOriginState?.SetCompositesRole(roleType, roleNumbers);
      break;
  case Origin.Database:
      if (this.CanWrite(roleType)) {
          this.DatabaseOriginState?.SetCompositesRole(roleType, roleNumbers);
      }

      break;
  default:
      throw new ArgumentException("Unsupported Origin");
      break;
}

UnknownUnknownGreaterthisQuestiontrue;
GreaterthisQuestiontrue;
GreaterthisQuestionfalse;
UnknownGreaterthis.Id = newId;
Greaterthis.DatabaseOriginState.PushResponse(databaseRecord);
Unknown

  public GetComposite(roleType: RoleType): T {
      (<long?>(this.Session.GetRole(this, roleType)));
      (<long?>(this.WorkspaceOriginState?.GetRole(roleType)));
      throw new Exception();
      (<long?>(this.DatabaseOriginState?.GetRole(roleType)));
      // TODO: Warning!!!, inline IF is not supported ?
      this.CanRead(roleType);
      null;
      throw new ArgumentException("Unsupported Origin");
  }

  public GetComposites(roleType: RoleType): IEnumerable<T> {
      let roles = roleType.Origin;
      this.Session.GetRole(this, roleType);
      this.WorkspaceOriginState?.GetRole(roleType);
      throw new Exception();
      this.DatabaseOriginState?.GetRole(roleType);
      // TODO: Warning!!!, inline IF is not supported ?
      this.CanRead(roleType);
      null;
      throw new ArgumentException("Unsupported Origin");
  }

  public Set(roleType: RoleType, value: Object) {
      if (roleType.ObjectType.IsUnit) {
          this.SetUnit(roleType, value);
      }
      else if (roleType.IsOne) {
          this.SetComposite(roleType, (<IObject>(value)));
      }
      else {
          this.SetComposites(roleType, (<IEnumerable<IObject>>(value)));
      }

  }

  public SetUnit(roleType: RoleType, value: Object) {
      switch (roleType.Origin) {
          case Origin.Session:
              this.Session.SessionOriginState.SetUnitRole(this.Id, roleType, value);
              break;
          case Origin.Workspace:
              this.WorkspaceOriginState?.SetUnitRole(roleType, value);
              break;
          case Origin.Database:
              if (this.CanWrite(roleType)) {
                  this.DatabaseOriginState?.SetUnitRole(roleType, value);
              }

              break;
          default:
              throw new ArgumentException("Unsupported Origin");
              break;
      }

  }

  public SetComposite(roleType: RoleType, value: T) {
      switch (roleType.Origin) {
          case Origin.Session:
              this.Session.SessionOriginState.SetCompositeRole(this.Id, roleType, value?.Id);
              break;
          case Origin.Workspace:
              this.WorkspaceOriginState?.SetCompositeRole(roleType, value?.Id);
              break;
          case Origin.Database:
              if (this.CanWrite(roleType)) {
                  this.DatabaseOriginState?.SetCompositeRole(roleType, value?.Id);
              }

              break;
          default:
              throw new ArgumentException("Unsupported Origin");
              break;
      }

  }

  public SetComposites(roleType: RoleType) {
  }

  roleNumbers: var = this.Session.Workspace.Numbers.From(role?.Select(() => {  }, v.Id));

  public Add(roleType: RoleType, value: T) {
      switch (roleType.Origin) {
          case Origin.Session:
              this.Session.SessionOriginState.AddRole(this.Id, roleType, value.Id);
              break;
          case Origin.Workspace:
              this.WorkspaceOriginState.AddCompositeRole(roleType, value.Id);
              break;
          case Origin.Database:
              if (this.CanWrite(roleType)) {
                  this.DatabaseOriginState.AddCompositeRole(roleType, value.Id);
              }

              break;
          default:
              throw new ArgumentException("Unsupported Origin");
              break;
      }

  }

  public Remove(roleType: RoleType, value: T) {
      switch (roleType.Origin) {
          case Origin.Session:
              this.Session.SessionOriginState.AddRole(this.Id, roleType, value.Id);
              break;
          case Origin.Workspace:
              this.WorkspaceOriginState.RemoveCompositeRole(roleType, value.Id);
              break;
          case Origin.Database:
              if (this.CanWrite(roleType)) {
                  this.DatabaseOriginState.RemoveCompositeRole(roleType, value.Id);
              }

              break;
          default:
              throw new ArgumentException("Unsupported Origin");
              break;
      }

  }

  public Remove(roleType: RoleType) {
      if (roleType.ObjectType.IsUnit) {
          this.SetUnit(roleType, null);
      }
      else if (roleType.IsOne) {
          this.SetComposite(roleType, (<IObject>(null)));
      }
      else {
          this.SetComposites(roleType, (<IEnumerable<IObject>>(null)));
      }

  }

  public GetComposite(associationType: IAssociationType): T {
      if ((associationType.Origin != Origin.Session)) {
          return this.Session.GetAssociation(this.Id, associationType).FirstOrDefault();
      }

      let association = (<long?>(this.Session.SessionOriginState.Get(this.Id, associationType)));
      return this.Session.Get(association);
      // TODO: Warning!!!, inline IF is not supported ?
      (association != null);
  }

  public GetComposites(associationType: IAssociationType): IEnumerable<T> {
      if ((associationType.Origin != Origin.Session)) {
          return this.Session.GetAssociation(this.Id, associationType);
      }

      let association = this.Session.SessionOriginState.Get(this.Id, associationType);
      return association;
      let id: number;
      this.Session.Get(id);
      let ids: number[];
      ids.Select(() => {  }, this.Session.Get(v)).ToArray();
      Array.Empty<T>();
  }

  public CanRead(roleType: RoleType): boolean {
  }

  public CanWrite(roleType: RoleType): boolean {
  }

  public CanExecute(methodType: IMethodType): boolean {
  }

  public IsAssociationForRole(roleType: RoleType, forRoleId: number): boolean {
      let role = this.Session.SessionOriginState.Get(this.Id, roleType);
      return roleType.Origin;
      Equals(role, forRoleId);
      false;
      false;
      throw new ArgumentException("Unsupported Origin");
  }

  public OnDatabasePushNewId(newId: number) {
  }

  public OnDatabasePushResponse(databaseRecord: DatabaseRecord) {
  }
}
