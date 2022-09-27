import { Component, Self } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import {
  RefreshService,
  SharedPullService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import {
  NavigationService,
  PanelService,
  ScopedService,
  AllorsOverviewPageComponent,
} from '@allors/base/workspace/angular/application';
import { IPullResult, Pull } from '@allors/system/workspace/domain';
import { AllorsMaterialPanelService } from '@allors/base/workspace/angular-material/application';
import { M } from '@allors/default/workspace/meta';
import { Proposal } from '@allors/default/workspace/domain';

@Component({
  templateUrl: './proposal-overview-page.component.html',
  providers: [
    ScopedService,
    {
      provide: PanelService,
      useClass: AllorsMaterialPanelService,
    },
  ],
})
export class ProposalOverviewPageComponent extends AllorsOverviewPageComponent {
  m: M;
  proposal: Proposal;
  canWrite: () => boolean;

  constructor(
    @Self() scopedService: ScopedService,
    @Self() panelService: PanelService,
    public navigation: NavigationService,
    sharedPullService: SharedPullService,
    refreshService: RefreshService,
    route: ActivatedRoute,
    workspaceService: WorkspaceService
  ) {
    super(
      scopedService,
      panelService,
      sharedPullService,
      refreshService,
      route,
      workspaceService
    );
    this.m = workspaceService.workspace.configuration.metaPopulation as M;
    this.canWrite = () => this.proposal.canWriteQuoteItems;
  }

  onPreSharedPull(pulls: Pull[], prefix?: string) {
    const {
      m: { pullBuilder: p },
    } = this;

    const id = this.scoped.id;

    pulls.push(
      p.Proposal({
        name: prefix,
        objectId: id,
        include: {
          QuoteItems: {
            Product: {},
            QuoteItemState: {},
          },
          Receiver: {},
          ContactPerson: {},
          QuoteState: {},
          CreatedBy: {},
          LastModifiedBy: {},
          Request: {},
          FullfillContactMechanism: {
            PostalAddress_Country: {},
          },
        },
      })
    );
  }

  onPostSharedPull(loaded: IPullResult, prefix?: string) {
    this.proposal = loaded.object<Proposal>(prefix);
  }
}
