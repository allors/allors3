import { Injectable } from '@angular/core';
import { IReactiveDatabaseClient, ISession, IWorkspace } from '@allors/workspace/domain/system';

import { WorkspaceService } from '../workspace/workspace-service';
import { Context } from './context';

@Injectable({
  providedIn: 'root',
})
export class SessionService {
  session: ISession;
  context: Context;

  constructor(public workspaceService: WorkspaceService) {
    this.session = this.workspaceService.workspace.createSession();
    this.context = this.workspaceService.contextBuilder(this.session); 
  }

  get workspace(): IWorkspace {
    return this.workspaceService.workspace;
  }

  get client(): IReactiveDatabaseClient {
    return this.workspaceService.client;
  }
}
