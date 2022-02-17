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
export abstract class AllorsEditRelationPanelComponent
  extends AllorsScopedPanelComponent
  implements RelationPanel
{
  override dataAllorsKind = 'edit-relation-panel';

  readonly panelMode = 'Edit';

  readonly panelKind = 'Relation';

  abstract propertyType: PropertyType;

  constructor(
    itemPageService: ScopedService,
    panelService: PanelService,
    sharedPullService: SharedPullService,
    refreshService: RefreshService
  ) {
    super(itemPageService, panelService, sharedPullService, refreshService);
  }
}
