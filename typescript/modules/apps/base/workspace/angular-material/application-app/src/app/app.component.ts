import { map, Subscription, switchMap, tap } from 'rxjs';
import { Component, NgZone, OnDestroy } from '@angular/core';
import {
  RefreshService,
  SharedPullService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { SharedPullHandler } from '@allors/system/workspace/domain';
import { Router } from '@angular/router';

@Component({
  selector: 'allors-root',
  templateUrl: './app.component.html',
})
export class AppComponent implements OnDestroy {
  subscription: Subscription;

  constructor(
    public ngZone: NgZone,
    public router: Router,
    private workspaceService: WorkspaceService,
    private refreshService: RefreshService,
    private sharePullService: SharedPullService
  ) {
    this.subscription = this.refreshService.refresh$
      .pipe(
        switchMap(() => {
          const context = this.workspaceService.contextBuilder();
          context.name = 'refresh';
          const onPulls = [...this.sharePullService.handlers];

          const prefixByOnPull: Map<SharedPullHandler, string> = new Map();
          let counter = 0;

          const pulls = [];
          for (const onPull of onPulls) {
            const prefix = `${++counter}`;
            prefixByOnPull.set(onPull, prefix);
            onPull.onPreSharedPull(pulls, prefix);
          }

          return context.pull(pulls).pipe(
            map((pullResult) => ({
              onPulls,
              pullResult,
              prefixByOnPull,
            }))
          );
        }),
        tap(({ onPulls, pullResult, prefixByOnPull }) => {
          for (const onPull of onPulls) {
            const prefix = prefixByOnPull.get(onPull);
            onPull.onPostSharedPull(pullResult, prefix);
          }
        })
      )
      .subscribe();
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }
}
