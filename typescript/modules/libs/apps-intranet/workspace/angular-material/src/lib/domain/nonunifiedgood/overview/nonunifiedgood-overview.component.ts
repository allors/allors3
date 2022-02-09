import {
  Component,
  Self,
  AfterViewInit,
  OnDestroy,
  Injector,
} from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { M } from '@allors/default/workspace/meta';
import { Good, NonUnifiedGood } from '@allors/default/workspace/domain';
import {
  NavigationActivatedRoute,
  NavigationService,
  OldPanelManagerService,
  RefreshService,
} from '@allors/base/workspace/angular/foundation';
import {
  ContextService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './nonunifiedgood-overview.component.html',
  providers: [OldPanelManagerService, ContextService],
})
export class NonUnifiedGoodOverviewComponent
  implements AfterViewInit, OnDestroy
{
  title = 'Good';

  good: Good;

  subscription: Subscription;
  m: M;

  constructor(
    @Self() public allors: ContextService,
    @Self() public panelManager: OldPanelManagerService,
    public workspaceService: WorkspaceService,
    public refreshService: RefreshService,
    public navigation: NavigationService,
    private route: ActivatedRoute,
    public injector: Injector,
    private internalOrganisationId: InternalOrganisationId,
    titleService: Title
  ) {
    titleService.setTitle(this.title);

    this.allors.context.name = this.constructor.name;
    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest(
      this.route.url,
      this.route.queryParams,
      this.refreshService.refresh$,
      this.internalOrganisationId.observable$
    )
      .pipe(
        switchMap(([, ,]) => {
          const navRoute = new NavigationActivatedRoute(this.route);
          this.panelManager.objectType = m.NonUnifiedGood;
          this.panelManager.id = navRoute.id();
          this.panelManager.expanded = navRoute.panel();

          this.panelManager.on();

          const pulls = [
            pull.NonUnifiedGood({
              objectId: this.panelManager.id,
              include: {
                ProductIdentifications: {
                  ProductIdentificationType: x,
                },
                Part: {
                  Brand: x,
                  Model: x,
                },
              },
            }),
          ];

          this.panelManager.onPull(pulls);

          return this.panelManager.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.panelManager.context.reset();
        this.panelManager.onPulled(loaded);

        this.good = loaded.object<NonUnifiedGood>(m.NonUnifiedGood);
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
