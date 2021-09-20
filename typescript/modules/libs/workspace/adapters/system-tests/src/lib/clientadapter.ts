import { IAsyncDatabaseClient, IInvokeResult, InvokeOptions, IPullResult, IPushResult, IReactiveDatabaseClient, ISession, Method, Procedure, Pull } from '@allors/workspace/domain/system';

export class ClientAdapter implements IAsyncDatabaseClient {
  constructor(private reactiveClient: IReactiveDatabaseClient) {}

  invokeAsync(session: ISession, method: Method | Method[], options?: InvokeOptions): Promise<IInvokeResult> {
    return this.reactiveClient.invokeReactive(session, method, options).toPromise();
  }

  callAsync(session: ISession, procedure: Procedure, ...pulls: Pull[]): Promise<IPullResult> {
    return this.reactiveClient.callReactive(session, procedure, ...pulls).toPromise();
  }

  pullAsync(session: ISession, pulls: Pull[]): Promise<IPullResult> {
    return this.reactiveClient.pullReactive(session, pulls).toPromise();
  }

  pushAsync(session: ISession): Promise<IPushResult> {
    return this.reactiveClient.pushReactive(session).toPromise();
  }
}
