import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import { Directive } from '@angular/core';
import { PanelManagerService } from '../panel/panel-manager.service';
import { AllorsRelationshipPanelComponent } from './relationship-panel.component';

@Directive()
export abstract class AllorsRelationshipViewPanelComponent extends AllorsRelationshipPanelComponent {
  dataAllorsKind = 'rel-view-panel';

  constructor(
    panelManagerService: PanelManagerService,
    workspaceService: WorkspaceService
  ) {
    super(panelManagerService, workspaceService);
  }
}
