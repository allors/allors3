import { IObject } from './IObject';

export interface IWorkspaceResult {
  hasErrors: boolean;

  versionErrors: IObject[];

  missingErrors: IObject[];

  mergeErrors: IObject[];
}
