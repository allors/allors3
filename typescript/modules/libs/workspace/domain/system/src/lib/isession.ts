import { Class, Composite } from '@allors/workspace/meta/system';

import { IValidation } from './derivation/ivalidation';
import { IObject } from './iobject';
import { IWorkspace } from './iworkspace';
import { IChangeSet } from './ichange-set';
import { IWorkspaceResult } from './iworkspace-result';

export interface ISession {
  workspace: IWorkspace;

  hasChanges: boolean;

  derive(): IValidation;

  reset(): void;

  pullFromWorkspace(): IWorkspaceResult;

  pushToWorkspace(): IWorkspaceResult;

  checkpoint(): IChangeSet;

  create<T extends IObject>(cls: Class): T;

  instantiate<T extends IObject>(id: number): T;
  instantiate<T extends IObject>(ids: number[]): T[];
  instantiate<T extends IObject>(obj: T): T;
  instantiate<T extends IObject>(objectType: Composite): T[];
}
