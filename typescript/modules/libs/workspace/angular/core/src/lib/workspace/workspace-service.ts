import { Injectable } from '@angular/core';
import { IWorkspace, IDatabaseClient } from '@allors/workspace/domain/system';
import { Context } from '../context/context';

@Injectable({
  providedIn: 'root',
})
export class WorkspaceService {
  workspace: IWorkspace;

  client: IDatabaseClient;

  contextBuilder: () => Context;
}
