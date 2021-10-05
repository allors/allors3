import { ISession } from '../isession';
import { Method } from '../method';

import { IInvokeResult } from './pull/iinvoke-result';
import { InvokeOptions } from './pull/invoke-options';
import { IPullResult } from './pull/ipull-result';
import { Procedure } from './pull/procedure';
import { Pull } from './pull/pull';
import { IPushResult } from './push/ipush-result';

export interface IDatabaseClient {
  invoke(session: ISession, methodOrMethods: Method | Method[], options?: InvokeOptions): Promise<IInvokeResult>;

  call(session: ISession, procedure: Procedure, ...pulls: Pull[]): Promise<IPullResult>;

  pull(session: ISession, pulls: Pull | Pull[]): Promise<IPullResult>;

  push(session: ISession): Promise<IPushResult>;
}
