import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import { Directive } from '@angular/core';
import { PanelManagerService } from '../panel/panel-manager.service';
import { AllorsRelationshipPanelComponent } from './relationship-panel.component';

@Directive()
export abstract class AllorsRelationshipEditPanelComponent extends AllorsRelationshipPanelComponent {
  dataAllorsKind = 'rel-edit-panel';

  constructor(
    panelMangerService: PanelManagerService,
    workspaceService: WorkspaceService
  ) {
    super(panelMangerService, workspaceService);
  }
}
