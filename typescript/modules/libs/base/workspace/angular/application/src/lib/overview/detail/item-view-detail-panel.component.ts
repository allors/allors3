import {
  OnPullService,
  RefreshService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { Directive } from '@angular/core';
import { PanelService } from '../../panel/panel.service';
import { OverviewPageService } from '../overview-page.service';
import { AllorsOverviewOnPullPanelComponent } from '../overview-panel-onpull.component';

@Directive()
export abstract class AllorsItemViewDetailPanelComponent extends AllorsOverviewOnPullPanelComponent {
  dataAllorsKind = 'item-view-detail-panel';

  panelId = 'Detail';

  readonly panelMode = 'View';

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
