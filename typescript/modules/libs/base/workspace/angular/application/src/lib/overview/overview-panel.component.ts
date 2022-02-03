import { Directive } from '@angular/core';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import { Panel, PanelKind, PanelMode } from '../panel/panel';
import { AllorsOverviewComponent } from './overview.component';

@Directive()
export abstract class AllorsOverviewPanelComponent
  extends AllorsOverviewComponent
  implements Panel
{
  abstract panelId: string;

  abstract panelMode: PanelMode;

  abstract panelKind: PanelKind;

  panelEnabled: boolean;

  constructor(public workspaceService: WorkspaceService) {
    super(workspaceService);
  }
}
