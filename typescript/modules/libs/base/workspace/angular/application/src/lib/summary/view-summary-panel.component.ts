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
export abstract class AllorsViewSummaryPanelComponent extends AllorsScopedPanelComponent {
  override dataAllorsKind = 'view-summary-panel';

  panelId = 'ItemSummary';

  readonly panelKind = 'Summary';

  readonly panelMode = 'View';

  constructor(
    itemPageService: ScopedService,
    panelService: PanelService,
    sharedPullService: SharedPullService,
    refreshService: RefreshService
  ) {
    super(itemPageService, panelService, sharedPullService, refreshService);
  }

  toggle() {
    if (this.panelService.activePanel) {
      this.panelService.stopEdit();
    } else {
      this.panelService.startEdit('Detail');
    }
  }
}
