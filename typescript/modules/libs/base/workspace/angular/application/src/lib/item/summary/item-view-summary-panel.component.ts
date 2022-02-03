import {
  SharedPullService,
  RefreshService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { Directive } from '@angular/core';
import { PanelService } from '../../panel/panel.service';
import { ItemPageService } from '../item-page.service';
import { AllorsItemPanelComponent } from '../item-panel.component';

@Directive()
export abstract class AllorsItemViewSummaryPanelComponent extends AllorsItemPanelComponent {
  override dataAllorsKind = 'item-view-summary-panel';

  panelId = 'ItemSummary';

  readonly panelKind = 'Summary';

  readonly panelMode = 'View';

  constructor(
    itemPageService: ItemPageService,
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
