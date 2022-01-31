import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import { Directive } from '@angular/core';
import { OverviewPageService } from '../overview/overview.service';
import { PanelMode } from '../panel/panel';
import { PanelService } from '../panel/panel-manager.service';
import { AllorsSummaryPanelComponent } from './summary-panel.component';

@Directive()
export abstract class AllorsSummaryEditPanelComponent extends AllorsSummaryPanelComponent {
  dataAllorsKind = 'summary-edit-panel';

  panelId = 'SummaryEdit';

  panelMode: PanelMode = 'Edit';

  constructor(
    overviewService: OverviewPageService,
    panelService: PanelService,
    workspaceService: WorkspaceService
  ) {
    super(overviewService, panelService, workspaceService);
  }
}
