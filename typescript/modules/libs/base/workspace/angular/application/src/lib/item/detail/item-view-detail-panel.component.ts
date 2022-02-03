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
export abstract class AllorsItemViewDetailPanelComponent extends AllorsItemPanelComponent {
  override dataAllorsKind = 'item-view-detail-panel';

  panelId = 'Detail';

  readonly panelMode = 'View';

  readonly panelKind = 'Detail';

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
