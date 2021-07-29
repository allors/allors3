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
import { IWorkspaceResult } from './IWorkspaceResult';

export interface ISession {
  workspace: IWorkspace;

  services: ISessionServices;

  invoke(method: Method | Method[], options?: InvokeOptions): Promise<IInvokeResult>;

  call(procedure: Procedure, ...pulls: Pull[]): Promise<IPullResult>;

  pull(pulls: Pull[]): Promise<IPullResult>;

  push(): Promise<IPushResult>;

  pullFromWorkspace(): IWorkspaceResult;

  pushToWorkspace(): IWorkspaceResult;

  checkpoint(): IChangeSet;

  create(cls: Class): IObject;

  instantiate<T extends IObject>(id: number): T;
  instantiate<T extends IObject>(ids: number[]): T[];
  instantiate<T extends IObject>(objectType: Composite): T[];
}
