import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import { Directive } from '@angular/core';
import { OverviewPageService } from '../overview/overview.service';
import { ViewPanel } from '../panel/panel';
import { PanelService } from '../panel/panel.service';
import { AllorsDetailPanelComponent } from './detail-panel.component';

@Directive()
export abstract class AllorsViewDetailPanelComponent
  extends AllorsDetailPanelComponent
  implements ViewPanel
{
  dataAllorsKind = 'view-detail-panel';

  panelId = 'ViewDetail';

  panelMode: 'View' = 'View';

  constructor(
    overviewService: OverviewPageService,
    panelService: PanelService,
    workspaceService: WorkspaceService
  ) {
    super(overviewService, panelService, workspaceService);
  }
}
