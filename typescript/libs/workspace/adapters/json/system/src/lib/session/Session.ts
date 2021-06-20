import { IChangeSet, IInvokeResult, InvokeOptions, IPullResult, IPushResult, ISession, ISessionServices, Method, Pull } from '@allors/workspace/domain/system';
import { Class, Composite, Origin } from '@allors/workspace/meta/system';
import { Workspace } from '../workspace/Workspace';
import { SessionOriginState } from './originstate/SessionOriginState';
import { Strategy } from './Strategy';
import { ChangeSetTracker } from './trackers/ChangSetTracker';
import { PushToDatabaseTracker } from './trackers/PushToDatabaseTracker';
import { PushToWorkspaceTracker } from './trackers/PushToWorkspaceTracker';
import { ChangeSet } from './ChangeSet';
import { InvokeRequest, Invocation, PullRequest } from '@allors/protocol/json/system';
import { Observable } from 'rxjs';
import { InvokeResult } from '../database/invoke/InvokeResult';
import { map } from 'rxjs/operators';


export class Session implements ISession {

  ChangeSetTracker: ChangeSetTracker;

  PushToDatabaseTracker: PushToDatabaseTracker;

  PushToWorkspaceTracker: PushToWorkspaceTracker;

  SessionOriginState: SessionOriginState;

  StrategyByWorkspaceId: Map<number, Strategy>;

  private strategiesByClass: Map<Class, Set<Strategy>>;

  constructor (public Workspace: Workspace, public Services: ISessionServices) {
      this.StrategyByWorkspaceId = new Map();
      this.strategiesByClass = new Map();
      this.SessionOriginState = new SessionOriginState();
      this.ChangeSetTracker = new ChangeSetTracker();
      this.PushToDatabaseTracker = new PushToDatabaseTracker();
      this.PushToWorkspaceTracker = new PushToWorkspaceTracker();
      this.Services.onInit(this);
  }

  public Create(cls: Class): any {
    var workspaceId = this.Workspace.database.nextId();
    var strategy = new Strategy(this, cls, workspaceId);
    this.AddStrategy(strategy);

    if (cls.origin !== Origin.Session)
    {
        this.PushToWorkspaceTracker.OnCreated(strategy);
        if (cls.origin === Origin.Database)
        {
            this.PushToDatabaseTracker.OnCreated(strategy);
        }
    }

    this.ChangeSetTracker.OnCreated(strategy);

    return strategy.Object;
  }

  public *GetAll(objectType: Composite) {
      for (let cls of objectType.classes) {
          switch (cls.origin) {
              case Origin.Workspace:
                  if (this.Workspace.workspaceIdsByWorkspaceClass.has(cls)) {
                    const ids = this.Workspace.workspaceIdsByWorkspaceClass.get(cls);
                      for (let id of ids) {
                          if (this.StrategyByWorkspaceId.has(id)) {
                            const strategy = this.StrategyByWorkspaceId.get(id);
                              yield strategy.Object;
                          }                          else {
                              const strategy = this.InstantiateWorkspaceStrategy(id);
                              yield strategy.Object;
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
          for (let databaseOriginState of this.ChangeSetTracker.DatabaseOriginStates) {
              databaseOriginState.Checkpoint(changeSet);
          }

      }

      if ((this.ChangeSetTracker.WorkspaceOriginStates != null)) {
          for (let workspaceOriginState of this.ChangeSetTracker.WorkspaceOriginStates) {
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


  public Invoke(methods: Method[], options: InvokeOptions): Observable<IInvokeResult>
  {
    var invokeRequest: InvokeRequest =
    {
        l = methods.map(v => {
          return {
            i = v.object.id,
            v = (v.object.strategy as Strategy).DatabaseOriginState.Version,
            m = v.MethodType.tag
           }
        }),
        o = options != null
            ?  {
                c = options.continueOnError,
                i = options.isolated
            }
            : null
    };

    return this.Workspace.database.invoke(invokeRequest)
    .pipe(
      map((v) => new InvokeResult(this, v))
    );
  }

  public Pull(...pulls: Pull[]): Observable<IPullResult>{
    const pullRequest: PullRequest = { l = pulls.map(v => v.ToJson()) };

    var pullResponse = await this.Workspace.DatabaseConnection.Pull(pullRequest);
    return await this.OnPull(pullResponse);
  }

  public Proc(procedure: Procedure, params pulls: Pull[]): Task<IPullResult>;

  public Push(): Task<IPushResult>;

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
      for (let association of this.Get(associationType.ObjectType)) {
          if (!association.CanRead(roleType)) {
              // TODO: Warning!!! continue If
          }

          if (association.IsAssociationForRole(roleType, role)) {
              yield;
              return (<T>(association.Object));
          }

      }

  }

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
          for (let state of this.PushToWorkspaceTracker.Changed) {
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

