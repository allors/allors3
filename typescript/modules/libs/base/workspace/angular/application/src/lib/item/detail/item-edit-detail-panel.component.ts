import { Directive } from '@angular/core';

import { ItemPageService } from '../item-page.service';
import { PanelService } from '../../panel/panel.service';
import { AllorsItemPanelComponent } from '../item-panel.component';
import {
  RefreshService,
  SharedPullService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';

@Directive()
export abstract class AllorsItemEditDetailPanelComponent extends AllorsItemPanelComponent {
  override dataAllorsKind = 'item-edit-detail-panel';

  panelId = 'ItemDetail';

  readonly panelMode = 'Edit';

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
