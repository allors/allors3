import {
  AfterViewInit,
  Directive,
  HostBinding,
  OnDestroy,
} from '@angular/core';
import {
  IPullResult,
  Pull,
  SharedPullHandler,
} from '@allors/system/workspace/domain';
import {
  combineLatest,
  delay,
  map,
  shareReplay,
  Subscription,
  switchMap,
  tap,
} from 'rxjs';
import {
  AllorsComponent,
  RefreshService,
  SharedPullService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { ScopedService } from '../scoped/scoped.service';
import { Scoped } from '../scoped/scoped';
import { ActivatedRoute } from '@angular/router';
import { NavigationActivatedRoute } from '../navigation/navigation-activated-route';
import { PanelService } from '../panel/panel.service';
import { Composite } from '@allors/system/workspace/meta';

@Directive()
export abstract class AllorsOverviewPageComponent
  extends AllorsComponent
  implements SharedPullHandler, AfterViewInit, OnDestroy
{
  @HostBinding('attr.data-allors-id')
  get dataAllorsId() {
    return this.scoped?.id;
  }

  @HostBinding('attr.data-allors-objecttype')
  get dataAllorsObjectType() {
    return this.scoped?.objectType?.tag;
  }

  scoped: Scoped;

  private subscription: Subscription;

  constructor(
    public scopedService: ScopedService,
    public panelService: PanelService,
    public sharedPullService: SharedPullService,
    public refreshService: RefreshService,
    route: ActivatedRoute,
    workspaceService: WorkspaceService,
    tag?: string
  ) {
    super();

    this.sharedPullService.register(this);

    const metaPopulation = workspaceService.metaPopulation;
    let objectType: Composite;

    this.scopedService.scoped$ = combineLatest([
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
        if (tag) {
          objectType = metaPopulation.metaObjectByTag.get(tag) as Composite;
        } else {
          const uppercaseName = navRoute.composite().toUpperCase();
          objectType = metaPopulation.objectTypeByUppercaseName.get(
            uppercaseName
          ) as Composite;
        }

        return {
          objectType,
          id: navRoute.id(),
        };
      }),
      shareReplay()
    );
  }

  ngAfterViewInit(): void {
    this.subscription = this.scopedService.scoped$
      .pipe(
        tap((info) => {
          this.scoped = info;
          this.refreshService.refresh();
        })
      )
      .subscribe();
  }

  ngOnDestroy(): void {
    this.sharedPullService.unregister(this);

    if (this.subscription) {
      this.subscription?.unsubscribe();
    }
  }

  abstract onPreSharedPull(pulls: Pull[], prefix?: string): void;

  abstract onPostSharedPull(pullResult: IPullResult, prefix?: string): void;
}
