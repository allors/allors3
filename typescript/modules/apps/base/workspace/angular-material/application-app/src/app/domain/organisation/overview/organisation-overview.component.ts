import { combineLatest, delay, Subscription, switchMap, tap } from 'rxjs';
import { Component, Self, OnDestroy, AfterViewInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { Organisation } from '@allors/default/workspace/domain';
import {
  OnPullService,
  RefreshService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import {
  AllorsOverviewPageComponent,
  NavigationService,
  NavigationActivatedRoute,
  OverviewPageService,
  PanelService,
} from '@allors/base/workspace/angular/application';
import { IPullResult, OnPull, Pull } from '@allors/system/workspace/domain';
import { AllorsMaterialPanelService } from '@allors/base/workspace/angular-material/application';

@Component({
  templateUrl: './organisation-overview.component.html',
  providers: [
    OverviewPageService,
    {
      provide: PanelService,
      useClass: AllorsMaterialPanelService,
    },
  ],
})
export class OrganisationOverviewComponent
  extends AllorsOverviewPageComponent
  implements OnPull, OnDestroy
{
  object: Organisation;

  private subscription: Subscription;

  constructor(
    @Self() overviewService: OverviewPageService,
    @Self() panelService: PanelService,
    public navigation: NavigationService,
    titleService: Title,
    refreshService: RefreshService,
    route: ActivatedRoute,
    onPullService: OnPullService,
    workspaceService: WorkspaceService
  ) {
    super(overviewService, workspaceService);

    onPullService.register(this);

    this.subscription = combineLatest([route.url, route.queryParams])
      .pipe(
        switchMap(() => {
          const navRoute = new NavigationActivatedRoute(route);
          this.overviewService.objectType = this.m.Organisation;
          this.overviewService.id = navRoute.id();

          const title = this.overviewService.objectType.singularName;
          titleService.setTitle(title);

          return panelService.startEdit(navRoute.panel());
        }),
        tap(() => refreshService.refresh())
      )
      .subscribe();
  }

  ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  onPrePull(pulls: Pull[], prefix: string) {
    const {
      m: { pullBuilder: p },
    } = this;

    pulls.push(
      p.Organisation({
        name: prefix,
        objectId: this.overviewService.id,
      })
    );
  }

  onPostPull(pullResult: IPullResult, prefix: string) {
    this.object = pullResult.object(prefix);
  }
}
