import { IObject, IWorkspaceResult } from '@allors/system/workspace/domain';
export class WorkspaceResult implements IWorkspaceResult {
  get hasErrors(): boolean {
    return (
      this.versionErrors?.length > 0 ||
      this.missingErrors?.length > 0 ||
      this.mergeErrors?.length > 0
    );
  }

  versionErrors: IObject[];

  missingErrors: IObject[];

  mergeErrors: IObject[];

  addMergeError(object: IObject) {
    this.mergeErrors ??= [];
    this.mergeErrors.push(object);
  }
}
