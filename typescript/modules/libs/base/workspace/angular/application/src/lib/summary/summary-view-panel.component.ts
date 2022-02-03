import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import { Directive } from '@angular/core';
import { OverviewPageService } from '../overview/overview.service';
import { PanelService } from '../panel/panel.service';
import { AllorsSummaryPanelComponent } from './summary-panel.component';

@Directive()
export abstract class AllorsSummaryViewPanelComponent extends AllorsSummaryPanelComponent {
  dataAllorsKind = 'rel-view-panel';

  panelId = 'SummaryView';

  readonly panelMode = 'View';

  constructor(
    overviewService: OverviewPageService,
    panelService: PanelService,
    workspaceService: WorkspaceService
  ) {
    super(overviewService, panelService, workspaceService);
  }
}
