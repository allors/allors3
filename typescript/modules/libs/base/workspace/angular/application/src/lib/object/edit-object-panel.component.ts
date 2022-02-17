import {
  SharedPullService,
  RefreshService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { RoleType } from '@allors/system/workspace/meta';
import { Directive } from '@angular/core';
import { AllorsScopedPanelComponent } from '../scoped/scoped-panel.component';
import { ScopedService } from '../scoped/scoped.service';
import { PanelService } from '../panel/panel.service';
import { ObjectPanel } from './object-panel';
import { Path } from '@allors/system/workspace/domain';

@Directive()
export abstract class AllorsEditObjectPanelComponent
  extends AllorsScopedPanelComponent
  implements ObjectPanel
{
  override dataAllorsKind = 'edit-object-panel';

  readonly panelMode = 'Edit';

  readonly panelKind = 'Object';

  abstract anchor: RoleType;

  abstract target: Path;

  constructor(
    itemPageService: ScopedService,
    panelService: PanelService,
    sharedPullService: SharedPullService,
    refreshService: RefreshService
  ) {
    super(itemPageService, panelService, sharedPullService, refreshService);
  }
}
