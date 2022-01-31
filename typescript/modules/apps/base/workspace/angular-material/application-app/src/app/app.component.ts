import { map, Subscription, switchMap, tap } from 'rxjs';
import { Component, OnDestroy } from '@angular/core';
import {
  OnPullService,
  RefreshService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';

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
          const onPulls = [...this.onPullService.onPulls];

          const pulls = [];
          for (const onPull of onPulls) {
            onPull.onPrePull(context, pulls);
          }

          return context
            .pull(pulls)
            .pipe(map((pullResult) => ({ context, onPulls, pullResult })));
        }),
        tap(({ context, onPulls, pullResult }) => {
          for (const onPull of onPulls) {
            onPull.onPostPull(context, pullResult);
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
