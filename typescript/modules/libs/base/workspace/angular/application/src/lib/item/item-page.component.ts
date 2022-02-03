import {
  AfterViewInit,
  Directive,
  HostBinding,
  OnDestroy,
} from '@angular/core';
import { ItemPageInfo, ItemPageService } from './item-page.service';
import {
  IPullResult,
  Pull,
  SharedPullHandler,
} from '@allors/system/workspace/domain';
import { Subscription, tap } from 'rxjs';
import {
  AllorsComponent,
  RefreshService,
  SharedPullService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { M } from '@allors/default/workspace/meta';

@Directive()
export abstract class AllorsItemPageComponent
  extends AllorsComponent
  implements SharedPullHandler, AfterViewInit, OnDestroy
{
  @HostBinding('attr.data-allors-id')
  get dataAllorsId() {
    return this.overviewPageInfo.id;
  }

  @HostBinding('attr.data-allors-objecttype')
  get dataAllorsObjectType() {
    return this.overviewPageInfo?.objectType?.tag;
  }

  m: M;

  overviewPageInfo: ItemPageInfo;

  private subscription: Subscription;

  constructor(
    public itemPageService: ItemPageService,
    public sharedPullService: SharedPullService,
    public refreshService: RefreshService,
    workspaceService: WorkspaceService
  ) {
    super();

    this.m = workspaceService.workspace.configuration.metaPopulation as M;

    this.sharedPullService.register(this);
  }

  ngAfterViewInit(): void {
    this.subscription = this.itemPageService.info$
      .pipe(
        tap((info) => {
          this.overviewPageInfo = info;
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
