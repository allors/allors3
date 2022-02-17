import { Directive } from '@angular/core';

import {
  RefreshService,
  SharedPullService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { AllorsScopedPanelComponent } from '../scoped/scoped-panel.component';
import { ScopedService } from '../scoped/scoped.service';
import { PanelService } from '../panel/panel.service';

@Directive()
export abstract class AllorsEditDetailPanelComponent extends AllorsScopedPanelComponent {
  override dataAllorsKind = 'edit-detail-panel';

  panelId = 'Detail';

  readonly panelMode = 'Edit';

  readonly panelKind = 'Detail';

  constructor(
    scopedService: ScopedService,
    panelService: PanelService,
    sharedPullService: SharedPullService,
    refreshService: RefreshService
  ) {
    super(scopedService, panelService, sharedPullService, refreshService);
  }
}
