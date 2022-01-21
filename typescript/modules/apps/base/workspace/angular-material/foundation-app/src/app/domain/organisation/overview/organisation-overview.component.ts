import { Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { Component, Self, OnDestroy, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { Organisation } from '@allors/default/workspace/domain';
import {
  NavigationService,
  RefreshService,
  PanelManagerService,
  NavigationActivatedRoute,
  AllorsOverviewComponent,
} from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';

@Component({
  templateUrl: './organisation-overview.component.html',
  providers: [PanelManagerService, ContextService],
})
export class OrganisationOverviewComponent
  extends AllorsOverviewComponent<Organisation>
  implements OnInit, OnDestroy
{
  subscription: Subscription;

  constructor(
    @Self() allors: ContextService,
    @Self() panelManager: PanelManagerService,
    titleService: Title,
    public refreshService: RefreshService,
    public navigation: NavigationService,
    private route: ActivatedRoute
  ) {
    super(allors, panelManager, titleService);
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;

    this.subscription = combineLatest([
      this.route.url,
      this.route.queryParams,
      this.refreshService.refresh$,
    ])
      .pipe(
        switchMap(([, ,]) => {
          const navRoute = new NavigationActivatedRoute(this.route);
          this.panelManager.objectType = m.Organisation;
          this.panelManager.id = navRoute.id();
          this.panelManager.expanded = navRoute.panel();

          this.panelManager.on();

          const pulls = [
            pull.Organisation({
              objectId: this.panelManager.id,
            }),
          ];

          this.panelManager.onPull(pulls);

          return this.panelManager.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.panelManager.context.reset();
        this.panelManager.onPulled(loaded);

        this.object = loaded.object<Organisation>(m.Organisation);
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
