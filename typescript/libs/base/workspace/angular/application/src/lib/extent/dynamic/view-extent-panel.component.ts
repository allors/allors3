import {
  SharedPullService,
  RefreshService,
  MetaService,
} from '@allors/base/workspace/angular/foundation';
import { Directive } from '@angular/core';
import { PanelService } from '../../panel/panel.service';
import { ScopedService } from '../../scoped/scoped.service';
import { AllorsDynamicExtentPanelComponent } from './extent-panel.component';

@Directive()
export abstract class AllorsDynamicViewExtentPanelComponent extends AllorsDynamicExtentPanelComponent {
  override dataAllorsKind = 'view-extent-panel';

  readonly panelMode = 'View';

  constructor(
    itemPageService: ScopedService,
    panelService: PanelService,
    onShareService: SharedPullService,
    refreshService: RefreshService,
    metaService: MetaService
  ) {
    super(
      itemPageService,
      panelService,
      onShareService,
      refreshService,
      metaService
    );
  }
}
