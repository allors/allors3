import { Component, Self, AfterViewInit, OnDestroy, Injector } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { Good, NonUnifiedGood } from '@allors/workspace/domain/default';
import { NavigationActivatedRoute, NavigationService, PanelManagerService, RefreshService, TestScope } from '@allors/workspace/angular/base';
import { SessionService, WorkspaceService } from '@allors/workspace/angular/core';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './nonunifiedgood-overview.component.html',
  providers: [PanelManagerService, SessionService],
})
export class NonUnifiedGoodOverviewComponent extends TestScope implements AfterViewInit, OnDestroy {
  title = 'Good';

  good: Good;

  subscription: Subscription;
  m: M;

  constructor(
    @Self() public panelManager: PanelManagerService,
    public workspaceService: WorkspaceService,
    public refreshService: RefreshService,
    public navigation: NavigationService,
    private route: ActivatedRoute,
    public injector: Injector,
    private internalOrganisationId: InternalOrganisationId,
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
        switchMap(([, ,]) => {
          const navRoute = new NavigationActivatedRoute(this.route);
          this.panelManager.objectType = m.NonUnifiedGood;
          this.panelManager.id = navRoute.id();
          this.panelManager.expanded = navRoute.panel();

          this.panelManager.on();

          const pulls = [
            pull.NonUnifiedGood({
              objectId: this.panelManager.id,
              include: {
                ProductIdentifications: {
                  ProductIdentificationType: x,
                },
                Part: {
                  Brand: x,
                  Model: x,
                },
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

        this.good = loaded.object<NonUnifiedGood>(m.NonUnifiedGood);
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
