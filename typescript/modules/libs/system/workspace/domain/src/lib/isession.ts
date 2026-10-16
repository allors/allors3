import { Class, Composite } from '@allors/system/workspace/meta';

import { IObject } from './iobject';
import { IWorkspace } from './iworkspace';
import { IChangeSet } from './ichange-set';
import { IRule } from './derivation/irule';
import { Method } from './method';
import { InvokeOptions } from './api/pull/invoke-options';
import { IInvokeResult } from './api/pull/iinvoke-result';
import { Pull } from './api/pull/pull';
import { IPullResult } from './api/pull/ipull-result';
import { PushOptions } from './api/push/push-options';
import { IPushResult } from './api/push/ipush-result';

export interface ISession {
  close(): void;

  workspace: IWorkspace;

  context: string;

  hasChanges: boolean;

  readonly reconciliationRequired: boolean;

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
    options?: InvokeOptions,
    cancellation?: AbortSignal
  ): Promise<IInvokeResult>;

  pull(pulls: Pull | Pull[]): Promise<IPullResult>;

  push(options?: PushOptions, cancellation?: AbortSignal): Promise<IPushResult>;
}
