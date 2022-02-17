import {
  SharedPullService,
  RefreshService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { PropertyType } from '@allors/system/workspace/meta';
import { Directive } from '@angular/core';
import { AllorsScopedPanelComponent } from '../scoped/scoped-panel.component';
import { ScopedService } from '../scoped/scoped.service';
import { PanelService } from '../panel/panel.service';
import { RelationPanel } from './relation-panel';

@Directive()
export abstract class AllorsViewRelationPanelComponent
  extends AllorsScopedPanelComponent
  implements RelationPanel
{
  override dataAllorsKind = 'view-relation-panel';

  readonly panelMode = 'View';

  readonly panelKind = 'Relation';

  abstract propertyType: PropertyType;

  constructor(
    itemPageService: ScopedService,
    panelService: PanelService,
    onShareService: SharedPullService,
    refreshService: RefreshService
  ) {
    super(itemPageService, panelService, onShareService, refreshService);
  }
}
