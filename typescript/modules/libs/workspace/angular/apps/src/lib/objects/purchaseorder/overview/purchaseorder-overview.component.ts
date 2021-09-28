import { Component, Self, AfterViewInit, OnDestroy, Injector } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { ActivatedRoute } from '@angular/router';

import { M } from '@allors/workspace/meta/default';
import { PurchaseOrder, PurchaseOrderItem } from '@allors/workspace/domain/default';
import { NavigationActivatedRoute, NavigationService, PanelManagerService, RefreshService, TestScope } from '@allors/workspace/angular/base';
import { SessionService } from '@allors/workspace/angular/core';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './purchaseorder-overview.component.html',
  providers: [PanelManagerService, SessionService],
})
export class PurchaseOrderOverviewComponent extends TestScope implements AfterViewInit, OnDestroy {
  title = 'Purchase Order';

  public order: PurchaseOrder;
  public orderItems: PurchaseOrderItem[] = [];

  subscription: Subscription;

  constructor(
    @Self() public panelManager: PanelManagerService,

    public refreshService: RefreshService,
    public navigation: NavigationService,
    private route: ActivatedRoute,
    public injector: Injector,
    private internalOrganisationId: InternalOrganisationId,
    titleService: Title
  ) {
    super();

    titleService.setTitle(this.title);
  }

  public ngAfterViewInit(): void {
    this.subscription = combineLatest(this.route.url, this.route.queryParams, this.refreshService.refresh$, this.internalOrganisationId.observable$)
      .pipe(
        switchMap(() => {
          const m = this.allors.workspace.configuration.metaPopulation as M;
          const { pullBuilder: pull } = m;
          const x = {};

          const navRoute = new NavigationActivatedRoute(this.route);
          this.panelManager.id = navRoute.id();
          this.panelManager.objectType = m.PurchaseOrder;
          this.panelManager.expanded = navRoute.panel();

          this.panelManager.on();

          const pulls = [
            pull.PurchaseOrder({
              object: this.panelManager.id,
              include: {
                PurchaseOrderItems: {
                  InvoiceItemType: x,
                },
              },
            }),
          ];

          this.panelManager.onPull(pulls);

          return this.panelManager.context.load(new PullRequest({ pulls }));
        })
      )
      .subscribe((loaded) => {
        this.panelManager.context.session.reset();

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
