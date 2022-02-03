import { Component } from '@angular/core';
import { Organisation } from '@allors/default/workspace/domain';
import {
  RefreshService,
  SharedPullService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import {
  AllorsItemViewSummaryPanelComponent,
  ItemPageService,
  NavigationService,
  PanelService,
} from '@allors/base/workspace/angular/application';
import { IPullResult, Pull } from '@allors/system/workspace/domain';

@Component({
  selector: 'organisation-summary',
  templateUrl: './organisation-item-summary-panel.component.html',
})
export class OrganisationItemSummaryPanelComponent extends AllorsItemViewSummaryPanelComponent {
  organisation: Organisation;
  contactKindsText: string;

  constructor(
    itemPageService: ItemPageService,
    panelService: PanelService,
    sharedPullService: SharedPullService,
    workspaceService: WorkspaceService,
    refreshService: RefreshService,
    public navigation: NavigationService
  ) {
    super(
      itemPageService,
      panelService,
      sharedPullService,
      refreshService,
      workspaceService
    );
  }

  onPreSharedPull(pulls: Pull[], prefix?: string) {
    const {
      m: { pullBuilder: p },
    } = this;

    pulls.push(
      p.Organisation({
        name: prefix,
        objectId: this.itemPageInfo.id,
        include: {
          Country: {},
        },
      })
    );
  }

  onPostSharedPull(pullResult: IPullResult, prefix?: string) {
    this.organisation = pullResult.object<Organisation>(prefix);
  }
}
