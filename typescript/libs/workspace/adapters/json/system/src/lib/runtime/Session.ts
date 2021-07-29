import { InvokeRequest, PullRequest, PullResponse, PushRequest } from '@allors/protocol/json/system';
import { Session as SystemSession } from '@allors/workspace/adapters/system';
import { IInvokeResult, InvokeOptions, IObject, IPullResult, IPushResult, ISessionServices, Method, Procedure, Pull } from '@allors/workspace/domain/system';
import { Class, Origin } from '@allors/workspace/meta/system';
import { procedureToJson, pullToJson } from '../json/toJson';
import { Database } from './Database';
import { DatabaseOriginState } from './DatabaseOriginState';
import { DatabaseRecord } from './DatabaseRecord';
import { InvokeResult } from './invoke/InvokeResult';
import { PullResult } from './pull/PullResult';
import { PushResult } from './push/PushResult';
import { Strategy } from './Strategy';
import { Workspace } from './Workspace';

export class Session extends SystemSession {
  get database(): Database {
    return this.workspace.database as Database;
  }

  constructor(workspace: Workspace, services: ISessionServices) {
    super(workspace, services);

    this.services.onInit(this);
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

    const response = await this.database.invoke(invokeRequest);
    return new InvokeResult(this, response);
  }

  async pull(pulls: Pull[]): Promise<IPullResult> {
    const pullRequest: PullRequest = {
      l: pulls.map((v) => pullToJson(v)),
    };

    const result = await this.database.pull(pullRequest);
    return this.OnPull(result);
  }

  async call(procedure: Procedure, ...pulls: Pull[]): Promise<IPullResult> {
    const pullRequest: PullRequest = {
      p: procedureToJson(procedure),
      l: pulls.map((v) => pullToJson(v)),
    };

    const response = await this.database.pull(pullRequest);
    return await this.OnPull(response);
  }

  async push(): Promise<IPushResult> {
    const pushRequest: PushRequest = {
      n: [...this.pushToDatabaseTracker.created].map((v) => (v.DatabaseOriginState as DatabaseOriginState).pushNew()),
      o: [...this.pushToDatabaseTracker.changed].map((v) => (v.strategy.DatabaseOriginState as DatabaseOriginState).pushExisting()),
    };

    const pushResponse = await this.database.push(pushRequest);

    const result = new PushResult(this, pushResponse);

    if(result.hasErrors){
      return result;
    }

    if ((pushResponse.n != null)) {
        for (let pushResponseNewObject of pushResponse.n) {
            let workspaceId = pushResponseNewObject.w;
            let databaseId = pushResponseNewObject.d;
            this.onDatabasePushResponseNew(workspaceId, databaseId);
        }
    }

    this.pushToDatabaseTracker.created = null;

    if ((pushRequest.o != null)) {
        for (let id of pushRequest.o.map(v => v.d)) {
            let strategy = this.getStrategy(id);
            this.onDatabasePushResponse(strategy);
        }

    }

    if (!result.hasErrors) {
        this.PushToWorkspace(result);
    }
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

  instantiateDatabaseStrategy(id: number): Strategy {
    const databaseRecord = this.workspace.database.getRecord(id) as DatabaseRecord;
    const strategy = Strategy.fromDatabaseRecord(this, databaseRecord);
    this.addStrategy(strategy);
    this.changeSetTracker.onInstantiated(strategy);
    return strategy;
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

  private async OnPull(pullResponse: PullResponse): Promise<IPullResult> {
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
          strategy.DatabaseOriginState.onPulled();
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
        strategy.DatabaseOriginState.onPulled();
      } else {
        this.instantiateDatabaseStrategy(v.i);
      }
    }

    return new PullResult(this, pullResponse);
  }
}
