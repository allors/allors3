import { Directive } from '@angular/core';
import {
  OnPullService,
  RefreshService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { OverviewPageService } from '../overview-page.service';
import { PanelService } from '../../panel/panel.service';
import { AllorsSummaryPanelComponent } from './summary-panel.component';

@Directive()
export abstract class AllorsSummaryEditPanelComponent extends AllorsSummaryPanelComponent {
  dataAllorsKind = 'view-summary-panel';

  panelId = 'ViewSummary';

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
