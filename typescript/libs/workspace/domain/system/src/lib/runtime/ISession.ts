import { Class, Composite } from '@allors/workspace/meta/system';
import { IObject } from './IObject';
import { IWorkspace } from './IWorkspace';
import { IChangeSet } from './IChangeSet';
import { InvokeOptions } from '../api/pull/InvokeOptions';
import { IInvokeResult } from '../api/pull/IInvokeResult';
import { Procedure } from '../api/pull/Procedure';
import { Pull } from '../api/pull/Pull';
import { IPullResult } from '../api/pull/IPullResult';
import { IPushResult } from '../api/push/IPushResult';
import { ISessionServices } from './ISessionServices';
import { Method } from './Method';

export interface ISession {
  workspace: IWorkspace;

  services: ISessionServices;

  create(cls: Class): IObject;

  getOne<T extends IObject>(id: number): T;

  getMany<T extends IObject>(ids: number[]): T[];

  getAll<T extends IObject>(objectType?: Composite): T[];

  checkpoint(): IChangeSet;

  invoke(method: Method | Method[], options?: InvokeOptions): Promise<IInvokeResult>;

  call(procedure: Procedure, ...pulls: Pull[]): Promise<IPullResult>;

  pull(pulls: Pull[]): Promise<IPullResult>;

  push(): Promise<IPushResult>;
}
