import { Component } from '@angular/core';
import { Organisation } from '@allors/default/workspace/domain';
import {
  RefreshService,
  SharedPullService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import {
  AllorsViewSummaryPanelComponent,
  ObjectService,
  NavigationService,
  PanelService,
} from '@allors/base/workspace/angular/application';
import { IPullResult, Pull } from '@allors/system/workspace/domain';

@Component({
  selector: 'organisation-summary-panel',
  templateUrl: './organisation-summary-panel.component.html',
})
export class OrganisationSummaryPanelComponent extends AllorsViewSummaryPanelComponent {
  organisation: Organisation;
  contactKindsText: string;

  constructor(
    objectService: ObjectService,
    panelService: PanelService,
    sharedPullService: SharedPullService,
    workspaceService: WorkspaceService,
    refreshService: RefreshService,
    public navigation: NavigationService
  ) {
    super(
      objectService,
      panelService,
      sharedPullService,
      refreshService,
      workspaceService
    );
  }

  onPreSharedPull(pulls: Pull[], scope?: string) {
    const {
      m: { pullBuilder: p },
    } = this;

    pulls.push(
      p.Organisation({
        name: scope,
        objectId: this.objectInfo.id,
        include: {
          Country: {},
        },
      })
    );
  }

  onPostSharedPull(pullResult: IPullResult, scope?: string) {
    this.organisation = pullResult.object<Organisation>(scope);
  }
}
