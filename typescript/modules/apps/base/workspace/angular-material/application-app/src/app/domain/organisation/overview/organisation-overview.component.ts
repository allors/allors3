import { combineLatest, Subscription, switchMap } from 'rxjs';
import { Component, Self, OnDestroy, AfterViewInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { Organisation } from '@allors/default/workspace/domain';
import {
  ContextService,
  OnPullService,
  RefreshService,
} from '@allors/base/workspace/angular/foundation';
import {
  NavigationService,
  AllorsEditPageComponent,
  PanelManagerService,
} from '@allors/base/workspace/angular/application';

@Component({
  templateUrl: './organisation-overview.component.html',
  providers: [ContextService, PanelManagerService, OnPullService],
})
export class OrganisationOverviewComponent
  extends AllorsEditPageComponent<Organisation>
  implements AfterViewInit, OnDestroy
{
  subscription: Subscription;

  constructor(
    @Self() allors: ContextService,
    @Self() panelManagerService: PanelManagerService,
    @Self() onPullService: OnPullService,
    titleService: Title,
    public refreshService: RefreshService,
    public navigation: NavigationService,
    private route: ActivatedRoute
  ) {
    super(allors, panelManagerService, titleService);

    // this.subscription = combineLatest([
    //   this.route.url,
    //   this.route.queryParams,
    //   this.refreshService.refresh$,
    // ])
    //   .pipe(
    //     switchMap(([, ,]) => {
    //       const navRoute = new NavigationActivatedRoute(this.route);
    //       this.panelService.objectType = m.Organisation;
    //       this.panelService.id = navRoute.id();
    //       this.panelService.expanded = navRoute.panel();

    //       this.panelManager.on();

    //       const pulls = [
    //         pull.Organisation({
    //           objectId: this.panelManager.id,
    //         }),
    //       ];

    //       this.panelManager.onPull(pulls);

    //       return this.panelManager.context.pull(pulls);
    //     })
    //   )
    //   .subscribe((loaded) => {
    //     this.panelManager.context.reset();
    //     this.panelManager.onPulled(loaded);

    //     this.object = loaded.object<Organisation>(m.Organisation);
    //   });
  }

  public ngAfterViewInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;

    // this.subscription = combineLatest([
    //   this.route.url,
    //   this.route.queryParams,
    //   this.refreshService.refresh$,
    // ])
    //   .pipe(
    //     switchMap(([, ,]) => {
    //       const navRoute = new NavigationActivatedRoute(this.route);
    //       this.panelManager.objectType = m.Organisation;
    //       this.panelManager.id = navRoute.id();
    //       this.panelManager.expanded = navRoute.panel();

    //       this.panelManager.on();

    //       const pulls = [
    //         pull.Organisation({
    //           objectId: this.panelManager.id,
    //         }),
    //       ];

    //       this.panelManager.onPull(pulls);

    //       return this.panelManager.context.pull(pulls);
    //     })
    //   )
    //   .subscribe((loaded) => {
    //     this.panelManager.context.reset();
    //     this.panelManager.onPulled(loaded);

    //     this.object = loaded.object<Organisation>(m.Organisation);
    //   });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
