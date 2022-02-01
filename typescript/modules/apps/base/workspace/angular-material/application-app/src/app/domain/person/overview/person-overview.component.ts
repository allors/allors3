import { Subscription, combineLatest, tap, switchMap } from 'rxjs';
import { Component, Self, OnDestroy, AfterViewInit } from '@angular/core';
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
import { AllorsMaterialPanelService } from '@allors/base/workspace/angular-material/application';

@Component({
  templateUrl: './person-overview.component.html',
  providers: [
    OverviewPageService,
    {
      provide: PanelService,
      useClass: AllorsMaterialPanelService,
    },
  ],
})
export class PersonOverviewComponent
  extends AllorsOverviewPageComponent
  implements OnPull, AfterViewInit, OnDestroy
{
  object: Person;

  private subscription: Subscription;

  constructor(
    @Self() overviewService: OverviewPageService,
    @Self() private panelService: PanelService,
    public navigation: NavigationService,
    private titleService: Title,
    private refreshService: RefreshService,
    private route: ActivatedRoute,
    onPullService: OnPullService,
    workspaceService: WorkspaceService
  ) {
    super(overviewService, workspaceService);

    onPullService.register(this);
  }

  ngAfterViewInit(): void {
    this.subscription = combineLatest([this.route.url, this.route.queryParams])
      .pipe(
        switchMap(() => {
          const navRoute = new NavigationActivatedRoute(this.route);
          this.overviewService.objectType = this.m.Organisation;
          this.overviewService.id = navRoute.id();

          const title = this.overviewService.objectType.singularName;
          this.titleService.setTitle(title);

          return this.panelService.startEdit(navRoute.panel());
        }),
        tap(() => {
          this.refreshService.refresh();
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
