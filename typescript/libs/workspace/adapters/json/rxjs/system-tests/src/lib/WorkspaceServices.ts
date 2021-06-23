import { IWorkspace, IWorkspaceServices } from '@allors/workspace/domain/system';
import { SessionServices } from './SessionServices';

export class WorkspaceServices implements IWorkspaceServices {
  workspace: IWorkspace;

  onInit(workspace: IWorkspace): void {
    this.workspace = workspace;
  }

  createSessionServices() {
    return new SessionServices();
  }
}
