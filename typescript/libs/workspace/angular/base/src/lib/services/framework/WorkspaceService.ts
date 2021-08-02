import { Injectable } from '@angular/core';
import { Workspace } from '@allors/workspace/domain/system';

@Injectable()
export class WorkspaceService {
  public workspace: Workspace;
}
