import { Component, Self, AfterViewInit, OnDestroy, Injector } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { ActivatedRoute } from '@angular/router';

import { M } from '@allors/workspace/meta/default';
import { PurchaseOrder, PurchaseOrderItem } from '@allors/workspace/domain/default';
import { NavigationActivatedRoute, NavigationService, PanelManagerService, RefreshService, TestScope } from '@allors/workspace/angular/base';
import { ContextService, WorkspaceService } from '@allors/workspace/angular/core';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './purchaseorder-overview.component.html',
  providers: [PanelManagerService, ContextService],
})
export class PurchaseOrderOverviewComponent extends TestScope implements AfterViewInit, OnDestroy {
  title = 'Purchase Order';

  public order: PurchaseOrder;
  public orderItems: PurchaseOrderItem[] = [];

  subscription: Subscription;
  m: M;

  constructor(
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

    titleService.setTitle(this.title);

    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;
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
          this.panelManager.objectType = m.PurchaseOrder;
          this.panelManager.expanded = navRoute.panel();

          this.panelManager.on();

          const pulls = [
            pull.PurchaseOrder({
              objectId: this.panelManager.id,
              include: {
                PurchaseOrderItems: {
                  InvoiceItemType: x,
                },
              },
            }),
          ];

          this.panelManager.onPull(pulls);

          return this.panelManager.context.pull( pulls);
        })
      )
      .subscribe((loaded) => {
        this.panelManager.context.reset();

        this.panelManager.onPulled(loaded);

        this.order = loaded.object<PurchaseOrder>(m.PurchaseOrder);
        this.orderItems = this.order.PurchaseOrderItems;
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
