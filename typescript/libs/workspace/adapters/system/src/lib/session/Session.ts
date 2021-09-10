import { IChangeSet, IObject, ISession, ISessionServices, IWorkspaceResult } from '@allors/workspace/domain/system';
import { Workspace } from '../workspace/Workspace';
import { SessionOriginState } from './originstate/SessionOriginState';
import { Strategy } from './Strategy';
import { ChangeSetTracker } from './trackers/ChangSetTracker';
import { PushToDatabaseTracker } from './trackers/PushToDatabaseTracker';
import { PushToWorkspaceTracker } from './trackers/PushToWorkspaceTracker';
import { ChangeSet } from './ChangeSet';
import { AssociationType, Class, Composite, Origin } from '@allors/workspace/meta/system';
import { WorkspaceResult } from '../workspace/WorkspaceResult';
import { ObjectBase } from '../ObjectBase';
import { Ranges } from '../collections/ranges/Ranges';
import { DefaultStrategyRanges } from '../collections/ranges/DefaultStrategyRanges';
import { IStrategy } from '../../../../../domain/system/src/lib/IStrategy';

export function isNewId(id: number): boolean {
  return id < 0;
}

export abstract class Session implements ISession {
  changeSetTracker: ChangeSetTracker;

  pushToDatabaseTracker: PushToDatabaseTracker;

  pushToWorkspaceTracker: PushToWorkspaceTracker;

  sessionOriginState: SessionOriginState;

  strategyByWorkspaceId: Map<number, Strategy>;

  readonly ranges: Ranges<Strategy>;

  private strategiesByClass: Map<Class, Set<Strategy>>;

  constructor(public workspace: Workspace, public services: ISessionServices) {
    this.ranges = new DefaultStrategyRanges();

    this.strategyByWorkspaceId = new Map();
    this.strategiesByClass = new Map();
    this.sessionOriginState = new SessionOriginState(this.ranges);

    this.changeSetTracker = new ChangeSetTracker();
    this.pushToDatabaseTracker = new PushToDatabaseTracker();
    this.pushToWorkspaceTracker = new PushToWorkspaceTracker();
  }

  get hasChangedRoles(): boolean {
    // TODO: Optimize
    for (const [, strategy] of this.strategyByWorkspaceId) {
      if (strategy.hasChangedRoles) {
        return true;
      }
    }

    return false;
  }

  reset(): void {
    const changeSet = this.checkpoint();

    const strategies: Set<IStrategy> = new Set(changeSet.created);

    for (const roles of changeSet.rolesByAssociationType.values()) {
      for (const role of roles) {
        strategies.add(role);
      }
    }

    for (const associations of changeSet.associationsByRoleType.values()) {
      for (const association of associations) {
        strategies.add(association);
      }
    }

    for (const strategy of strategies) {
      strategy.reset();
    }
  }

  abstract create<T extends IObject>(cls: Composite): T;

  pullFromWorkspace(): IWorkspaceResult {
    const result = new WorkspaceResult();

    for (const [, strategy] of this.strategyByWorkspaceId) {
      if (strategy.cls.origin != Origin.Session) {
        strategy.WorkspaceOriginState.onPulled(result);
      }
    }

    return result;
  }

  pushToWorkspace(): IWorkspaceResult {
    const pushResult = new WorkspaceResult();

    if (this.pushToWorkspaceTracker.created != null) {
      for (const strategy of this.pushToWorkspaceTracker.created) {
        strategy.WorkspaceOriginState.push();
      }
    }

    if (this.pushToWorkspaceTracker.changed != null) {
      for (const state of this.pushToWorkspaceTracker.changed) {
        if (this.pushToWorkspaceTracker.created?.has(state.strategy) == true) {
          continue;
        }

        state.push();
      }
    }

    this.pushToWorkspaceTracker.created = null;
    this.pushToWorkspaceTracker.changed = null;

    return pushResult;
  }

