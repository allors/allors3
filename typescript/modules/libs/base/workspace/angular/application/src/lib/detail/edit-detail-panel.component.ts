import { Directive } from '@angular/core';

import {
  RefreshService,
  SharedPullService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { AllorsObjectPanelComponent } from '../object/object-panel.component';
import { ObjectService } from '../object/object.service';
import { PanelService } from '../panel/panel.service';

@Directive()
export abstract class AllorsEditDetailPanelComponent extends AllorsObjectPanelComponent {
  override dataAllorsKind = 'edit-detail-panel';

  panelId = 'Detail';

  readonly panelMode = 'Edit';

  readonly panelKind = 'Detail';

  constructor(
    objectService: ObjectService,
    panelService: PanelService,
    sharedPullService: SharedPullService,
    refreshService: RefreshService,
    workspaceService: WorkspaceService
  ) {
    super(
      objectService,
      panelService,
      sharedPullService,
      refreshService,
      workspaceService
    );
  }
}
