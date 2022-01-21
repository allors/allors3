import { Injectable } from '@angular/core';
import { IWorkspace } from '@allors/system/workspace/domain';
import { Context } from '../context/context';

@Injectable({
  providedIn: 'root',
})
export class WorkspaceService {
  workspace: IWorkspace;

  contextBuilder: () => Context;
}
