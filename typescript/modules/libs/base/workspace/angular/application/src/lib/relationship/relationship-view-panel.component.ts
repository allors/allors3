import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import { Directive } from '@angular/core';
import { OverviewPageService } from '../overview/overview.service';
import { PanelMode } from '../panel/panel';
import { PanelService } from '../panel/panel-manager.service';
import { AllorsRelationshipPanelComponent } from './relationship-panel.component';

@Directive()
export abstract class AllorsRelationshipViewPanelComponent extends AllorsRelationshipPanelComponent {
  dataAllorsKind = 'rel-view-panel';

  panelMode: PanelMode = 'View';

  constructor(
    overviewService: OverviewPageService,
    panelService: PanelService,
    workspaceService: WorkspaceService
  ) {
    super(overviewService, panelService, workspaceService);
  }
}
