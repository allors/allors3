import { Component, Self } from '@angular/core';
import { M } from '@allors/default/workspace/meta';
import { Organisation } from '@allors/default/workspace/domain';
import {
  OnPullService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import {
  AllorsSummaryViewPanelComponent,
  NavigationService,
  OverviewPageService,
  PanelService,
} from '@allors/base/workspace/angular/application';
import { IPullResult, OnPull, Pull } from '@allors/system/workspace/domain';

@Component({
  selector: 'organisation-summary',
  templateUrl: './organisation-summary.component.html',
})
export class OrganisationSummaryComponent
  extends AllorsSummaryViewPanelComponent
  implements OnPull
{
  organisation: Organisation;
  contactKindsText: string;

  constructor(
    overviewService: OverviewPageService,
    panelService: PanelService,
    onPullService: OnPullService,
    workspaceService: WorkspaceService,
    public navigation: NavigationService
  ) {
    super(overviewService, panelService, workspaceService);

    panelService.register(this);
    onPullService.register(this);
  }

  onPrePull(pulls: Pull[], prefix?: string) {
    const {
      m: { pullBuilder: p },
    } = this;

    pulls.push(
      p.Organisation({
        name: prefix,
        objectId: this.overviewService.id,
        include: {
          Country: {},
        },
      })
    );
  }

  onPostPull(pullResult: IPullResult, prefix?: string) {
    this.organisation = pullResult.object<Organisation>(prefix);
  }
}
