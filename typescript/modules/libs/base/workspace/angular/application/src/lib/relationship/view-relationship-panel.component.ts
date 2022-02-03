import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import { Directive } from '@angular/core';
import { OverviewPageService } from '../overview/overview.service';
import { PanelService } from '../panel/panel.service';
import { AllorsRelationshipPanelComponent } from './relationship-panel.component';

@Directive()
export abstract class AllorsViewRelationshipPanelComponent extends AllorsRelationshipPanelComponent {
  dataAllorsKind = 'view-relationship-panel';

  readonly panelMode = 'View';

  constructor(
    overviewService: OverviewPageService,
    panelService: PanelService,
    workspaceService: WorkspaceService
  ) {
    super(overviewService, panelService, workspaceService);
  }
}
