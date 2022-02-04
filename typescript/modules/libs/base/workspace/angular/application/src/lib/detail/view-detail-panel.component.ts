import {
  SharedPullService,
  RefreshService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { Directive } from '@angular/core';
import { AllorsObjectPanelComponent } from '../object/object-panel.component';
import { ObjectService } from '../object/object.service';
import { PanelService } from '../panel/panel.service';

@Directive()
export abstract class AllorsViewDetailPanelComponent extends AllorsObjectPanelComponent {
  override dataAllorsKind = 'view-detail-panel';

  panelId = 'Detail';

  readonly panelMode = 'View';

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
