import { Class, Composite } from '@allors/workspace/meta/system';
import { IObject } from './IObject';
import { IWorkspace } from './IWorkspace';
import { IChangeSet } from './IChangeSet';
import { IWorkspaceResult } from './IWorkspaceResult';

import { ISessionServices } from './services/ISessionServices';

export interface ISession {
  workspace: IWorkspace;

  services: ISessionServices;

  pullFromWorkspace(): IWorkspaceResult;

  pushToWorkspace(): IWorkspaceResult;

  checkpoint(): IChangeSet;

  create<T extends IObject>(cls: Class): T;

  instantiate<T extends IObject>(id: number): T;
  instantiate<T extends IObject>(ids: number[]): T[];
  instantiate<T extends IObject>(objectType: Composite): T[];
}
