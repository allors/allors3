import { Subscription, combineLatest, tap } from 'rxjs';
import { Component, Self, OnDestroy } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { Person } from '@allors/default/workspace/domain';
import {
  OnPullService,
  RefreshService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import {
  AllorsOverviewPageComponent,
  NavigationActivatedRoute,
  NavigationService,
  OverviewPageService,
  PanelService,
} from '@allors/base/workspace/angular/application';
import { IPullResult, OnPull, Pull } from '@allors/system/workspace/domain';

@Component({
  templateUrl: './person-overview.component.html',
  providers: [OverviewPageService, PanelService],
})
export class PersonOverviewComponent
  extends AllorsOverviewPageComponent
  implements OnPull, OnDestroy
{
  object: Person;

  private subscription: Subscription;

  constructor(
    @Self() overviewService: OverviewPageService,
    @Self() panelService: PanelService,
    public navigation: NavigationService,
    onPullService: OnPullService,
    titleService: Title,
    refreshService: RefreshService,
    route: ActivatedRoute,
    workspaceService: WorkspaceService
  ) {
    super(overviewService, workspaceService);

    onPullService.register(this);

    this.subscription = combineLatest([route.url, route.queryParams])
      .pipe(
        tap(() => {
          const navRoute = new NavigationActivatedRoute(route);
          this.overviewService.objectType = this.m.Organisation;
          this.overviewService.id = navRoute.id();

          const panel = panelService.get(navRoute.panel(), 'Edit');
          panelService.select(panel);

          const title = this.overviewService.objectType.singularName;
          titleService.setTitle(title);

          refreshService.refresh();
        })
      )
      .subscribe();
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

  ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
