import { Component, Self, AfterViewInit, OnDestroy, Injector } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { Good, UnifiedGood } from '@allors/workspace/domain/default';
import { NavigationActivatedRoute, NavigationService, PanelManagerService, RefreshService, TestScope } from '@allors/workspace/angular/base';
import { SessionService, WorkspaceService } from '@allors/workspace/angular/core';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { M } from '@allors/workspace/meta/default';

@Component({
  templateUrl: './unifiedgood-overview.component.html',
  providers: [PanelManagerService, SessionService],
})
export class UnifiedGoodOverviewComponent extends TestScope implements AfterViewInit, OnDestroy {
  title = 'Good';

  good: Good;

  subscription: Subscription;
  serialised: boolean;
  m: M;

  constructor(
    @Self() public panelManager: PanelManagerService,
    public workspaceService: WorkspaceService,
    public refreshService: RefreshService,
    public navigation: NavigationService,
    private route: ActivatedRoute,
    private internalOrganisationId: InternalOrganisationId,
    public injector: Injector,
    titleService: Title
  ) {
    super();

    titleService.setTitle(this.title);

    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;
  }

  public ngAfterViewInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest(this.route.url, this.route.queryParams, this.refreshService.refresh$, this.internalOrganisationId.observable$)
      .pipe(
        switchMap(() => {
          const navRoute = new NavigationActivatedRoute(this.route);
          this.panelManager.id = navRoute.id();
          this.panelManager.objectType = m.UnifiedGood;
          this.panelManager.expanded = navRoute.panel();

          this.panelManager.on();

          const pulls = [
            pull.UnifiedGood({
              objectId: this.panelManager.id,
              include: {
                InventoryItemKind: x,
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

        const unifiedGood = loaded.object<UnifiedGood>(m.UnifiedGood);
        this.serialised = unifiedGood.InventoryItemKind.UniqueId === '2596e2dd-3f5d-4588-a4a2-167d6fbe3fae';
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
