import { IChangeSet, IInvokeResult, InvokeOptions, IPullResult, IPushResult, ISession, ISessionServices, Method, Procedure, Pull } from '@allors/workspace/domain/system';
import { AssociationType, Class, Composite, Origin, RoleType } from '@allors/workspace/meta/system';
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
import { PushRequest } from '../../../../../../../protocol/json/system/src/lib/api/push/PushRequest';
import { PushResult } from '../database/push/PushResult';
import { enumerate } from '../collections/Numbers';

export function isNewId(id: number): boolean {
  return id < 0;
}

export class Session implements ISession {
  changeSetTracker: ChangeSetTracker;

  pushToDatabaseTracker: PushToDatabaseTracker;

  pushToWorkspaceTracker: PushToWorkspaceTracker;

  sessionOriginState: SessionOriginState;

  strategyByWorkspaceId: Map<number, Strategy>;

  private strategiesByClass: Map<Class, Set<Strategy>>;

  constructor(public workspace: Workspace, public services: ISessionServices) {
    this.strategyByWorkspaceId = new Map();
    this.strategiesByClass = new Map();
    this.sessionOriginState = new SessionOriginState();
    this.changeSetTracker = new ChangeSetTracker();
    this.pushToDatabaseTracker = new PushToDatabaseTracker();
    this.pushToWorkspaceTracker = new PushToWorkspaceTracker();
    this.services.onInit(this);
  }

  create(cls: Class): any {
    const workspaceId = this.workspace.database.nextId();
    const strategy = new Strategy(this, cls, workspaceId);
    this.AddStrategy(strategy);

    if (cls.origin !== Origin.Session) {
      this.pushToWorkspaceTracker.OnCreated(strategy);
      if (cls.origin === Origin.Database) {
        this.pushToDatabaseTracker.OnCreated(strategy);
      }
    }

    this.changeSetTracker.OnCreated(strategy);

    return strategy.Object;
  }

  getOne<T>(id: number): T {
    return (this.GetStrategy(id)?.object as unknown) as T;
  }

  *getMany<T>(...ids: number[]) {
    for (const id of ids) {
      const strategy = this.GetStrategy(id);
      if (strategy !== null) {
        yield (strategy.object as unknown) as T;
      }
    }
  }

  *getAll<T>(objectType: Composite) {
    for (const cls of objectType.classes) {
      switch (cls.origin) {
        case Origin.Workspace:
          if (this.workspace.workspaceIdsByWorkspaceClass.has(cls)) {
            const ids = this.workspace.workspaceIdsByWorkspaceClass.get(cls);
            for (const id of ids) {
              if (this.strategyByWorkspaceId.has(id)) {
                const strategy = this.strategyByWorkspaceId.get(id);
                yield (strategy.object as unknown) as T;
              } else {
                const strategy = this.InstantiateWorkspaceStrategy(id);
                yield strategy.Object as T;
              }
            }
          }

          break;
      }
    }
  }

  Checkpoint(): IChangeSet {
    const changeSet = new ChangeSet(this, this.changeSetTracker.Created, this.changeSetTracker.Instantiated);
    if (this.changeSetTracker.DatabaseOriginStates != null) {
      for (const databaseOriginState of this.changeSetTracker.DatabaseOriginStates) {
        databaseOriginState.Checkpoint(changeSet);
      }
    }

    if (this.changeSetTracker.WorkspaceOriginStates != null) {
      for (const workspaceOriginState of this.changeSetTracker.WorkspaceOriginStates) {
        workspaceOriginState.Checkpoint(changeSet);
      }
    }

    this.sessionOriginState.Checkpoint(changeSet);
    this.changeSetTracker.Created = null;
    this.changeSetTracker.Instantiated = null;
    this.changeSetTracker.DatabaseOriginStates = null;
    this.changeSetTracker.WorkspaceOriginStates = null;

    return changeSet;
  }

  public Invoke(methods: Method[], options: InvokeOptions): Observable<IInvokeResult> {
    const invokeRequest: InvokeRequest = {
      l: methods.map((v) => {
        return {
          i: v.object.id,
          v: (v.object.strategy as Strategy).DatabaseOriginState.Version,
          m: v.MethodType.tag,
        };
      }),
      o:
        options != null
          ? {
              c: options.continueOnError,
              i: options.isolated,
            }
          : null,
    };

    return this.workspace.database.invoke(invokeRequest).pipe(map((v) => new InvokeResult(this, v)));
  }

  public Pull(...pulls: Pull[]): Observable<IPullResult> {
    const pullRequest: PullRequest = {
      l: pulls.map((v) => v.ToJson()),
    };

    return this.workspace.database.pull(pullRequest).pipe(map((v) => this.OnPull(v)));
  }

  public Call(procedure: Procedure, ...pulls: Pull[]): Observable<IPullResult> {
    const pullRequest: PullRequest = {
      p: procedure.ToJson(this.database.UnitConvert),
      l: pulls.map((v) => v.ToJson(this.database.UnitConvert)),
    };

    var pullResponse = await this.workspace.DatabaseConnection.Pull(pullRequest);
    return await this.OnPull(pullResponse);
  }

