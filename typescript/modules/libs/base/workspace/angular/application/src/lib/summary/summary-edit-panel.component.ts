import { Observable, of } from 'rxjs';
import { Directive } from '@angular/core';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import { OverviewPageService } from '../overview/overview.service';
import { PanelService } from '../panel/panel.service';
import { AllorsSummaryPanelComponent } from './summary-panel.component';

@Directive()
export abstract class AllorsSummaryEditPanelComponent extends AllorsSummaryPanelComponent {
  dataAllorsKind = 'edit-summary-panel';

  panelId = 'EditSummary';

  readonly panelMode = 'Edit';

  constructor(
    overviewService: OverviewPageService,
    panelService: PanelService,
    workspaceService: WorkspaceService
  ) {
    super(overviewService, panelService, workspaceService);
  }

  panelStopEdit(): Observable<boolean> {
    return of(true);
  }
}
