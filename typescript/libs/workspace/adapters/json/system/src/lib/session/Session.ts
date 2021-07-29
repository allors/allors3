import { InvokeRequest, PullRequest, PullResponse, PushRequest } from '@allors/protocol/json/system';
import { Session as SystemSession } from '@allors/workspace/adapters/system';
import { IInvokeResult, InvokeOptions, IObject, IPullResult, IPushResult, ISessionServices, Method, Procedure, Pull } from '@allors/workspace/domain/system';
import { Class, Origin } from '@allors/workspace/meta/system';
import { procedureToJson, pullToJson } from '../json/toJson';
import { Database } from '../database/Database';
import { DatabaseRecord } from '../database/DatabaseRecord';
import { InvokeResult } from '../database/invoke/InvokeResult';
import { PullResult } from '../database/pull/PullResult';
import { PushResult } from '../database/push/PushResult';
import { Workspace } from '../workspace/Workspace';
import { DatabaseOriginState } from './originstate/DatabaseOriginState';
import { Strategy } from './Strategy';

export class Session extends SystemSession {
  database: Database;

  constructor(workspace: Workspace, services: ISessionServices) {
    super(workspace, services);

    this.services.onInit(this);
    this.database = this.workspace.database as Database;
  }

  async invoke(methods: Method[], options: InvokeOptions): Promise<IInvokeResult> {
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

    const invokeResponse = await this.database.invoke(invokeRequest);
    return new InvokeResult(this, invokeResponse);
  }

  async pull(pulls: Pull[]): Promise<IPullResult> {
    for (const pull of pulls) {
      if (pull.objectId < 0 || pull.object?.id < 0) {
        throw new Error('Id is not in the database');
      }

      if (pull.object != null && pull.object.strategy.cls.origin != Origin.Database) {
        throw new Error('Origin is not Database');
      }
    }

    const pullRequest: PullRequest = {
      l: pulls.map((v) => pullToJson(v)),
    };

    const pullResponse = await this.database.pull(pullRequest);
    return await this.onPull(pullResponse);
  }

  async call(procedure: Procedure, ...pulls: Pull[]): Promise<IPullResult> {
    const pullRequest: PullRequest = {
      p: procedureToJson(procedure),
      l: pulls.map((v) => pullToJson(v)),
    };

    const pullResponse = await this.database.pull(pullRequest);
    return await this.onPull(pullResponse);
  }

  async push(): Promise<IPushResult> {
    const pushRequest: PushRequest = {
      n: [...this.pushToDatabaseTracker.created].map((v) => (v.DatabaseOriginState as DatabaseOriginState).pushNew()),
      o: [...this.pushToDatabaseTracker.changed].map((v) => (v.strategy.DatabaseOriginState as DatabaseOriginState).pushExisting()),
    };

    const pushResponse = await this.database.push(pushRequest);

    const pushResult = new PushResult(this, pushResponse);

    if (pushResult.hasErrors) {
      return pushResult;
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
        const strategy = this.getStrategy(id);
        strategy.onDatabasePushed();
      }
    }

    return pushResult;
  }

  create<T extends IObject>(cls: Class): T {
    const workspaceId = this.workspace.database.nextId();
    const strategy = new Strategy(this, cls, workspaceId);
    this.addStrategy(strategy);

    if (cls.origin != Origin.Session) {
      this.pushToWorkspaceTracker.onCreated(strategy);
      if (cls.origin == Origin.Database) {
        this.pushToDatabaseTracker.onCreated(strategy);
      }
    }

    this.changeSetTracker.onCreated(strategy);
    return strategy.object as T;
  }

  instantiateDatabaseStrategy(id: number): void {
    const databaseRecord = this.workspace.database.getRecord(id) as DatabaseRecord;
    const strategy = Strategy.fromDatabaseRecord(this, databaseRecord);
    this.addStrategy(strategy);
    this.changeSetTracker.onInstantiated(strategy);
  }

  instantiateWorkspaceStrategy(id: number): Strategy {
    if (!this.workspace.workspaceClassByWorkspaceId.has(id)) {
      return null;
    }

    const cls = this.workspace.workspaceClassByWorkspaceId.get(id);

    const strategy = new Strategy(this, cls, id);
    this.addStrategy(strategy);

    this.changeSetTracker.onInstantiated(strategy);

    return strategy;
  }

  private async onPull(pullResponse: PullResponse): Promise<IPullResult> {

    const pullResult = new PullResult(this, pullResponse);

    const syncRequest = (this.workspace.database as Database).onPullResonse(pullResponse);
    if (syncRequest.o.length > 0) {
      const database = this.workspace.database as Database;
      const syncResponse = await database.sync(syncRequest);
      let securityRequest = database.onSyncResponse(syncResponse);

      for (const v of syncResponse.o) {
        if (!this.strategyByWorkspaceId.has(v.i)) {
          this.instantiateDatabaseStrategy(v.i);
        } else {
          const strategy = this.strategyByWorkspaceId.get(v.i);
          strategy.DatabaseOriginState.onPulled(pullResult);
        }
      }

      if (securityRequest != null) {
        let securityResponse = await database.security(securityRequest);
        securityRequest = database.securityResponse(securityResponse);
        if (securityRequest != null) {
          securityResponse = await database.security(securityRequest);
          database.securityResponse(securityResponse);
        }
      }
    }

    for (const v of pullResponse.p) {
      if (this.strategyByWorkspaceId.has(v.i)) {
        const strategy = this.strategyByWorkspaceId.get(v.i);
        strategy.DatabaseOriginState.onPulled(pullResult);
      } else {
        this.instantiateDatabaseStrategy(v.i);
      }
    }

    return pullResult;
  }
}
