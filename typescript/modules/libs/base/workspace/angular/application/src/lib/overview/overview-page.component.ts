import {
  AfterViewInit,
  Directive,
  HostBinding,
  OnDestroy,
} from '@angular/core';
import {
  IPullResult,
  Pull,
  ScopedPullHandler,
} from '@allors/system/workspace/domain';
import { Subscription, tap } from 'rxjs';
import {
  AllorsComponent,
  RefreshService,
  SharedPullService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { M } from '@allors/default/workspace/meta';
import { ObjectService } from '../object/object.service';
import { ObjectInfo } from '../object/object-info';

@Directive()
export abstract class AllorsOverviewPageComponent
  extends AllorsComponent
  implements ScopedPullHandler, AfterViewInit, OnDestroy
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

  objectInfo: ObjectInfo;

  private subscription: Subscription;

  constructor(
    public objectService: ObjectService,
    public sharedPullService: SharedPullService,
    public refreshService: RefreshService,
    workspaceService: WorkspaceService
  ) {
    super();

    this.m = workspaceService.workspace.configuration.metaPopulation as M;

    this.sharedPullService.register(this);
  }

  ngAfterViewInit(): void {
    this.subscription = this.objectService.objectInfo$
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

  abstract onPreScopedPull(pulls: Pull[], scope?: string): void;

  abstract onPostScopedPull(pullResult: IPullResult, scope?: string): void;
}
