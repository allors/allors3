import {
  SharedPullService,
  RefreshService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { PropertyType } from '@allors/system/workspace/meta';
import { Directive } from '@angular/core';
import { AllorsObjectPanelComponent } from '../object/object-panel.component';
import { ObjectService } from '../object/object.service';
import { PanelService } from '../panel/panel.service';
import { RelationPanel } from './relation-panel';

@Directive()
export abstract class AllorsEditRelationPanelComponent
  extends AllorsObjectPanelComponent
  implements RelationPanel
{
  override dataAllorsKind = 'edit-relation-panel';

  readonly panelMode = 'Edit';

  readonly panelKind = 'Relation';

  abstract propertyType: PropertyType;

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
