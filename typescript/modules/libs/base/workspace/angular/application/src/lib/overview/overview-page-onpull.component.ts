import { Directive, OnDestroy } from '@angular/core';
import {
  OnPullService,
  RefreshService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { OverviewPageService } from './overview-page.service';
import { AllorsOverviewPageComponent } from './overview-page.component';
import { IPullResult, OnPull, Pull } from '@allors/system/workspace/domain';
import { Subscription, tap } from 'rxjs';

@Directive()
export abstract class AllorsOverviewPageOnPullComponent
  extends AllorsOverviewPageComponent
  implements OnPull, OnDestroy
{
  private subscription: Subscription;

  constructor(
    public overviewService: OverviewPageService,
    public onPullService: OnPullService,
    public refreshService: RefreshService,
    workspaceService: WorkspaceService
  ) {
    super(workspaceService);

    this.onPullService.register(this);
  }

  ngAfterViewInit(): void {
    this.subscription = this.overviewService.info$
      .pipe(
        tap((info) => {
          this.overviewPageInfo = info;
          this.refreshService.refresh();
        })
      )
      .subscribe();
  }

  ngOnDestroy(): void {
    this.onPullService.unregister(this);

    if (this.subscription) {
      this.subscription?.unsubscribe();
    }
  }

  abstract onPrePull(pulls: Pull[], prefix?: string): void;

  abstract onPostPull(pullResult: IPullResult, prefix?: string): void;
}
