import { Directive } from '@angular/core';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import { AllorsOverviewComponent } from './overview.component';

@Directive()
export abstract class AllorsOverviewPageComponent extends AllorsOverviewComponent {
  override dataAllorsKind = 'overview';

  constructor(workspaceService: WorkspaceService) {
    super(workspaceService);
  }
}
