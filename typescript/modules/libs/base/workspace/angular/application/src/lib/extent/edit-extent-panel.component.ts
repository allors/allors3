import {
  SharedPullService,
  RefreshService,
  MetaService,
} from '@allors/base/workspace/angular/foundation';
import { Directive, Input } from '@angular/core';
import { ScopedService } from '../scoped/scoped.service';
import { PanelService } from '../panel/panel.service';
import { AllorsExtentPanelComponent } from './extent-panel.component';
import { Composite } from '@allors/system/workspace/meta';

@Directive()
export abstract class AllorsEditExtentPanelComponent extends AllorsExtentPanelComponent {
  override dataAllorsKind = 'edit-extent-panel';

  readonly panelMode = 'Edit';

  @Input()
  factory: Composite;

  constructor(
    itemPageService: ScopedService,
    panelService: PanelService,
    sharedPullService: SharedPullService,
    refreshService: RefreshService,
    metaService: MetaService
  ) {
    super(
      itemPageService,
      panelService,
      sharedPullService,
      refreshService,
      metaService
    );
  }
}