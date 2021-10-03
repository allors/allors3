import { Component, Self, AfterViewInit, OnDestroy, Injector } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { WorkTask } from '@allors/workspace/domain/default';
import { NavigationActivatedRoute, NavigationService, PanelManagerService, RefreshService, TestScope } from '@allors/workspace/angular/base';
import { ContextService, WorkspaceService } from '@allors/workspace/angular/core';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './worktask-overview.component.html',
  providers: [PanelManagerService, ContextService],
})
export class WorkTaskOverviewComponent extends TestScope implements AfterViewInit, OnDestroy {
  readonly m: M;
  title = 'WorkTask';

  workTask: WorkTask;

  subscription: Subscription;

  constructor(
    @Self() public panelManager: PanelManagerService,
    public workspaceService: WorkspaceService,
    public refreshService: RefreshService,
    public navigation: NavigationService,
    private route: ActivatedRoute,
    public injector: Injector,
    private internalOrganistationId: InternalOrganisationId,
    titleService: Title
  ) {
    super();

    titleService.setTitle(this.title);

    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;
  }

  public ngAfterViewInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;

    this.subscription = combineLatest(this.route.url, this.route.queryParams, this.refreshService.refresh$, this.internalOrganistationId.observable$)
      .pipe(
        switchMap(([, ,]) => {
          const navRoute = new NavigationActivatedRoute(this.route);
          this.panelManager.objectType = m.WorkTask;
          this.panelManager.id = navRoute.id();
          this.panelManager.expanded = navRoute.panel();

          this.panelManager.on();

          const pulls = [
            pull.WorkTask({
              objectId: this.panelManager.id,
            }),
          ];

          this.panelManager.onPull(pulls);

          return this.panelManager.context.pull( pulls);
        })
      )
      .subscribe((loaded) => {
        this.panelManager.context.reset();
        this.panelManager.onPulled(loaded);

        this.workTask = loaded.object<WorkTask>(m.WorkTask);
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
