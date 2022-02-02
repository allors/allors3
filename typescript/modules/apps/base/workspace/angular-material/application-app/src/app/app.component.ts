import { map, Subscription, switchMap, tap } from 'rxjs';
import { Component, OnDestroy } from '@angular/core';
import {
  OnPullService,
  RefreshService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { OnPull } from '@allors/system/workspace/domain';

@Component({
  selector: 'allors-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent implements OnDestroy {
  subscription: Subscription;

  constructor(
    private workspaceService: WorkspaceService,
    private refreshService: RefreshService,
    private onPullService: OnPullService
  ) {
    this.subscribe();
  }

  private subscribe() {
    this.subscription?.unsubscribe();
    this.subscription = this.refreshService.refresh$
      .pipe(
        switchMap(() => {
          const context = this.workspaceService.contextBuilder();
          context.name = 'refresh';
          const onPulls = [...this.onPullService.onPulls];

          const prefixByOnPull: Map<OnPull, string> = new Map();
          let counter = 0;

          const pulls = [];
          for (const onPull of onPulls) {
            const prefix = `${++counter}`;
            prefixByOnPull.set(onPull, prefix);
            onPull.onPrePull(pulls, prefix);
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
            onPull.onPostPull(pullResult, prefix);
          }
        })
      )
      .subscribe({
        error: this.subscribe,
      });
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }
}
