import { Observable, of } from 'rxjs';
import { Directive } from '@angular/core';
import {
  EditBlocking,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { OverviewPageService } from '../overview/overview.service';
import { PanelService } from '../panel/panel.service';
import { AllorsDetailPanelComponent } from './detail-panel.component';

@Directive()
export abstract class AllorsEditDetailPanelComponent
  extends AllorsDetailPanelComponent
  implements EditBlocking
{
  dataAllorsKind = 'edit-detail-panel';

  panelId = 'Detail';

  readonly panelMode = 'Edit';

  constructor(
    overviewService: OverviewPageService,
    panelService: PanelService,
    workspaceService: WorkspaceService
  ) {
    super(overviewService, panelService, workspaceService);
  }

  stopEdit(): Observable<boolean> {
    return of(true);
  }
}
