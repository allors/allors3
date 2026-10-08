import {
  IDatabaseConnection,
  Invocation,
  PullResult as ConnectionPullResult,
} from '@allors/system/workspace/connection';
import {
  IChangeSet,
  IInvokeResult,
  InvokeOptions,
  IObject,
  IPullResult,
  IPushResult,
  IRule,
  ISession,
  Method,
  Procedure,
  Pull,
  ResultError,
} from '@allors/system/workspace/domain';
import {
  AssociationType,
  Class,
  Composite,
  Dependency,
  RoleType,
} from '@allors/system/workspace/meta';

import { Workspace } from '../workspace/workspace';
import { ObjectBase } from '../object-base';
import { DefaultObjectRanges } from '../collections/default-object-ranges';
import { Ranges } from '@allors/system/workspace/connection';
import { InvokeResult } from '../results/invoke-result';
import { PullResult } from '../results/pull-result';
import { PushResult } from '../results/push-result';

import { SessionOriginState } from './originstate/session-origin-state';
import { DatabaseOriginState } from './originstate/database-origin-state';
import { Strategy } from './strategy';
import { ChangeSetTracker } from './trackers/change-set-tracker';
import { PushToDatabaseTracker } from './trackers/push-to-database-tracker';
import { ChangeSet } from './change-set';

export function isNewId(id: number): boolean {
  return id < 0;
}

export class Session implements ISession {
  context: string;

  changeSetTracker: ChangeSetTracker;

  pushToDatabaseTracker: PushToDatabaseTracker;

  sessionOriginState: SessionOriginState;

  activeRulesByRoleType: Map<RoleType, Set<IRule<IObject>>>;

  readonly ranges: Ranges<IObject>;

  protected objectByWorkspaceId: Map<number, IObject>;

  private objectsByClass: Map<Class, Set<IObject>>;

  protected dependencies: Set<Dependency>;

  constructor(public workspace: Workspace) {
    this.ranges = new DefaultObjectRanges();

    this.objectByWorkspaceId = new Map();
    this.objectsByClass = new Map();
    this.sessionOriginState = new SessionOriginState(this.ranges);

    this.changeSetTracker = new ChangeSetTracker();
    this.pushToDatabaseTracker = new PushToDatabaseTracker();

    this.activeRulesByRoleType = new Map();
    this.dependencies = new Set();
  }

  private get connection(): IDatabaseConnection {
    return this.workspace.connection;
  }

  activate(rules: IRule<IObject>[]): void {
    if (rules == null) {
      return;
    }

    for (const rule of rules) {
      let activeRules = this.activeRulesByRoleType.get(rule.roleType);
      if (activeRules == null) {
        activeRules = new Set();
        this.activeRulesByRoleType.set(rule.roleType, activeRules);
      }

      activeRules.add(rule);

      if (rule.dependencies != null) {
        for (const dependency of rule.dependencies) {
          this.dependencies.add(dependency);
        }
      }
    }
  }

  resolve(strategy: Strategy, roleType: RoleType): IRule<IObject> {
    const activeRules = this.activeRulesByRoleType.get(roleType);

    if (activeRules?.size > 0) {
      const rule = this.workspace.rule(roleType, strategy);

      if (rule != null && activeRules.has(rule)) {
        return rule;
      }
    }

    return null;
  }

  get hasChanges(): boolean {
    // TODO: Optimize
    for (const [, object] of this.objectByWorkspaceId) {
      if (object.strategy.hasChanges) {
        return true;
      }
    }

    return false;
  }

  reset(): void {
    const changeSet = this.checkpoint();
    const objects: Set<IObject> = new Set(changeSet.created);

    for (const roles of changeSet.rolesByAssociationType.values()) {
      if (roles != null) {
        for (const role of roles) {
          objects.add(role);
        }
      }
    }

    for (const associations of changeSet.associationsByRoleType.values()) {
      if (associations != null) {
        for (const association of associations) {
          objects.add(association);
        }
      }
    }

    for (const object of objects) {
      object.strategy.reset();
    }
  }

