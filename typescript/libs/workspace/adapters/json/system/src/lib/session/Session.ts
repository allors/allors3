import { PullResponse, PushRequest, PushResponse, SyncRequest } from '@allors/protocol/json/system';
import { IChangeSet, IInvokeResult, InvokeOptions, IObject, IPullResult, IPushResult, ISession, ISessionServices, IStrategy, Method, Procedure, Pull } from '@allors/workspace/domain/system';
import { AssociationType, Class, Composite, Origin, RoleType } from '@allors/workspace/meta/system';
import { Database } from '../Database/Database';
import { DatabaseState } from '../Database/DatabaseState';
import { Strategy } from '../Strategy';
import { Workspace } from '../Workspace/Workspace';
import { WorkspaceState } from '../Workspace/WorkspaceState';
import { SessionState } from './SessionState';
import { ChangeSet } from '../ChangeSet';
import { EMPTY, Observable } from 'rxjs';

export /* abstract */ class Session extends ISession {

  private strategiesByClass: Dictionary<IClass, ISet<Strategy>>;

  protected constructor (workspace: Workspace, sessionServices: ISessionServices) {
      this.Workspace = workspace;
      this.Services = sessionServices;
      this.StrategyByWorkspaceId = new Dictionary<number, Strategy>();
      this.strategiesByClass = new Dictionary<IClass, ISet<Strategy>>();
      this.SessionOriginState = new SessionOriginState(this.Workspace.Numbers);
      this.ChangeSetTracker = new ChangeSetTracker();
      this.PushToDatabaseTracker = new PushToDatabaseTracker();
      this.PushToWorkspaceTracker = new PushToWorkspaceTracker();
      this.Services.OnInit(this);
  }

  public get Services(): ISessionServices {
  }

  ISession.Workspace: IWorkspace;

  public get Workspace(): Workspace {
  }

  public get ChangeSetTracker(): ChangeSetTracker {
  }

  public get PushToDatabaseTracker(): PushToDatabaseTracker {
  }

  public get PushToWorkspaceTracker(): PushToWorkspaceTracker {
  }

  public get SessionOriginState(): SessionOriginState {
  }

  protected get StrategyByWorkspaceId(): Dictionary<number, Strategy> {
  }

  public Create(): T {
      let .: number;
      TryParse(v, /* out */var, id);
      return id;
  }

  public GetAll(): IEnumerable<T> {
      let objectType = (<IComposite>(this.Workspace.DatabaseConnection.Configuration.ObjectFactory.GetObjectType()));
      return this.GetAll(objectType);
  }

  public GetAll(objectType: IComposite): IEnumerable<T> {
      for (let class in objectType.Classes) {
          switch (class.Origin) {
              case Origin.Workspace:
                  if (this.Workspace.WorkspaceIdsByWorkspaceClass.TryGetValue(class, /* out */var, ids)) {
                      for (let id in ids) {
                          if (this.StrategyByWorkspaceId.TryGetValue(id, /* out */var, strategy)) {
                              yield;
                              return (<T>(strategy.Object));
                          }
                          else {
                              strategy = this.InstantiateWorkspaceStrategy(id);
                              yield;
                              return (<T>(strategy.Object));
                          }

                      }

                  }

                  break;
          }

      }

  }

  public Checkpoint(): IChangeSet {
      let changeSet = new ChangeSet(this, this.ChangeSetTracker.Created, this.ChangeSetTracker.Instantiated);
      if ((this.ChangeSetTracker.DatabaseOriginStates != null)) {
          for (let databaseOriginState in this.ChangeSetTracker.DatabaseOriginStates) {
              databaseOriginState.Checkpoint(changeSet);
          }

      }

      if ((this.ChangeSetTracker.WorkspaceOriginStates != null)) {
          for (let workspaceOriginState in this.ChangeSetTracker.WorkspaceOriginStates) {
              workspaceOriginState.Checkpoint(changeSet);
          }

      }

      this.SessionOriginState.Checkpoint(changeSet);
      this.ChangeSetTracker.Created = null;
      this.ChangeSetTracker.Instantiated = null;
      this.ChangeSetTracker.DatabaseOriginStates = null;
      this.ChangeSetTracker.WorkspaceOriginStates = null;
      return changeSet;
  }

  public abstract Invoke(method: Method, options: InvokeOptions = null): Task<IInvokeResult>;

  public abstract Invoke(methods: Method[], options: InvokeOptions = null): Task<IInvokeResult>;

  public abstract Pull(params pulls: Pull[]): Task<IPullResult>;

  public abstract Pull(procedure: Procedure, params pulls: Pull[]): Task<IPullResult>;

  public abstract Push(): Task<IPushResult>;

  public GetStrategy(id: number): Strategy {
      if ((id == 0)) {
          return;
      }

      if (this.StrategyByWorkspaceId.TryGetValue(id, /* out */var, sessionStrategy)) {
          return sessionStrategy;
      }

      return this.InstantiateWorkspaceStrategy(id);
      // TODO: Warning!!!, inline IF is not supported ?
      Session.IsNewId(id);
      null;
  }

  public GetRole(association: Strategy, roleType: RoleType): Object {
      let role = this.SessionOriginState.Get(association.Id, roleType);
      if (roleType.ObjectType.IsUnit) {
          return role;
      }

      if (roleType.IsOne) {
          return this.Get((<long?>(role)));
      }

      return this.Workspace.Numbers.Enumerate(role).Select(this.Get).ToArray();
      // TODO: Warning!!!, inline IF is not supported ?
      (role != null);
      this.Workspace.DatabaseConnection.EmptyArray(roleType.ObjectType);
  }

  public GetAssociation(role: number, associationType: IAssociationType): IEnumerable<T> {
      let roleType = associationType.RoleType;
      for (let association in this.Get(associationType.ObjectType)) {
          if (!association.CanRead(roleType)) {
              // TODO: Warning!!! continue If
          }

          if (association.IsAssociationForRole(roleType, role)) {
              yield;
              return (<T>(association.Object));
          }

      }

  }

  public abstract Create(class: IClass): T;

  protected RemoveStrategy(strategy: Strategy) {
      this.StrategyByWorkspaceId.Remove(strategy.Id);
      let class = strategy.Class;
      if (!this.strategiesByClass.TryGetValue(class, /* out */var, strategies)) {
          return;
      }

      strategies.Remove(strategy);
  }

  protected PushToWorkspace(result: IPushResult): IPushResult {
      if ((this.PushToWorkspaceTracker.Created != null)) {
          for (let strategy in this.PushToWorkspaceTracker.Created) {
              strategy.WorkspaceOriginState.Push();
          }

      }

      if ((this.PushToWorkspaceTracker.Changed != null)) {
          for (let state in this.PushToWorkspaceTracker.Changed) {
              if ((this.PushToWorkspaceTracker.Created?.Contains(state.Strategy) == true)) {
                  // TODO: Warning!!! continue If
              }

              state.Push();
          }

      }

      this.PushToWorkspaceTracker.Created = null;
      this.PushToWorkspaceTracker.Changed = null;
      return result;
  }

  protected OnDatabasePushResponseNew(workspaceId: number, databaseId: number) {
      let strategy = this.StrategyByWorkspaceId[workspaceId];
      this.PushToDatabaseTracker.Created.Remove(strategy);
      this.RemoveStrategy(strategy);
      strategy.OnDatabasePushNewId(databaseId);
      this.AddStrategy(strategy);
      this.OnDatabasePushResponse(strategy);
  }

  protected OnDatabasePushResponse(strategy: Adapters.Strategy) {
      let databaseRecord = this.Workspace.DatabaseConnection.OnPushResponse(strategy.Class, strategy.Id);
      strategy.OnDatabasePushResponse(databaseRecord);
  }

  private /* internal */ static IsNewId(id: number): boolean {
  }

  classes.Contains.((Unknown: v.Value.Class): id<IEnumerable<Strategy>, Get, IComposite, objectType, var, classes, HashSet<IClass>, objectType.Classes, StrategyByWorkspaceId.Where, v> {
  }

