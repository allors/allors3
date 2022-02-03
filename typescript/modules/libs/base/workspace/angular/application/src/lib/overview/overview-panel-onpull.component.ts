import { Subscription, tap } from 'rxjs';
import { Directive, OnDestroy } from '@angular/core';
import { IPullResult, OnPull, Pull } from '@allors/system/workspace/domain';
import {
  OnPullService,
  RefreshService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { Panel } from '../panel/panel';
import { PanelService } from '../panel/panel.service';
import { OverviewPageService } from './overview-page.service';
import { AllorsOverviewPanelComponent } from './overview-panel.component';

@Directive()
export abstract class AllorsOverviewOnPullPanelComponent
  extends AllorsOverviewPanelComponent
  implements Panel, OnPull, OnDestroy
{
  private subscription: Subscription;

  constructor(
    public overviewService: OverviewPageService,
    public panelService: PanelService,
    public onPullService: OnPullService,
    public refreshService: RefreshService,
    workspaceService: WorkspaceService
  ) {
    super(workspaceService);

    panelService.register(this);
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
    this.panelService.unregister(this);
    this.onPullService.unregister(this);

    if (this.subscription) {
      this.subscription?.unsubscribe();
    }
  }

  abstract onPrePull(pulls: Pull[], prefix?: string): void;

  abstract onPostPull(pullResult: IPullResult, prefix?: string): void;
}