  create<T extends IObject>(cls: Class): T {
    const workspaceId = this.workspace.idGenerator();
    const strategy = new Strategy(this, cls, workspaceId);
    this.addObject(strategy.object);
    this.pushToDatabaseTracker.onCreated(strategy.object);
    this.changeSetTracker.onCreated(strategy.object);
    return strategy.object as T;
  }

  checkpoint(): IChangeSet {
    const changeSet = new ChangeSet(this, this.changeSetTracker.created);
    if (this.changeSetTracker.databaseOriginStates != null) {
      for (const databaseOriginState of this.changeSetTracker
        .databaseOriginStates) {
        databaseOriginState.checkpoint(changeSet);
      }
    }

    this.sessionOriginState.checkpoint(changeSet);

    this.changeSetTracker.created = null;
    this.changeSetTracker.databaseOriginStates = null;

    return changeSet;
  }

  instantiate<T extends IObject>(id: number): T;
  instantiate<T extends IObject>(ids: number[]): T[];
  instantiate<T extends IObject>(objectType: Composite): T[];
  instantiate<T extends IObject>(obj: T): T[];
  instantiate<T extends IObject>(args: unknown): unknown {
    if (typeof args === 'number') {
      return (this.getObject(args) as unknown as T) ?? null;
    }

    if (args instanceof ObjectBase) {
      return (this.getObject(args.id) as unknown as T) ?? null;
    }

    if (Array.isArray(args)) {
      return args
        .map((v) => this.getObject(v))
        .filter((v) => v != null) as unknown as T[];
    }

    if (args && args['classes']) {
      const all: T[] = [];

      for (const cls of (args as Composite).classes) {
        if (this.objectsByClass.has(cls)) {
          const strategies = this.objectsByClass.get(cls);
          strategies.forEach((v) => {
            all.push(v as T);
          });
        }
      }

      return all;
    }

    return null;
  }

  onDelete(strategy: Strategy) {
    this.removeObject(strategy.object);
    this.pushToDatabaseTracker.onDelete(strategy.object);
    this.changeSetTracker.onDelete(strategy.object);
  }

  public getObject(id: number): IObject {
    if (id == 0) {
      return null;
    }

    if (this.objectByWorkspaceId.has(id)) {
      return this.objectByWorkspaceId.get(id);
    }

    return this.instantiateDatabaseStrategy(id);
  }

  public getCompositeAssociation(
    role: IObject,
    associationType: AssociationType
  ): IObject {
    const roleType = associationType.roleType;

    for (const cls of (associationType.objectType as Composite).classes) {
      const associations = this.objectsByClass.get(cls);
      if (associations != null) {
        for (const association of associations) {
          if (!association.strategy.canRead(roleType)) {
            continue;
          }

          if (
            (association.strategy as Strategy).isCompositeAssociationForRole(
              roleType,
              role
            )
          ) {
            return association;
          }
        }
      }
    }

    return null;
  }

  public getCompositesAssociation(
    role: IObject,
    associationType: AssociationType
  ): IObject[] {
    const roleType = associationType.roleType;

    const results: IObject[] = [];

    for (const cls of (associationType.objectType as Composite).classes) {
      const associations = this.objectsByClass.get(cls);
      if (associations != null) {
        for (const association of associations) {
          if (!association.strategy.canRead(roleType)) {
            continue;
          }

          if (
            (association.strategy as Strategy).isCompositesAssociationForRole(
              roleType,
              role
            )
          ) {
            results.push(association);
          }
        }
      }
    }

    return results;
  }

