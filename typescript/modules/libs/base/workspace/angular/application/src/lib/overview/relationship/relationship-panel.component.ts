import { Directive, HostBinding } from '@angular/core';
import { RoleType } from '@allors/system/workspace/meta';
import {
  OnPullService,
  RefreshService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { Panel } from '../../panel/panel';
import { AllorsOverviewOnPullPanelComponent } from '../overview-panel-onpull.component';
import { OverviewPageService } from '../overview-page.service';
import { PanelService } from '../../panel/panel.service';

@Directive()
export abstract class AllorsRelationshipPanelComponent
  extends AllorsOverviewOnPullPanelComponent
  implements Panel
{
  @HostBinding('attr.data-allors-anchor')
  get dataAllorsFromRelationType() {
    return this.anchor.relationType.tag;
  }

  @HostBinding('attr.data-allors-target')
  get dataAllorsToRelationType() {
    return this.target.relationType.tag;
  }

  abstract anchor: RoleType;

  abstract target: RoleType;

  constructor(
    public overviewService: OverviewPageService,
    public panelService: PanelService,
    public onPullService: OnPullService,
    public refreshService: RefreshService,
    workspaceService: WorkspaceService
  ) {
    super(
      overviewService,
      panelService,
      onPullService,
      refreshService,
      workspaceService
    );
  }
}
