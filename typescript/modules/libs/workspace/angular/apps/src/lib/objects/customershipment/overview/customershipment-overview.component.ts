import { Component, Self, AfterViewInit, OnDestroy, Injector } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { Good, CustomerShipment, ShipmentItem, SalesInvoice, BillingProcess, SerialisedInventoryItemState } from '@allors/workspace/domain/default';
import { NavigationService, RefreshService, TestScope, PanelManagerService, NavigationActivatedRoute } from '@allors/workspace/angular/base';
import { SessionService } from '@allors/workspace/angular/core';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { ActivatedRoute } from '@angular/router';

@Component({
  templateUrl: './customershipment-overview.component.html',
  providers: [PanelManagerService, SessionService],
})
export class CustomerShipmentOverviewComponent extends TestScope implements AfterViewInit, OnDestroy {
  title = 'Customer Shipment';

  public shipment: CustomerShipment;
  public orderItems: ShipmentItem[] = [];
  public goods: Good[] = [];
  public salesInvoice: SalesInvoice;
  public billingProcesses: BillingProcess[];
  public billingForOrderItems: BillingProcess;
  public selectedSerialisedInventoryState: string;
  public inventoryItemStates: SerialisedInventoryItemState[];

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
    this.subscription = combineLatest([this.route.url, this.route.queryParams, this.refreshService.refresh$, this.internalOrganisationId.observable$])
      .pipe(
        switchMap(() => {
          const m = this.m;
          const { pullBuilder: pull } = m;
          const x = {};

          const navRoute = new NavigationActivatedRoute(this.route);
          this.panelManager.id = navRoute.id();
          this.panelManager.objectType = m.Shipment;
          this.panelManager.expanded = navRoute.panel();

          this.panelManager.on();

          const pulls = [
            pull.Shipment({
              objectId: this.panelManager.id,
              include: {
                ShipmentItems: {
                  Good: x,
                },
                ShipFromParty: x,
                ShipFromAddress: x,
                ShipToParty: x,
                ShipToContactPerson: x,
                ShipmentState: x,
                CreatedBy: x,
                LastModifiedBy: x,
                ShipToAddress: {
                  Country: x,
                },
              },
            }),
          ];

          this.panelManager.onPull(pulls);

          return this.panelManager.client.pullReactive(this.panelManager.session, pulls);
        })
      )
      .subscribe((loaded) => {
        this.panelManager.session.reset();

        this.panelManager.onPulled(loaded);

        this.shipment = loaded.object<Shipment>(m.Shipment);
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
