import { AfterViewInit, Component, OnDestroy } from '@angular/core';
import { Organisation } from '@allors/default/workspace/domain';
import {
  OnPullService,
  RefreshService,
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
  implements OnPull, OnDestroy, AfterViewInit
{
  organisation: Organisation;
  contactKindsText: string;

  constructor(
    overviewService: OverviewPageService,
    panelService: PanelService,
    onPullService: OnPullService,
    workspaceService: WorkspaceService,
    refreshService: RefreshService,
    public navigation: NavigationService
  ) {
    super(
      overviewService,
      panelService,
      onPullService,
      refreshService,
      workspaceService
    );
  }

  onPrePull(pulls: Pull[], prefix?: string) {
    const {
      m: { pullBuilder: p },
    } = this;

    pulls.push(
      p.Organisation({
        name: prefix,
        objectId: this.overviewPageInfo.id,
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
