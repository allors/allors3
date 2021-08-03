import { Injectable } from '@angular/core';
import { IWorkspace } from '@allors/workspace/domain/system';

@Injectable()
export class WorkspaceService {
  public workspace: IWorkspace;
}
