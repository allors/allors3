import { Directive } from '@angular/core';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import { AllorsOverviewOnEditPanelComponent } from '../overview-panel-onedit.component';
import { OverviewPageService } from '../overview-page.service';
import { PanelService } from '../../panel/panel.service';

@Directive()
export abstract class AllorsItemEditDetailPanelComponent extends AllorsOverviewOnEditPanelComponent {
  dataAllorsKind = 'edit-detail-panel';

  panelId = 'Detail';

  readonly panelMode = 'Edit';

  constructor(
    overviewService: OverviewPageService,
    panelService: PanelService,
    workspaceService: WorkspaceService
  ) {
    super(overviewService, panelService, workspaceService);
  }
}