  checkpoint(): IChangeSet {
    const changeSet = new ChangeSet(this, this.changeSetTracker.created, this.changeSetTracker.instantiated);
    if (this.changeSetTracker.databaseOriginStates != null) {
      for (const databaseOriginState of this.changeSetTracker.databaseOriginStates) {
        databaseOriginState.checkpoint(changeSet);
      }
    }

    if (this.changeSetTracker.workspaceOriginStates != null) {
      for (const workspaceOriginState of this.changeSetTracker.workspaceOriginStates) {
        workspaceOriginState.checkpoint(changeSet);
      }
    }

    this.sessionOriginState.checkpoint(changeSet);

    this.changeSetTracker.created = null;
    this.changeSetTracker.instantiated = null;
    this.changeSetTracker.databaseOriginStates = null;
    this.changeSetTracker.workspaceOriginStates = null;

    return changeSet;
  }

  instantiate<T extends IObject>(id: number): T;
  instantiate<T extends IObject>(ids: number[]): T[];
  instantiate<T extends IObject>(objectType: Composite): T[];
  instantiate<T extends IObject>(obj: T): T[];
  instantiate<T extends IObject>(args: unknown): unknown {
    if (typeof args === 'number') {
      return (this.getStrategy(args)?.object as unknown as T) ?? null;
    }

    if (args instanceof ObjectBase) {
      return (this.getStrategy(args.id)?.object as unknown as T) ?? null;
    }

    if (Array.isArray(args)) {
      return args.map((v) => this.getStrategy(v)?.object).filter((v) => v != null) as unknown as T[];
    }

    if (args['classes']) {
      const all: T[] = [];

      for (const cls of (args as Composite).classes) {
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
          case Origin.Database:
          case Origin.Session:
            if (this.strategiesByClass.has(cls)) {
              const strategies = this.strategiesByClass.get(cls);
              strategies.forEach((v) => {
                all.push(v.object as T);
              });
            }

            break;

          default:
            throw new Error(`Unknown Origin {@class.Origin}`);
        }
      }

      return all;
    }

    return null;
  }

  public getStrategy(id: number): Strategy {
    if (id == 0) {
      return null;
    }

    if (this.strategyByWorkspaceId.has(id)) {
      return this.strategyByWorkspaceId.get(id);
    }

    return isNewId(id) ? this.instantiateWorkspaceStrategy(id) : null;
  }

  public getCompositeAssociation(role: Strategy, associationType: AssociationType): Strategy {
    const roleType = associationType.roleType;
    for (const association of this.strategiesForClass(associationType.objectType as Composite)) {
      if (!association.canRead(roleType)) {
        continue;
      }

      if (association.isCompositeAssociationForRole(roleType, role)) {
        return association;
      }
    }

    return null;
  }

  public getCompositesAssociation(role: Strategy, associationType: AssociationType): Strategy[] {
    const roleType = associationType.roleType;

    const associations: Strategy[] = [];

    for (const association of this.strategiesForClass(associationType.objectType as Composite)) {
      if (!association.canRead(roleType)) {
        continue;
      }

      if (association.isCompositesAssociationForRole(roleType, role)) {
        associations.push(association);
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

  onDatabasePushResponseNew(workspaceId: number, databaseId: number) {
    const strategy = this.strategyByWorkspaceId.get(workspaceId);
    this.pushToDatabaseTracker.created.delete(strategy);
    strategy.onDatabasePushNewId(databaseId);
    this.addStrategy(strategy);
    strategy.onDatabasePushed();
  }

  private strategiesForClass(objectType: Composite): Strategy[] {
    const classes = objectType.classes;

    // TODO: Optimize
    const strategies: Strategy[] = [];
    for (const [, strategy] of this.strategyByWorkspaceId) {
      if (classes.has(strategy.cls) && strategies.indexOf(strategy) < 0) {
        strategies.push(strategy);
      }
    }

    return strategies;
  }

  abstract instantiateWorkspaceStrategy(id: number): Strategy;
}
