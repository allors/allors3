import {
  SharedPullService,
  RefreshService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { Directive } from '@angular/core';
import { AllorsScopedPanelComponent } from '../scoped/scoped-panel.component';
import { ScopedService } from '../scoped/scoped.service';
import { PanelService } from '../panel/panel.service';

@Directive()
export abstract class AllorsViewDetailPanelComponent extends AllorsScopedPanelComponent {
  override dataAllorsKind = 'view-detail-panel';

  panelId = 'Detail';

  readonly panelMode = 'View';

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
