import { Injectable } from '@angular/core';
import { IWorkspace } from '@allors/system/workspace/domain';
import { Context } from '../context/context';
import { MetaPopulation } from '@allors/system/workspace/meta';

@Injectable({
  providedIn: 'root',
})
export class WorkspaceService {
  workspace: IWorkspace;

  get metaPopulation(): MetaPopulation {
    return this.workspace.configuration.metaPopulation;
  }

  contextBuilder: () => Context;
}
