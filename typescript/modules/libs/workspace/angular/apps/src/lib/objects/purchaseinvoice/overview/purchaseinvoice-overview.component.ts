import { Component, Self, AfterViewInit, OnDestroy, Injector } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { PurchaseOrder, PurchaseInvoice } from '@allors/workspace/domain/default';
import { NavigationActivatedRoute, NavigationService, PanelManagerService, RefreshService, TestScope } from '@allors/workspace/angular/base';
import { SessionService } from '@allors/workspace/angular/core';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './purchaseinvoice-overview.component.html',
  providers: [PanelManagerService, SessionService],
})
export class PurchaseInvoiceOverviewComponent extends TestScope implements AfterViewInit, OnDestroy {
  title = 'Purchase Invoice';

  order: PurchaseOrder;
  invoice: PurchaseInvoice;

  subscription: Subscription;

  constructor(
    @Self() public panelManager: PanelManagerService,

    public refreshService: RefreshService,
    public navigation: NavigationService,
    private route: ActivatedRoute,
    private internalOrganisationId: InternalOrganisationId,
    public injector: Injector,
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
          this.panelManager.objectType = m.PurchaseInvoice;
          this.panelManager.expanded = navRoute.panel();

          const { id } = this.panelManager;

          this.panelManager.on();

          const pulls = [
            pull.PurchaseInvoice({
              objectId: id,
              include: {
                PurchaseInvoiceItems: {
                  InvoiceItemType: x,
                },
                BilledFrom: x,
                BilledFromContactPerson: x,
                BillToEndCustomer: x,
                BillToEndCustomerContactPerson: x,
                ShipToEndCustomer: x,
                ShipToEndCustomerContactPerson: x,
                PurchaseInvoiceState: x,
                CreatedBy: x,
                LastModifiedBy: x,
                PurchaseOrders: x,
                DerivedBillToEndCustomerContactMechanism: {
                  PostalAddress_Country: {},
                },
                DerivedShipToEndCustomerAddress: {
                  Country: x,
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
        this.invoice = loaded.object<PurchaseInvoice>(m.PurchaseInvoice);
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
