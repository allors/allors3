import { InvokeRequest, PullRequest, PullResponse, PushRequest } from '@allors/protocol/json/system';
import { Session as SystemSession } from '@allors/workspace/adapters/system';
import { IInvokeResult, InvokeOptions, IObject, IPullResult, IPushResult, Method, Procedure, Pull, ResultError } from '@allors/workspace/domain/system';
import { Class, Origin } from '@allors/workspace/meta/system';
import { DatabaseConnection } from '../database/database-connection';
import { DatabaseRecord } from '../database/database-record';
import { InvokeResult } from '../database/invoke/invoke-result';
import { PullResult } from '../database/pull/pull-result';
import { PushResult } from '../database/push/push-result';
import { dependenciesToJson, procedureToJson, pullToJson } from '../json/to-json';
import { Workspace } from '../workspace/workspace';
import { DatabaseOriginState } from './originstate/database-origin-state';
import { Strategy } from './strategy';

export class Session extends SystemSession {
  
  database: DatabaseConnection;

  constructor(workspace: Workspace) {
    super(workspace);

    this.database = this.workspace.database as DatabaseConnection;
  }

  create<T extends IObject>(cls: Class): T {
    const workspaceId = this.workspace.database.nextId();
    const strategy = new Strategy(this, cls, workspaceId);
    this.addObject(strategy.object);

    if (cls.origin != Origin.Session) {
      this.pushToWorkspaceTracker.onCreated(strategy.object);
      if (cls.origin == Origin.Database) {
        this.pushToDatabaseTracker.onCreated(strategy.object);
      }
    }

    this.changeSetTracker.onCreated(strategy.object);
    return strategy.object as T;
  }

  onDelete(strategy: Strategy) {
    this.removeObject(strategy.object);

    if (strategy.cls.origin != Origin.Session) {
      this.pushToWorkspaceTracker.onDelete(strategy.object);
      if (strategy.cls.origin == Origin.Database) {
        this.pushToDatabaseTracker.onDelete(strategy.object);
      }
    }

    this.changeSetTracker.onDelete(strategy.object);
  }

  instantiateDatabaseStrategy(id: number): void {
    const databaseRecord = this.workspace.database.getRecord(id) as DatabaseRecord;
    const strategy = Strategy.fromDatabaseRecord(this, databaseRecord);
    this.addObject(strategy.object);
    this.changeSetTracker.onInstantiated(strategy.object);
  }

  instantiateWorkspaceStrategy(id: number): IObject {
    if (!this.workspace.workspaceClassByWorkspaceId.has(id)) {
      return null;
    }

    const cls = this.workspace.workspaceClassByWorkspaceId.get(id);

    const strategy = new Strategy(this, cls, id);
    this.addObject(strategy.object);

    this.changeSetTracker.onInstantiated(strategy.object);

    return strategy.object;
  }

  async invoke(methodOrMethods: Method | Method[], options?: InvokeOptions): Promise<IInvokeResult> {
    const methods = Array.isArray(methodOrMethods) ? methodOrMethods : [methodOrMethods];
    const invokeRequest: InvokeRequest = {
      l: methods.map((v) => {
        return {
          i: v.object.id,
          v: (v.object.strategy as Strategy).DatabaseOriginState.version,
          m: v.methodType.tag,
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

    const invokeResponse = await this.database.client.invoke(invokeRequest);
    return new InvokeResult(this, invokeResponse);
  }

  async call(procedure: Procedure, ...pulls: Pull[]): Promise<IPullResult> {
    const pullRequest: PullRequest = {
      d: dependenciesToJson(this.dependencies),
      p: procedureToJson(procedure),
      l: pulls.map((v) => pullToJson(v)),
    };

    const pullResponse = await this.database.client.pull(pullRequest);
    return await this.onPull(pullResponse);
  }

  async pull(pullOrPulls: Pull | Pull[]): Promise<IPullResult> {
    const pulls = Array.isArray(pullOrPulls) ? pullOrPulls : [pullOrPulls];

    for (const pull of pulls) {
      if (pull.objectId < 0 || pull.object?.id < 0) {
        throw new Error('Id is not in the database');
      }

      if (pull.object != null && pull.object.strategy.cls.origin != Origin.Database) {
        throw new Error('Origin is not Database');
      }
    }

    const pullRequest: PullRequest = {
      d: dependenciesToJson(this.dependencies),
      l: pulls.map((v) => pullToJson(v)),
    };

    const pullResponse = await this.database.client.pull(pullRequest);
    return await this.onPull(pullResponse);
  }

  async push(): Promise<IPushResult> {
    const pushRequest: PushRequest = {};

    if (this.pushToDatabaseTracker.created) {
      pushRequest.n = [...this.pushToDatabaseTracker.created].map((v) => ((v.strategy as Strategy).DatabaseOriginState as DatabaseOriginState).pushNew());
    }

    if (this.pushToDatabaseTracker.changed) {
      pushRequest.o = [...this.pushToDatabaseTracker.changed].map((v) => (v as DatabaseOriginState).pushExisting());
    }

    const pushResponse = await this.database.client.push(pushRequest);

    const pushResult = new PushResult(this, pushResponse);

    if (pushResult.hasErrors) {
      throw new ResultError(pushResult);
    }

    if (pushResponse.n != null) {
      for (const pushResponseNewObject of pushResponse.n) {
        const workspaceId = pushResponseNewObject.w;
        const databaseId = pushResponseNewObject.d;
        this.onDatabasePushResponseNew(workspaceId, databaseId);
      }
    }

    this.pushToDatabaseTracker.created = null;
    this.pushToDatabaseTracker.changed = null;

    if (pushRequest.o != null) {
      for (const id of pushRequest.o.map((v) => v.d)) {
        const object = this.getObject(id);
        (object.strategy as Strategy).onDatabasePushed();
      }
    }

    return pushResult;
  }

  private async onPull(pullResponse: PullResponse): Promise<IPullResult> {
    const pullResult = new PullResult(this, pullResponse);

    if (pullResponse.p == null || pullResult.hasErrors) {
      return pullResult;
    }

    const syncRequest = this.database.onPullResonse(pullResponse);
    if (syncRequest.o.length > 0) {
      const syncResponse = await this.database.client.sync(syncRequest);
      const accessRequest = this.database.onSyncResponse(syncResponse);

      if (accessRequest != null) {
        const accessResponse = await this.database.client.access(accessRequest);
        const permissionRequest = this.database.accessResponse(accessResponse);
        if (permissionRequest != null) {
          const permissionResponse = await this.database.client.permission(permissionRequest);
          this.database.permissionResponse(permissionResponse);
        }
      }
    }

    for (const v of pullResponse.p) {
      if (this.objectByWorkspaceId.has(v.i)) {
        const object = this.objectByWorkspaceId.get(v.i);
        (object.strategy as Strategy).DatabaseOriginState.onPulled(pullResult);
      } else {
        this.instantiateDatabaseStrategy(v.i);
      }
    }

    return pullResult;
  }
}
