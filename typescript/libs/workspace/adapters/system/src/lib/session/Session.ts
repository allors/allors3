import { IChangeSet, InvokeOptions, IObject, IPullResult, IPushResult, IResult, ISession, ISessionServices, IStrategy, Method, Procedure, Pull } from '@allors/workspace/domain/system';
import { Workspace } from '../workspace/Workspace';
import { SessionOriginState } from './originstate/SessionOriginState';
import { Strategy } from './Strategy';
import { ChangeSetTracker } from './trackers/ChangSetTracker';
import { PushToDatabaseTracker } from './trackers/PushToDatabaseTracker';
import { PushToWorkspaceTracker } from './trackers/PushToWorkspaceTracker';
import { ChangeSet } from './ChangeSet';
import { enumerate } from '../collections/Numbers';
import { AssociationType, Class, Composite, Origin } from '@allors/workspace/meta/system';

export function isNewId(id: number): boolean {
  return id < 0;
}

export abstract class Session implements ISession {
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

  abstract invoke(method: Method | Method[], options?: InvokeOptions): Promise<IResult>;

  abstract call(procedure: Procedure, ...pulls: Pull[]): Promise<IPullResult>;

  abstract pull(pulls: Pull[]): Promise<IPullResult>;

  abstract push(): Promise<IResult>;

  create(cls: Class): any {
    const workspaceId = this.workspace.database.nextId();
    const strategy = new Strategy(this, cls, workspaceId);
    this.addStrategy(strategy);

    if (cls.origin !== Origin.Session) {
      this.pushToWorkspaceTracker.OnCreated(strategy);
      if (cls.origin === Origin.Database) {
        this.pushToDatabaseTracker.OnCreated(strategy);
      }
    }

    this.changeSetTracker.OnCreated(strategy);

    return strategy.object;
  }

  getOne<T>(id: number): T {
    return (this.getStrategy(id)?.object as unknown) as T;
  }

  getMany<T>(ids: number[]): T[] {
    return (ids.map((v) => this.getStrategy(v)).filter((v) => v != null) as unknown) as T[];
  }

  getAll<T extends IObject>(objectType: Composite): T[] {
    const all: T[] = [];

    for (const cls of objectType.classes) {
      switch (cls.origin) {
        case Origin.Workspace:
          if (this.workspace.workspaceIdsByWorkspaceClass.has(cls)) {
            const ids = this.workspace.workspaceIdsByWorkspaceClass.get(cls);
            for (const id of ids) {
              if (this.strategyByWorkspaceId.has(id)) {
                const strategy = this.strategyByWorkspaceId.get(id);
                all.push(strategy.object as T);
              } else {
                const strategy = this.instantiateWorkspaceStrategy(id);
                all.push(strategy.object as T);
              }
            }
          }

          break;
      }
    }

    return all;
  }

  checkpoint(): IChangeSet {
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



  public getStrategy(id: number): Strategy {
    if (id == 0) {
      return;
    }

    if (this.strategyByWorkspaceId.has(id)) {
      return this.strategyByWorkspaceId.get(id);
    }

    if (isNewId(id)) {
      this.instantiateWorkspaceStrategy(id);
    }

    return null;
  }

  public getRole(association: Strategy, roleType: RoleType): any {
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

  public getCompositeAssociation<T extends IObject>(role: number, associationType: AssociationType): T {
    const roleType = associationType.roleType;
    for (const association of this.getForAssociation(associationType.objectType as Composite)) {
      if (association.canRead(roleType)) {
        if (association.isAssociationForRole(roleType, role)) {
          return association.object as T;
        }
      }
    }
  }

  public getCompositesAssociation<T extends IObject>(role: number, associationType: AssociationType): T[] {
    const roleType = associationType.roleType;

    const associations: T[] = [];

    for (const association of this.getForAssociation(associationType.objectType as Composite)) {
      if (!association.canRead(roleType)) {
        // TODO: Warning!!! continue If
      }

      if (association.isAssociationForRole(roleType, role)) {
        associations.push(association.object as T);
      }
    }

    return associations;
  }

  protected addStrategy(strategy: Strategy) {
    this.strategyByWorkspaceId.set(strategy.id, strategy);

    let strategies = this.strategiesByClass.get(strategy.cls);
    if (strategies == null) {
      strategies = new Set();
      this.strategiesByClass.set(strategy.cls, strategies);
    }

    strategies.add(strategy);
  }

  protected removeStrategy(strategy: Strategy) {
    this.strategyByWorkspaceId.delete(strategy.id);

    const strategies = this.strategiesByClass.get(strategy.cls);
    if (strategies != null) {
      strategies.delete(strategy);
    }
  }

  protected pushToWorkspace(result: IPushResult): IPushResult {
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

  protected onDatabasePushResponseNew(workspaceId: number, databaseId: number) {
    const strategy = this.strategyByWorkspaceId[workspaceId];
    this.pushToDatabaseTracker.Created.delete(strategy);
    this.removeStrategy(strategy);
    strategy.OnDatabasePushNewId(databaseId);
    this.addStrategy(strategy);
    this.onDatabasePushResponse(strategy);
  }

  protected onDatabasePushResponse(strategy: Strategy) {
    const databaseRecord = this.workspace.database.onPushResponse(strategy.Class, strategy.Id);
    strategy.onDatabasePushResponse(databaseRecord);
  }

  private getForAssociation(objectType: Composite): IStrategy[] {
    const classes = objectType.classes;

    const strategies: IStrategy[] = [];

    for (const [_, strategy] of this.strategyByWorkspaceId) {
      if (classes.has(strategy.cls)) {
        strategies.push(strategy);
      }
    }

    return strategies;
  }

  public instantiateDatabaseStrategy(id: number): Strategy {
    const databaseRecord = this.workspace.database.getRecord(id);
    const strategy = Strategy.fromDatabaseRecord(this, databaseRecord);
    this.addStrategy(strategy);
    this.changeSetTracker.OnInstantiated(strategy);
    return strategy;
  }

  protected instantiateWorkspaceStrategy(id: number): Strategy {
    if (!this.workspace.workspaceClassByWorkspaceId.has(id)) {
      return null;
    }

    const cls = this.workspace.workspaceClassByWorkspaceId.get(id);
    const strategy = new Strategy(this, cls, id);
    this.addStrategy(strategy);
    this.changeSetTracker.OnInstantiated(strategy);
    return strategy;
  }
}
