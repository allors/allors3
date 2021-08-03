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

  async onPull(pullResponse: PullResponse): Promise<IPullResult> {

    const pullResult = new PullResult(this, pullResponse);

    const syncRequest = (this.workspace.database as Database).onPullResonse(pullResponse);
    if (syncRequest.o.length > 0) {
      const database = this.workspace.database as Database;
      const syncResponse = await database.client.sync(syncRequest);
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
