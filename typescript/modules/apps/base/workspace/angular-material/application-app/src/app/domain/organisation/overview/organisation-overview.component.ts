import { combineLatest, delay, map, Subscription, switchMap, tap } from 'rxjs';
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
  OverviewPageInfo,
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
  implements OnPull, AfterViewInit, OnDestroy
{
  info: OverviewPageInfo;
  object: Organisation;

  private subscription: Subscription;

  constructor(
    @Self() overviewService: OverviewPageService,
    @Self() private panelService: PanelService,
    public navigation: NavigationService,
    private titleService: Title,
    private refreshService: RefreshService,
    private onPullService: OnPullService,
    workspaceService: WorkspaceService,
    route: ActivatedRoute
  ) {
    super(overviewService, workspaceService);

    onPullService.register(this);

    this.overviewService.info$ = combineLatest([
      route.url,
      route.queryParams,
    ]).pipe(
      delay(1),
      map(() => new NavigationActivatedRoute(route)),
      switchMap((navRoute) => {
        return this.panelService
          .startEdit(navRoute.panel())
          .pipe(map(() => navRoute));
      }),
      map((navRoute) => {
        return {
          objectType: this.m.Organisation,
          id: navRoute.id(),
        };
      })
    );
  }

  ngAfterViewInit(): void {
    this.subscription = this.overviewService.info$
      .pipe(
        tap((info) => {
          this.info = info;
          this.refreshService.refresh();
        })
      )
      .subscribe();
  }

  ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }

    this.onPullService.unregister(this);
  }

  onPrePull(pulls: Pull[], prefix: string) {
    const {
      m: { pullBuilder: p },
    } = this;

    pulls.push(
      p.Organisation({
        name: prefix,
        objectId: this.info.id,
      })
    );
  }

  onPostPull(pullResult: IPullResult, prefix: string) {
    this.object = pullResult.object(prefix);
    const title = this.info.objectType.singularName;
    this.titleService.setTitle(title);
  }
}
