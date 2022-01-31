import { HostBinding, Directive } from '@angular/core';
import {
  AllorsComponent,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { OverviewPageService } from './overview.service';
import { M } from '@allors/default/workspace/meta';

@Directive()
export abstract class AllorsOverviewPageComponent extends AllorsComponent {
  override dataAllorsKind = 'overview';

  @HostBinding('attr.data-allors-id')
  get dataAllorsId() {
    return this.overviewService?.id;
  }

  m: M;

  constructor(
    public overviewService: OverviewPageService,
    workspaceService: WorkspaceService
  ) {
    super();

    this.m = workspaceService.workspace.configuration.metaPopulation as M;
  }
}
