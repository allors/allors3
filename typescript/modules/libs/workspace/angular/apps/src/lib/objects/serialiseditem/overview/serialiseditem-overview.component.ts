import { Component, Self, AfterViewInit, OnDestroy, Injector } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { ActivatedRoute } from '@angular/router';

import { M } from '@allors/workspace/meta/default';
import { Party, Part, SerialisedItem } from '@allors/workspace/domain/default';
import { NavigationActivatedRoute, NavigationService, PanelManagerService, RefreshService, TestScope } from '@allors/workspace/angular/base';
import { SessionService, WorkspaceService } from '@allors/workspace/angular/core';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './serialiseditem-overview.component.html',
  providers: [PanelManagerService, SessionService],
})
export class SerialisedItemOverviewComponent extends TestScope implements AfterViewInit, OnDestroy {
  readonly m: M;
  title = 'Asset';

  serialisedItem: SerialisedItem;

  subscription: Subscription;
  part: Part;
  owner: Party;

  constructor(
    @Self() public panelManager: PanelManagerService,
    public workspaceService: WorkspaceService,

    public refreshService: RefreshService,
    public navigationService: NavigationService,
    private route: ActivatedRoute,
    public injector: Injector,
    private internalOrganisationId: InternalOrganisationId,
    titleService: Title
  ) {
    super();

    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;
    titleService.setTitle(this.title);
  }

  public ngAfterViewInit(): void {
    this.subscription = combineLatest(this.route.url, this.route.queryParams, this.refreshService.refresh$, this.internalOrganisationId.observable$)
      .pipe(
        switchMap(() => {
          const m = this.allors.workspace.configuration.metaPopulation as M;
          const { pullBuilder: pull } = m;
          const x = {};

          const navRoute = new NavigationActivatedRoute(this.route);
          this.panelManager.objectType = m.SerialisedItem;
          this.panelManager.id = navRoute.id();
          this.panelManager.expanded = navRoute.panel();

          this.panelManager.on();

          const pulls = [
            pull.SerialisedItem({
              objectId: this.panelManager.id,
              include: {
                OwnedBy: x,
              },
            }),
          ];

          this.panelManager.onPull(pulls);

          return this.panelManager.client.pullReactive(this.panelManager.session, pulls);
        })
      )
      .subscribe((loaded) => {
        this.panelManager.session.reset();
        this.panelManager.onPulled(loaded);

        this.serialisedItem = loaded.object<SerialisedItem>(m.SerialisedItem);
        this.owner = this.serialisedItem.OwnedBy;
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
