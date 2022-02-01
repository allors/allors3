import { Observable, of } from 'rxjs';
import { Directive } from '@angular/core';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import { OverviewPageService } from '../overview/overview.service';
import { EditPanel } from '../panel/panel';
import { PanelService } from '../panel/panel.service';
import { AllorsDetailPanelComponent } from './detail-panel.component';

@Directive()
export abstract class AllorsEditDetailPanelComponent
  extends AllorsDetailPanelComponent
  implements EditPanel
{
  dataAllorsKind = 'edit-detail-panel';

  panelId = 'EditDetail';

  panelMode: 'Edit' = 'Edit';

  constructor(
    overviewService: OverviewPageService,
    panelService: PanelService,
    workspaceService: WorkspaceService
  ) {
    super(overviewService, panelService, workspaceService);
  }

  panelStopEdit(): Observable<boolean> {
    return of(true);
  }
}
