import { Class, Composite } from '@allors/system/workspace/meta';

import { IObject } from './iobject';
import { IWorkspace } from './iworkspace';
import { IChangeSet } from './ichange-set';
import { IRule } from './derivation/irule';
import { Method } from './method';
import { InvokeOptions, Procedure, Pull } from '@allors/system/workspace/connection';
import { IInvokeResult } from './api/pull/iinvoke-result';
import { IPullResult } from './api/pull/ipull-result';
import { IPushResult } from './api/push/ipush-result';

export interface ISession {
  workspace: IWorkspace;

  context: string;

  hasChanges: boolean;

  activate(rules: IRule<IObject>[]): void;

  reset(): void;

  checkpoint(): IChangeSet;

  create<T extends IObject>(cls: Class): T;

  instantiate<T extends IObject>(id: number): T;
  instantiate<T extends IObject>(ids: number[]): T[];
  instantiate<T extends IObject>(obj: T): T;
  instantiate<T extends IObject>(objectType: Composite): T[];

  invoke(
    methodOrMethods: Method | Method[],
    options?: InvokeOptions
  ): Promise<IInvokeResult>;

  call(procedure: Procedure, ...pulls: Pull[]): Promise<IPullResult>;

  pull(pulls: Pull | Pull[]): Promise<IPullResult>;

  push(): Promise<IPushResult>;
}
