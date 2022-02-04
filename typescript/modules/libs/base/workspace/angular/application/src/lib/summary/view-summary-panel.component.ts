import {
  SharedPullService,
  RefreshService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { Directive } from '@angular/core';
import { AllorsObjectPanelComponent } from '../object/object-panel.component';
import { ObjectService } from '../object/object.service';
import { PanelService } from '../panel/panel.service';

@Directive()
export abstract class AllorsViewSummaryPanelComponent extends AllorsObjectPanelComponent {
  override dataAllorsKind = 'view-summary-panel';

  panelId = 'ItemSummary';

  readonly panelKind = 'Summary';

  readonly panelMode = 'View';

  constructor(
    itemPageService: ObjectService,
    panelService: PanelService,
    sharedPullService: SharedPullService,
    refreshService: RefreshService,
    workspaceService: WorkspaceService
  ) {
    super(
      itemPageService,
      panelService,
      sharedPullService,
      refreshService,
      workspaceService
    );
  }
}
