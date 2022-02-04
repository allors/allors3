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
export abstract class AllorsViewRelationshipPanelComponent
  extends AllorsObjectPanelComponent
  implements RelationshipPanel
{
  override dataAllorsKind = 'view-relationship-panel';

  readonly panelMode = 'View';

  readonly panelKind = 'Relationship';

  abstract anchor: RoleType;

  abstract target: RoleType;

  constructor(
    itemPageService: ObjectService,
    panelService: PanelService,
    onShareService: SharedPullService,
    refreshService: RefreshService,
    workspaceService: WorkspaceService
  ) {
    super(
      itemPageService,
      panelService,
      onShareService,
      refreshService,
      workspaceService
    );
  }
}
