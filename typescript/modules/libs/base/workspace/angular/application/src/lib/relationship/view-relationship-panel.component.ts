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
import { RelationshipPanel } from './relationship-panel';

@Directive()
export abstract class AllorsViewRelationshipPanelComponent
  extends AllorsScopedPanelComponent
  implements RelationshipPanel
{
  override dataAllorsKind = 'view-relationship-panel';

  readonly panelMode = 'View';

  readonly panelKind = 'Relationship';

  abstract anchor: RoleType;

  abstract target: RoleType;

  constructor(
    itemPageService: ScopedService,
    panelService: PanelService,
    onShareService: SharedPullService,
    refreshService: RefreshService
  ) {
    super(itemPageService, panelService, onShareService, refreshService);
  }
}
