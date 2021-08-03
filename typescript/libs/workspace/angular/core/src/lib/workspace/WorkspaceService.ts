import { Injectable } from '@angular/core';
import { IWorkspace } from '@allors/workspace/domain/system';

@Injectable({
  providedIn: 'root',
})
export class WorkspaceService {
  workspace: IWorkspace;
}
