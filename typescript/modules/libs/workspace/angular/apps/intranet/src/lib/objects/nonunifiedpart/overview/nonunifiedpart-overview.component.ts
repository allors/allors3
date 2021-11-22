import { Component, Self, AfterViewInit, OnDestroy, Injector } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { Part, NonUnifiedPart } from '@allors/workspace/domain/default';
import { NavigationActivatedRoute, NavigationService, PanelManagerService, RefreshService, TestScope } from '@allors/workspace/angular/base';
import { ContextService, WorkspaceService } from '@allors/workspace/angular/core';
import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { M } from '@allors/workspace/meta/default';

@Component({
  templateUrl: './nonunifiedpart-overview.component.html',
  providers: [PanelManagerService, ContextService],
})
export class NonUnifiedPartOverviewComponent extends TestScope implements AfterViewInit, OnDestroy {
  title = 'Part';

  part: Part;

  subscription: Subscription;
  serialised: boolean;
  m: M;

  constructor(
    @Self() public allors: ContextService,
    @Self() public panelManager: PanelManagerService,
    public workspaceService: WorkspaceService,
    public refreshService: RefreshService,
    public navigation: NavigationService,
    private route: ActivatedRoute,
    public injector: Injector,
    private internalOrganisationId: InternalOrganisationId,
    titleService: Title
  ) {
    super();

    this.allors.context.name = this.constructor.name;
    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;

    titleService.setTitle(this.title);
  }

  public ngAfterViewInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest(this.route.url, this.route.queryParams, this.refreshService.refresh$, this.internalOrganisationId.observable$)
      .pipe(
        switchMap(() => {
          const navRoute = new NavigationActivatedRoute(this.route);
          this.panelManager.id = navRoute.id();
          this.panelManager.objectType = m.NonUnifiedPart;
          this.panelManager.expanded = navRoute.panel();

          this.panelManager.on();

          const pulls = [
            pull.NonUnifiedPart({
              objectId: this.panelManager.id,
              include: {
                InventoryItemKind: x,
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

        const part = loaded.object<NonUnifiedPart>(m.NonUnifiedPart);
        this.serialised = part.InventoryItemKind.UniqueId === '2596e2dd-3f5d-4588-a4a2-167d6fbe3fae';
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
