import { MetaPopulation } from '@allors/system/workspace/meta';
import { Injectable } from '@angular/core';
import { WorkspaceService } from '../workspace/workspace-service';

import { Context } from './context';

@Injectable({
  providedIn: 'root',
})
export class ContextService {
  context: Context;

  get metaPopulation(): MetaPopulation {
    return this.workspaceService.metaPopulation;
  }

  constructor(public workspaceService: WorkspaceService) {
    this.context = this.workspaceService.contextBuilder();
  }
}
