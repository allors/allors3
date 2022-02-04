import {
  SharedPullService,
  RefreshService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { RoleType } from '@allors/system/workspace/meta';
import { Directive } from '@angular/core';
import { AllorsObjectPanelComponent } from '../object/object-panel.component';
import { ObjectService } from '../object/object.service';
import { PanelService } from '../panel/panel.service';
import { RelationshipPanel } from './relationship-panel';

@Directive()
export abstract class AllorsEditRelationshipPanelComponent
  extends AllorsObjectPanelComponent
  implements RelationshipPanel
{
  override dataAllorsKind = 'edit-relationship-panel';

  readonly panelMode = 'Edit';

  readonly panelKind = 'Relationship';

  abstract anchor: RoleType;

  abstract target: RoleType;

  constructor(
    itemPageService: ObjectService,
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
