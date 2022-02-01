import { Component } from '@angular/core';
import {
  PanelService,
  AllorsEditDetailPanelComponent,
  OverviewPageService,
} from '@allors/base/workspace/angular/application';
import { RoleType } from '@allors/system/workspace/meta';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';

@Component({
  selector: 'a-mat-dyn-detail-panel',
  templateUrl: './dynamic-edit-detail-panel.component.html',
})
export class AllorsMaterialDynamicEditDetailPanelComponent extends AllorsEditDetailPanelComponent {
  anchor: RoleType;
  target: RoleType;

  constructor(
    overviewService: OverviewPageService,
    panelService: PanelService,
    workspaceService: WorkspaceService
  ) {
    super(overviewService, panelService, workspaceService);
  }
}
