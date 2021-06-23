import { Observable } from 'rxjs';
import { Composite, Class } from '@allors/workspace/system';
import { IObject } from './IObject';
import { IWorkspace } from './IWorkspace';
import { Method } from './operands/Method';
import { ISessionServices } from './state/ISessionServices';
import { IChangeSet } from './IChangeSet';
import { InvokeOptions } from '../api/pull/InvokeOptions';
import { IInvokeResult } from '../api/pull/IInvokeResult';
import { Pull } from '../api/pull/Pull';
import { IPullResult } from '../api/pull/IPullResult';
import { Procedure } from '../api/pull/Procedure';
import { IPushResult } from '../api/push/IPushResult';

export interface ISession {
  workspace: IWorkspace;

  services: ISessionServices;

  create(cls: Class): IObject;

  getOne<T extends IObject>(id: number): T;

  getMany<T extends IObject>(...ids: number[]): Generator<T, void, unknown>;

  getAll<T extends IObject>(objectType?: Composite): Generator<T, void, unknown>;

  invoke(method: Method | Method[], options?: InvokeOptions): Observable<IInvokeResult>;

  pull(...pulls: Pull[]): Observable<IPullResult>;

  call(procedure: Procedure, ...pulls: Pull[]): Observable<IPullResult>;

  push(): Observable<IPushResult>;

  checkpoint(): IChangeSet;
}
