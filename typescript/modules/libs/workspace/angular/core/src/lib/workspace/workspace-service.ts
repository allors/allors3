import { Injectable } from '@angular/core';
import { IReactiveDatabaseClient, ISession, IWorkspace } from '@allors/workspace/domain/system';
import { Context } from '../session/context';

@Injectable({
  providedIn: 'root',
})
export class WorkspaceService {
  workspace: IWorkspace;

  client: IReactiveDatabaseClient;

  contextBuilder: (session: ISession) => Context;
}