  public Push(): Observable<IPushResult> {
    const pushRequest: PushRequest = {
      n: [...this.pushToDatabaseTracker.Created].map((v) => v.DatabasePushNew()),
      o: [...this.pushToDatabaseTracker.Changed].map((v) => v.Strategy.DatabasePushExisting()),
    };

    return this.workspace.database.push(pushRequest).pipe(map((v) => new PushResult(this, v)));

    // TODO:
    //
    // if (pushResponse.HasErrors) {
    //     return new PushResult(this, pushResponse);
    // }

    // if ((pushResponse.n != null)) {
    //     for (let pushResponseNewObject in pushResponse.n) {
    //         let workspaceId = pushResponseNewObject.w;
    //         let databaseId = pushResponseNewObject.d;
    //         this.OnDatabasePushResponseNew(workspaceId, databaseId);
    //     }

    // }

    // this.PushToDatabaseTracker.Created = null;
    // if ((pushRequest.o != null)) {
    //     for (let id in pushRequest.o.Select(() => {  }, v.d)) {
    //         let strategy = this.GetStrategy(id);
    //         this.OnDatabasePushResponse(strategy);
    //     }

    // }

    // let result = new PushResult(this, pushResponse);
    // if (!result.HasErrors) {
    //     this.PushToWorkspace(result);
    // }
  }

  public GetStrategy(id: number): Strategy {
    if (id == 0) {
      return;
    }

    if (this.strategyByWorkspaceId.has(id)) {
      return this.strategyByWorkspaceId.get(id);
    }

    if (isNewId(id)) {
      this.InstantiateWorkspaceStrategy(id);
    }

    return null;
  }

  public GetRole(association: Strategy, roleType: RoleType): Object {
    const role = this.sessionOriginState.Get(association.Id, roleType);
    if (roleType.objectType.isUnit) {
      return role;
    }

    if (roleType.isOne) {
      return this.Get(role);
    }

    if (role !== null) {
      return enumerate(role).map((v) => this.Get(v));
    } else {
      return [];
    }
  }

  public *GetAssociation(role: number, associationType: AssociationType) {
    const roleType = associationType.roleType;
    for (const association of this.GetForAssociation(associationType.objectType as Composite)) {
      if (!association.canRead(roleType)) {
        // TODO: Warning!!! continue If
      }

      if (association.isAssociationForRole(roleType, role)) {
        yield association.Object;
      }
    }
  }

  protected AddStrategy(strategy: Strategy) {
    this.strategyByWorkspaceId.set(strategy.id, strategy);

    let strategies = this.strategiesByClass.get(strategy.cls);
    if (strategies == null) {
      strategies = new Set();
      this.strategiesByClass.set(strategy.cls, strategies);
    }

    strategies.add(strategy);
  }

  protected RemoveStrategy(strategy: Strategy) {
    this.strategyByWorkspaceId.delete(strategy.id);

    const strategies = this.strategiesByClass.get(strategy.cls);
    if (strategies != null) {
      strategies.delete(strategy);
    }
  }

  protected PushToWorkspace(result: IPushResult): IPushResult {
    if (this.pushToWorkspaceTracker.Created != null) {
      for (const strategy of this.pushToWorkspaceTracker.Created) {
        strategy.WorkspaceOriginState.Push();
      }
    }

    if (this.pushToWorkspaceTracker.Changed != null) {
      for (const state of this.pushToWorkspaceTracker.Changed) {
        if (this.pushToWorkspaceTracker.Created?.has(state.Strategy) == true) {
          continue;
        }

        state.Push();
      }
    }

    this.pushToWorkspaceTracker.Created = null;
    this.pushToWorkspaceTracker.Changed = null;
    return result;
  }

  protected OnDatabasePushResponseNew(workspaceId: number, databaseId: number) {
    const strategy = this.strategyByWorkspaceId[workspaceId];
    this.pushToDatabaseTracker.Created.Remove(strategy);
    this.RemoveStrategy(strategy);
    strategy.OnDatabasePushNewId(databaseId);
    this.AddStrategy(strategy);
    this.OnDatabasePushResponse(strategy);
  }

  protected OnDatabasePushResponse(strategy: Strategy) {
    const databaseRecord = this.workspace.database.onPushResponse(strategy.Class, strategy.Id);
    strategy.onDatabasePushResponse(databaseRecord);
  }

  private *GetForAssociation(objectType: Composite) {
    const classes = objectType.classes;

    for (const [_, strategy] of this.strategyByWorkspaceId) {
      if (classes.has(strategy.Class)) {
        yield strategy;
      }
    }
  }

  public InstantiateDatabaseStrategy(id: number): Strategy {
    const databaseRecord = this.workspace.database.getRecord(id);
    const strategy = new Strategy(this, databaseRecord);
    this.AddStrategy(strategy);
    this.ChangeSetTracker.OnInstantiated(strategy);
    return strategy;
  }

  protected InstantiateWorkspaceStrategy(id: number): Strategy {
    if (!this.workspace.workspaceClassByWorkspaceId.has(id)) {
      return null;
    }

    const cls = this.workspace.workspaceClassByWorkspaceId.get(id);
    const strategy = new Strategy(this, cls, id);
    this.AddStrategy(strategy);
    this.changeSetTracker.OnInstantiated(strategy);
    return strategy;
  }
}
