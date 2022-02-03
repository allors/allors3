import {
  OnPullService,
  RefreshService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { Directive } from '@angular/core';
import { PanelService } from '../../panel/panel.service';
import { OverviewPageService } from '../overview-page.service';

import { AllorsRelationshipPanelComponent } from './relationship-panel.component';

@Directive()
export abstract class AllorsViewRelationshipPanelComponent extends AllorsRelationshipPanelComponent {
  dataAllorsKind = 'view-relationship-panel';

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
