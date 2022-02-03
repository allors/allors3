import {
  SharedPullService,
  RefreshService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { RoleType } from '@allors/system/workspace/meta';
import { Directive } from '@angular/core';
import { PanelService } from '../../panel/panel.service';
import { ItemPageService } from '../item-page.service';
import { AllorsItemPanelComponent } from '../item-panel.component';
import { RelationshipPanel } from './relationship-panel';

@Directive()
export abstract class AllorsItemEditRelationshipPanelComponent
  extends AllorsItemPanelComponent
  implements RelationshipPanel
{
  override dataAllorsKind = 'item-edit-relationship-panel';

  readonly panelMode = 'Edit';

  readonly panelKind = 'Relationship';

  abstract anchor: RoleType;

  abstract target: RoleType;

  constructor(
    itemPageService: ItemPageService,
    panelService: PanelService,
    sharedPullService: SharedPullService,
    refreshService: RefreshService,
    workspaceService: WorkspaceService
  ) {
    super(
      itemPageService,
      panelService,
      sharedPullService,
      refreshService,
      workspaceService
    );
  }
}
