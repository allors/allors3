import { Injectable } from '@angular/core';
import { ISession } from '@allors/workspace/domain/system';
import { WorkspaceService } from '../workspace/WorkspaceService';

@Injectable({
  providedIn: 'root',
})
export class SessionService {
  session: ISession;

  constructor(public workspaceService: WorkspaceService) {
    this.session ??= this.workspaceService.workspace.createSession();
  }
}
