import { Class, Composite } from '@allors/workspace/meta/system';

import { IObject } from './iobject';
import { IWorkspace } from './iworkspace';
import { IChangeSet } from './ichange-set';
import { IWorkspaceResult } from './iworkspace-result';
import { IRule } from './derivation/irule';
import { Method } from './method';
import { InvokeOptions } from './api/pull/invoke-options';
import { IInvokeResult } from './api/pull/iinvoke-result';
import { Procedure } from './api/pull/procedure';
import { Pull } from './api/pull/pull';
import { IPullResult } from './api/pull/ipull-result';
import { IPushResult } from './api/push/ipush-result';

export interface ISession {
  workspace: IWorkspace;

  hasChanges: boolean;

  activate(rules: IRule<IObject>[]): void;

  reset(): void;

  pullFromWorkspace(): IWorkspaceResult;

  pushToWorkspace(): IWorkspaceResult;

  checkpoint(): IChangeSet;

  create<T extends IObject>(cls: Class): T;

  instantiate<T extends IObject>(id: number): T;
  instantiate<T extends IObject>(ids: number[]): T[];
  instantiate<T extends IObject>(obj: T): T;
  instantiate<T extends IObject>(objectType: Composite): T[];

  invoke(methodOrMethods: Method | Method[], options?: InvokeOptions): Promise<IInvokeResult>;

  call(procedure: Procedure, ...pulls: Pull[]): Promise<IPullResult>;

  pull(pulls: Pull | Pull[]): Promise<IPullResult>;

  push(): Promise<IPushResult>;
}
