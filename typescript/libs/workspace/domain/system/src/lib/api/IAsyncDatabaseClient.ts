import { ISession } from '../ISession';
import { Method } from '../Method';

import { IInvokeResult } from './pull/IInvokeResult';
import { InvokeOptions } from './pull/InvokeOptions';
import { IPullResult } from './pull/IPullResult';
import { Procedure } from './pull/Procedure';
import { Pull } from './pull/Pull';
import { IPushResult } from './push/IPushResult';

export interface IAsyncDatabaseClient {
  invokeAsync(session: ISession, methodOrMethods: Method | Method[], options?: InvokeOptions): Promise<IInvokeResult>;

  callAsync(session: ISession, procedure: Procedure, ...pulls: Pull[]): Promise<IPullResult>;

  pullAsync(session: ISession, pulls: Pull[]): Promise<IPullResult>;

  pushAsync(session: ISession): Promise<IPushResult>;
}
