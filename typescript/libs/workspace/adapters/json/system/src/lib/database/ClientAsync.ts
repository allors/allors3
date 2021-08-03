import { InvokeRequest, PullRequest, PushRequest } from '@allors/protocol/json/system';
import { IClientAsync, IInvokeResult, InvokeOptions, IPullResult, IPushResult, ISession, Method, Procedure, Pull } from '@allors/workspace/domain/system';
import { Origin } from '@allors/workspace/meta/system';
import { procedureToJson, pullToJson } from '../json/toJson';
import { DatabaseOriginState } from '../session/originstate/DatabaseOriginState';
import { Session } from '../session/Session';
import { Strategy } from '../session/Strategy';
import { Client } from './Client';
import { InvokeResult } from './invoke/InvokeResult';
import { PushResult } from './push/PushResult';

export class ClientAsync implements IClientAsync {
  constructor(public client: Client) {}

  async invokeAsync(session: ISession, methods: Method[], options: InvokeOptions): Promise<IInvokeResult> {
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

  async pullAsync(session: ISession, pulls: Pull[]): Promise<IPullResult> {
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
    return await (session as Session).onPull(pullResponse);
  }

  async callAsync(session: ISession, procedure: Procedure, ...pulls: Pull[]): Promise<IPullResult> {
    const pullRequest: PullRequest = {
      p: procedureToJson(procedure),
      l: pulls.map((v) => pullToJson(v)),
    };

    const pullResponse = await this.client.pull(pullRequest);
    return await (session as Session).onPull(pullResponse);
  }

  async pushAsync(session: ISession): Promise<IPushResult> {
    const pushToDatabaseTracker = (session as Session).pushToDatabaseTracker;

    const pushRequest: PushRequest = {
      n: [...pushToDatabaseTracker.created].map((v) => (v.DatabaseOriginState as DatabaseOriginState).pushNew()),
      o: [...pushToDatabaseTracker.changed].map((v) => (v.strategy.DatabaseOriginState as DatabaseOriginState).pushExisting()),
    };

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
}
