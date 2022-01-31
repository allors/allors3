import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import { Directive } from '@angular/core';
import { OverviewPageService } from '../overview/overview.service';
import { PanelMode } from '../panel/panel';
import { PanelService } from '../panel/panel-manager.service';
import { AllorsDetailPanelComponent } from './detail-panel.component';

@Directive()
export abstract class AllorsDetailViewPanelComponent extends AllorsDetailPanelComponent {
  dataAllorsKind = 'rel-view-panel';

  panelId = 'DetailView';

  panelMode: PanelMode = 'View';

  constructor(
    overviewService: OverviewPageService,
    panelService: PanelService,
    workspaceService: WorkspaceService
  ) {
    super(overviewService, panelService, workspaceService);
  }
}