  async invoke(
    methodOrMethods: Method | Method[],
    options?: InvokeOptions
  ): Promise<IInvokeResult> {
    const methods = Array.isArray(methodOrMethods)
      ? methodOrMethods
      : [methodOrMethods];

    const invocations: Invocation[] = methods.map((v) => ({
      id: v.object.id,
      version: (v.object.strategy as Strategy).DatabaseOriginState.version,
      methodType: v.methodType,
    }));

    const invoked = await this.connection.invoke(invocations, {
      ...options,
      context: this.context,
    });
    const result = new InvokeResult(this, invoked);

    if (result.hasErrors) {
      throw new ResultError(result);
    }

    return result;
  }

  async call(procedure: Procedure, ...pulls: Pull[]): Promise<IPullResult> {
    const pulled = await this.connection.pull(pulls, {
      procedure,
      dependencies: this.dependencies,
      context: this.context,
    });

    return this.onPull(pulled);
  }

  async pull(pullOrPulls: Pull | Pull[]): Promise<IPullResult> {
    const pulls = Array.isArray(pullOrPulls) ? pullOrPulls : [pullOrPulls];

    const pulled = await this.connection.pull(pulls, {
      dependencies: this.dependencies,
      context: this.context,
    });

    return this.onPull(pulled);
  }

  async push(): Promise<IPushResult> {
    const databaseTracker = this.pushToDatabaseTracker;

    const newObjects = databaseTracker.created
      ? [...databaseTracker.created].map((v) =>
          (
            (v.strategy as Strategy).DatabaseOriginState as DatabaseOriginState
          ).pushNew()
        )
      : null;

    const changedObjects = databaseTracker.changed
      ? [...databaseTracker.changed].map((v) =>
          (v as DatabaseOriginState).pushExisting()
        )
      : null;

    const pushed = await this.connection.push(newObjects, changedObjects, {
      context: this.context,
    });

    const pushResult = new PushResult(this, pushed);

    if (pushResult.hasErrors) {
      throw new ResultError(pushResult);
    }

    for (const [workspaceId, databaseId] of pushed.databaseIdByWorkspaceId) {
      this.onDatabasePushResponseNew(workspaceId, databaseId);
    }

    databaseTracker.created = null;
    databaseTracker.changed = null;

    if (changedObjects != null) {
      for (const changedObject of changedObjects) {
        const object = this.getObject(changedObject.id);
        (object.strategy as Strategy).onDatabasePushed();
      }
    }

    return pushResult;
  }

  protected addObject(object: IObject) {
    this.objectByWorkspaceId.set(object.id, object);
    let objects = this.objectsByClass.get(object.strategy.cls);
    if (objects == null) {
      objects = new Set();
      this.objectsByClass.set(object.strategy.cls, objects);
    }

    objects.add(object);
  }

  protected removeObject(object: IObject) {
    this.objectByWorkspaceId.delete(object.id);
    this.objectsByClass.get(object.strategy.cls)?.delete(object);
  }

  private onDatabasePushResponseNew(workspaceId: number, databaseId: number) {
    const object = this.objectByWorkspaceId.get(workspaceId);
    this.pushToDatabaseTracker.created?.delete(object);
    (object.strategy as Strategy).onDatabasePushNewId(databaseId);
    this.addObject(object);
    (object.strategy as Strategy).onDatabasePushed();
  }

  private instantiateDatabaseStrategy(id: number): IObject {
    const record = this.connection.getRecord(id);

    if (record == null) {
      return null;
    }

    const strategy = Strategy.fromRecord(this, record);
    this.addObject(strategy.object);
    return strategy.object;
  }

  private onPull(pulled: ConnectionPullResult): IPullResult {
    const pullResult = new PullResult(this, pulled);

    if (pullResult.hasErrors) {
      return pullResult;
    }

    for (const id of pulled.pool) {
      if (this.objectByWorkspaceId.has(id)) {
        const object = this.objectByWorkspaceId.get(id);
        (object.strategy as Strategy).DatabaseOriginState.onPulled(pullResult);
      } else {
        this.instantiateDatabaseStrategy(id);
      }
    }

    return pullResult;
  }
}
