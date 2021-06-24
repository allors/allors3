import { InvokeRequest, PullRequest, PushRequest } from '@allors/protocol/json/system';
import { InvokeResult, Session as SystemSession, Strategy } from '@allors/workspace/adapters/system';
import { IInvokeResult, InvokeOptions, IPullResult, IPushResult, Method, Procedure, Pull } from '@allors/workspace/domain/system';

export class Session extends SystemSession {
  async invoke(methods: Method[], options: InvokeOptions): Promise<IInvokeResult> {
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

    const response = await this.workspace.database.invoke(invokeRequest);
    return new InvokeResult(this, response);
  }

  async pull(pulls: Pull[]): Promise<IPullResult> {
    const pullRequest: PullRequest = {
      l: pulls.map((v) => toJson(v)),
    };

    const result = await this.workspace.database.pull(pullRequest);
    return this.OnPull(result);
  }

  async call(procedure: Procedure, ...pulls: Pull[]): Promise<IPullResult> {
    const pullRequest: PullRequest = {
      p: procedure.ToJson(this.database.UnitConvert),
      l: pulls.map((v) => v.ToJson(this.database.UnitConvert)),
    };

    const response = await this.workspace.DatabaseConnection.Pull(pullRequest);
    return await this.OnPull(response);
  }

  async push(): Promise<IPushResult> {
    const pushRequest: PushRequest = {
      n: [...this.pushToDatabaseTracker.Created].map((v) => v.DatabasePushNew()),
      o: [...this.pushToDatabaseTracker.Changed].map((v) => v.Strategy.DatabasePushExisting()),
    };

    const result = await this.workspace.database.push(pushRequest);
    return new PushResult(this, result);

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



}
