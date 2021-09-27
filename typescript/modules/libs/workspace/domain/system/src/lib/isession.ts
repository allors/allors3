import { Class, Composite } from '@allors/workspace/meta/system';

import { ISessionServices } from './services/isession-services';

import { IObject } from './iobject';
import { IWorkspace } from './iworkspace';
import { IChangeSet } from './ichangeSet';
import { IWorkspaceResult } from './iworkspaceResult';

export interface ISession {
  workspace: IWorkspace;

  services: ISessionServices;

  pullFromWorkspace(): IWorkspaceResult;

  pushToWorkspace(): IWorkspaceResult;

  hasChangedRoles: boolean;

  checkpoint(): IChangeSet;

  create<T extends IObject>(cls: Class): T;

  instantiate<T extends IObject>(id: number): T;
  instantiate<T extends IObject>(ids: number[]): T[];
  instantiate<T extends IObject>(obj: T): T;
  instantiate<T extends IObject>(objectType: Composite): T[];

  reset(): void;
}
