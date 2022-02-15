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
import { Subscription, tap } from 'rxjs';
import {
  AllorsComponent,
  RefreshService,
  SharedPullService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { M } from '@allors/default/workspace/meta';
import { ScopedService } from '../scoped/scoped.service';
import { Scoped } from '../scoped/scoped';

@Directive()
export abstract class AllorsOverviewPageComponent
  extends AllorsComponent
  implements SharedPullHandler, AfterViewInit, OnDestroy
{
  @HostBinding('attr.data-allors-id')
  get dataAllorsId() {
    return this.objectInfo?.id;
  }

  @HostBinding('attr.data-allors-objecttype')
  get dataAllorsObjectType() {
    return this.objectInfo?.objectType?.tag;
  }

  m: M;

  objectInfo: Scoped;

  private subscription: Subscription;

  constructor(
    public objectService: ScopedService,
    public sharedPullService: SharedPullService,
    public refreshService: RefreshService,
    workspaceService: WorkspaceService
  ) {
    super();

    this.m = workspaceService.workspace.configuration.metaPopulation as M;

    this.sharedPullService.register(this);
  }

  ngAfterViewInit(): void {
    this.subscription = this.objectService.scoped$
      .pipe(
        tap((info) => {
          this.objectInfo = info;
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
