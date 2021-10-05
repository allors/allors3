import { InvokeRequest, PullRequest, PullResponse, PushRequest } from '@allors/protocol/json/system';
import { IDatabaseClient, IInvokeResult, InvokeOptions, IPullResult, IPushResult, ISession, Method, Procedure, Pull } from '@allors/workspace/domain/system';
import { Origin } from '@allors/workspace/meta/system';
import { procedureToJson, pullToJson } from './json/to-json';
import { DatabaseOriginState } from './session/originstate/database-origin-state';
import { Session } from './session/session';
import { Strategy } from './session/strategy';
import { InvokeResult } from './database/invoke/invoke-result';
import { PushResult } from './database/push/push-result';
import { PullResult } from './database/pull/pull-result';

import { IDatabaseJsonClient } from './idatabase-json-client';
import { DatabaseConnection } from './database/database-connection';

export class DatabaseClient implements IDatabaseClient {
  constructor(public client: IDatabaseJsonClient) {}

  async invoke(session: ISession, methodOrMethods: Method | Method[], options?: InvokeOptions): Promise<IInvokeResult> {
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

    const invokeResponse = await this.client.invoke(invokeRequest);
    return new InvokeResult(session, invokeResponse);
  }

  async pull(session: ISession, pullOrPulls: Pull | Pull[]): Promise<IPullResult> {
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
      l: pulls.map((v) => pullToJson(v)),
    };

    const pullResponse = await this.client.pull(pullRequest);
    return await this.onPull(session as Session, pullResponse);
  }

  async call(session: ISession, procedure: Procedure, ...pulls: Pull[]): Promise<IPullResult> {
    const pullRequest: PullRequest = {
      p: procedureToJson(procedure),
      l: pulls.map((v) => pullToJson(v)),
    };

    const pullResponse = await this.client.pull(pullRequest);
    return await this.onPull(session as Session, pullResponse);
  }

  async push(session: ISession): Promise<IPushResult> {
    const pushToDatabaseTracker = (session as Session).pushToDatabaseTracker;

    const pushRequest: PushRequest = {};

    if (pushToDatabaseTracker.created) {
      pushRequest.n = [...pushToDatabaseTracker.created].map((v) => (v.DatabaseOriginState as DatabaseOriginState).pushNew());
    }

    if (pushToDatabaseTracker.changed) {
      pushRequest.o = [...pushToDatabaseTracker.changed].map((v) => (v.strategy.DatabaseOriginState as DatabaseOriginState).pushExisting());
    }

    const pushResponse = await this.client.push(pushRequest);

    const pushResult = new PushResult(session, pushResponse);

    if (pushResult.hasErrors) {
      return pushResult;
    }

    if (pushResponse.n != null) {
      for (const pushResponseNewObject of pushResponse.n) {
        const workspaceId = pushResponseNewObject.w;
        const databaseId = pushResponseNewObject.d;
        (session as Session).onDatabasePushResponseNew(workspaceId, databaseId);
      }
    }

    pushToDatabaseTracker.created = null;
    pushToDatabaseTracker.changed = null;

    if (pushRequest.o != null) {
      for (const id of pushRequest.o.map((v) => v.d)) {
        const strategy = (session as Session).getStrategy(id);
        strategy.onDatabasePushed();
      }
    }

    return pushResult;
  }

  private async onPull(session: Session, pullResponse: PullResponse): Promise<IPullResult> {
    const pullResult = new PullResult(session, pullResponse);

    if (pullResponse.p == null || pullResult.hasErrors) {
      return pullResult;
    }

    const syncRequest = (session.workspace.database as DatabaseConnection).onPullResonse(pullResponse);
    if (syncRequest.o.length > 0) {
      const database = session.workspace.database as DatabaseConnection;
      const syncResponse = await this.client.sync(syncRequest);
      const accessRequest = database.onSyncResponse(syncResponse);

      if (accessRequest != null) {
        const accessResponse = await this.client.access(accessRequest);
        const permissionRequest = database.accessResponse(accessResponse);
        if (permissionRequest != null) {
          const permissionResponse = await this.client.permission(permissionRequest);
          database.permissionResponse(permissionResponse);
        }
      }
    }

    for (const v of pullResponse.p) {
      if (session.strategyByWorkspaceId.has(v.i)) {
        const strategy = session.strategyByWorkspaceId.get(v.i);
        strategy.DatabaseOriginState.onPulled(pullResult);
      } else {
        session.instantiateDatabaseStrategy(v.i);
      }
    }

    return pullResult;
  }
}
