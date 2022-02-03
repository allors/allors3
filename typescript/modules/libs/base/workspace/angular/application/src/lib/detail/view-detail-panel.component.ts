import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import { Directive } from '@angular/core';
import { OverviewPageService } from '../overview/overview.service';
import { PanelService } from '../panel/panel.service';
import { AllorsDetailPanelComponent } from './detail-panel.component';

@Directive()
export abstract class AllorsViewDetailPanelComponent extends AllorsDetailPanelComponent {
  dataAllorsKind = 'view-detail-panel';

  panelId = 'Detail';

  readonly panelMode = 'View';

  constructor(
    overviewService: OverviewPageService,
    panelService: PanelService,
    workspaceService: WorkspaceService
  ) {
    super(overviewService, panelService, workspaceService);
  }
}
