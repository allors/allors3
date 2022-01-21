import { IObject } from './iobject';

export interface IWorkspaceResult {
  hasErrors: boolean;

  versionErrors: IObject[];

  missingErrors: IObject[];

  mergeErrors: IObject[];
}
