import {
  SharedPullService,
  RefreshService,
} from '@allors/base/workspace/angular/foundation';
import { Directive } from '@angular/core';
import { ScopedService } from '../scoped/scoped.service';
import { PanelService } from '../panel/panel.service';
import { AllorsExtentPanelComponent } from './extent-panel.component';

@Directive()
export abstract class AllorsEditExtentPanelComponent extends AllorsExtentPanelComponent {
  override dataAllorsKind = 'edit-extent-panel';

  readonly panelMode = 'Edit';

  constructor(
    itemPageService: ScopedService,
    panelService: PanelService,
    sharedPullService: SharedPullService,
    refreshService: RefreshService
  ) {
    super(itemPageService, panelService, sharedPullService, refreshService);
  }
}
