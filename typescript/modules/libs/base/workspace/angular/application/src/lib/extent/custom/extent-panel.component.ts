import { Directive, Input } from '@angular/core';

import {
  RefreshService,
  SharedPullService,
} from '@allors/base/workspace/angular/foundation';
import { AllorsScopedPanelComponent } from '../../scoped/scoped-panel.component';
import { ScopedService } from '../../scoped/scoped.service';
import { PanelService } from '../../panel/panel.service';

@Directive()
export abstract class AllorsCustomExtentPanelComponent extends AllorsScopedPanelComponent {
  readonly panelKind = 'Extent';

  @Input()
  enabler: () => boolean;

  @Input()
  enabled: boolean;

  abstract override panelId: string;

  constructor(
    itemPageService: ScopedService,
    panelService: PanelService,
    sharedPullService: SharedPullService,
    refreshService: RefreshService
  ) {
    super(itemPageService, panelService, sharedPullService, refreshService);
  }
}
