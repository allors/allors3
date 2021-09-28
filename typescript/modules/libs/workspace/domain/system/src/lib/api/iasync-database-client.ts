import { ISession } from '../isession';
import { Method } from '../method';

import { IInvokeResult } from './pull/iinvoke-result';
import { InvokeOptions } from './pull/invoke-options';
import { IPullResult } from './pull/ipull-result';
import { Procedure } from './pull/procedure';
import { Pull } from './pull/pull';
import { IPushResult } from './push/ipush-result';

export interface IAsyncDatabaseClient {
  invokeAsync(session: ISession, methodOrMethods: Method | Method[], options?: InvokeOptions): Promise<IInvokeResult>;

  callAsync(session: ISession, procedure: Procedure, ...pulls: Pull[]): Promise<IPullResult>;

  pullAsync(session: ISession, pulls: Pull | Pull[]): Promise<IPullResult>;

  pushAsync(session: ISession): Promise<IPushResult>;
}
