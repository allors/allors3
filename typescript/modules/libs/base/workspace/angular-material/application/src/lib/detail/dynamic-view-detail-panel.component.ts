import { Component } from '@angular/core';
import {
  AllorsViewDetailPanelComponent,
  OverviewPageService,
  PanelService,
} from '@allors/base/workspace/angular/application';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';

@Component({
  selector: 'a-mat-dyn-view-detail-panel',
  templateUrl: './dynamic-view-detail-panel.component.html',
})
export class AllorsMaterialDynamicViewDetailPanelComponent extends AllorsViewDetailPanelComponent {
  constructor(
    overviewService: OverviewPageService,
    panelService: PanelService,
    workspaceService: WorkspaceService
  ) {
    super(overviewService, panelService, workspaceService);
  }
}
