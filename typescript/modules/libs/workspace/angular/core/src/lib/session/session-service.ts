import { Injectable } from '@angular/core';
import { IReactiveDatabaseClient, ISession, IWorkspace } from '@allors/workspace/domain/system';

import { WorkspaceService } from '../workspace/workspace-service';

@Injectable({
  providedIn: 'root',
})
export class SessionService {
  session: ISession;

  constructor(public workspaceService: WorkspaceService) {
    this.session ??= this.workspaceService.workspace.createSession();
  }

  get workspace(): IWorkspace {
    return this.workspaceService.workspace;
  }

  get client(): IReactiveDatabaseClient {
    return this.workspaceService.client;
  }
}
