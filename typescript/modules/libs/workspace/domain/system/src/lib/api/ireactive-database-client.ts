import type { Observable } from 'rxjs';

import { ISession } from '../isession';
import { Method } from '../method';

import { IInvokeResult } from './pull/iinvoke-result';
import { InvokeOptions } from './pull/invoke-options';
import { IPullResult } from './pull/ipull-result';
import { Procedure } from './pull/procedure';
import { Pull } from './pull/pull';
import { IPushResult } from './push/ipush-result';

export interface IReactiveDatabaseClient {
  invokeReactive(session: ISession, method: Method | Method[], options?: InvokeOptions): Observable<IInvokeResult>;

  callReactive(session: ISession, procedure: Procedure, ...pulls: Pull[]): Observable<IPullResult>;

  pullReactive(session: ISession, pulls: Pull[]): Observable<IPullResult>;

  pushReactive(session: ISession): Observable<IPushResult>;
}
