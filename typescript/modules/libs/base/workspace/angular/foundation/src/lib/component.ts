import { M } from '@allors/default/workspace/meta';
import { HostBinding, Directive } from '@angular/core';
import { WorkspaceService } from './workspace/workspace-service';

@Directive()
export abstract class AllorsComponent {
  @HostBinding('attr.data-allors-kind')
  dataAllorsKind: string;

  @HostBinding('attr.data-allors-component')
  dataAllorsComponent = this.constructor.name;

  m: M;

  constructor(public workspaceService: WorkspaceService) {
    this.m = workspaceService.workspace.configuration.metaPopulation as M;
  }
}
