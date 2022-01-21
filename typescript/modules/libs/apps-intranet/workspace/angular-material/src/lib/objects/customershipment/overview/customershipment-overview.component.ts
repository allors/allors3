import {
  Component,
  Self,
  AfterViewInit,
  OnDestroy,
  Injector,
} from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { M } from '@allors/default/workspace/meta';
import {
  Good,
  CustomerShipment,
  ShipmentItem,
  SalesInvoice,
  BillingProcess,
  SerialisedInventoryItemState,
} from '@allors/default/workspace/domain';
import {
  NavigationService,
  RefreshService,
  PanelManagerService,
  NavigationActivatedRoute,
} from '@allors/base/workspace/angular/foundation';
import {
  ContextService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { ActivatedRoute } from '@angular/router';

@Component({
  templateUrl: './customershipment-overview.component.html',
  providers: [PanelManagerService, ContextService],
})
export class CustomerShipmentOverviewComponent
  implements AfterViewInit, OnDestroy
{
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
    titleService.setTitle(this.title);

    this.allors.context.name = this.constructor.name;
    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;
  }

  public ngAfterViewInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest([
      this.route.url,
      this.route.queryParams,
      this.refreshService.refresh$,
      this.internalOrganisationId.observable$,
    ])
      .pipe(
        switchMap(() => {
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

          return this.panelManager.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.panelManager.context.reset();

        this.panelManager.onPulled(loaded);

        this.shipment = loaded.object<CustomerShipment>(m.Shipment);
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
