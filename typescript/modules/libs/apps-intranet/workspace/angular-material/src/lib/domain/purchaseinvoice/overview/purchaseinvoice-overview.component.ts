import { combineLatest, delay, map, switchMap } from 'rxjs';
import { Component, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import {
  PurchaseInvoice,
  PurchaseOrder,
} from '@allors/default/workspace/domain';
import {
  RefreshService,
  SharedPullService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import {
  NavigationService,
  NavigationActivatedRoute,
  PanelService,
  ScopedService,
  AllorsOverviewPageComponent,
} from '@allors/base/workspace/angular/application';
import { IPullResult, Pull } from '@allors/system/workspace/domain';
import { AllorsMaterialPanelService } from '@allors/base/workspace/angular-material/application';
import { M } from '@allors/default/workspace/meta';

@Component({
  templateUrl: './purchaseinvoice-overview.component.html',
  providers: [
    ScopedService,
    {
      provide: PanelService,
      useClass: AllorsMaterialPanelService,
    },
  ],
})
export class PurchaseInvoiceOverviewComponent extends AllorsOverviewPageComponent {
  m: M;

  order: PurchaseOrder;
  invoice: PurchaseInvoice;

  constructor(
    @Self() scopedService: ScopedService,
    @Self() panelService: PanelService,
    public navigation: NavigationService,
    sharedPullService: SharedPullService,
    refreshService: RefreshService,
    route: ActivatedRoute,
    workspaceService: WorkspaceService
  ) {
    super(
      scopedService,
      panelService,
      sharedPullService,
      refreshService,
      route,
      workspaceService
    );
    this.m = workspaceService.workspace.configuration.metaPopulation as M;
  }

  onPreSharedPull(pulls: Pull[], prefix?: string) {
    const {
      m: { pullBuilder: p },
    } = this;

    const id = this.scoped.id;

    pulls.push(
      p.PurchaseInvoice({
        name: prefix,
        objectId: id,
        include: {
          PurchaseInvoiceItems: {
            InvoiceItemType: {},
          },
          BilledFrom: {},
          BilledFromContactPerson: {},
          BillToEndCustomer: {},
          BillToEndCustomerContactPerson: {},
          ShipToEndCustomer: {},
          ShipToEndCustomerContactPerson: {},
          PurchaseInvoiceState: {},
          CreatedBy: {},
          LastModifiedBy: {},
          PurchaseOrders: {},
          DerivedBillToEndCustomerContactMechanism: {
            PostalAddress_Country: {},
          },
          DerivedShipToEndCustomerAddress: {
            Country: {},
          },
        },
      })
    );
  }

  onPostSharedPull(loaded: IPullResult, prefix?: string) {
    this.invoice = loaded.object<PurchaseInvoice>(prefix);
  }
}
