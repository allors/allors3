import { Component } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

import {
  ErrorService,
  InvokeService,
  RefreshService,
  SharedPullService,
} from '@allors/base/workspace/angular/foundation';

import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import {
  AllorsViewSummaryPanelComponent,
  NavigationService,
  PanelService,
  ScopedService,
} from '@allors/base/workspace/angular/application';
import { IPullResult, Pull } from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  CustomerShipment,
  Part,
  ProductQuote,
  RequestForQuote,
  SalesInvoice,
  SalesOrder,
  SerialisedItem,
} from '@allors/default/workspace/domain';

@Component({
  selector: 'serialiseditem-summary-panel',
  templateUrl: './serialiseditem-summary-panel.component.html',
})
export class SerialisedItemSummaryPanelComponent extends AllorsViewSummaryPanelComponent {
  m: M;

  serialisedItem: SerialisedItem;
  part: Part;
  request: RequestForQuote;
  quote: ProductQuote;
  order: SalesOrder;
  shipment: CustomerShipment;
  invoice: SalesInvoice;

  constructor(
    scopedService: ScopedService,
    panelService: PanelService,
    refreshService: RefreshService,
    sharedPullService: SharedPullService,
    workspaceService: WorkspaceService,
    public navigation: NavigationService,
    private snackBar: MatSnackBar,
    private invokeService: InvokeService,
    private errorService: ErrorService
  ) {
    super(scopedService, panelService, sharedPullService, refreshService);
    this.m = workspaceService.workspace.configuration.metaPopulation as M;
  }

  onPreSharedPull(pulls: Pull[], prefix?: string) {
    const { m } = this;
    const { pullBuilder: p } = m;

    const id = this.scoped.id;

    pulls.push(
      p.SerialisedItem({
        name: prefix,
        objectId: id,
        include: {
          SerialisedItemState: {},
          OwnedBy: {},
          RentedBy: {},
        },
      }),
      p.SerialisedItem({
        name: `${prefix}_part`,
        objectId: id,
        select: {
          PartWhereSerialisedItem: {},
        },
      }),
      p.SerialisedItem({
        name: `${prefix}_request`,
        objectId: id,
        select: {
          RequestItemsWhereSerialisedItem: {
            RequestWhereRequestItem: {},
          },
        },
      }),
      p.SerialisedItem({
        name: `${prefix}_quote`,
        objectId: id,
        select: {
          QuoteItemsWhereSerialisedItem: {
            QuoteWhereQuoteItem: {},
          },
        },
      }),
      p.SerialisedItem({
        name: `${prefix}_salesOrder`,
        objectId: id,
        select: {
          SalesOrderItemsWhereSerialisedItem: {
            SalesOrderWhereSalesOrderItem: {},
          },
        },
      }),
      p.SerialisedItem({
        name: `${prefix}_shipment`,
        objectId: id,
        select: {
          ShipmentItemsWhereSerialisedItem: {
            ShipmentWhereShipmentItem: {},
          },
        },
      }),
      p.SerialisedItem({
        name: `${prefix}_salesInvoice`,
        objectId: id,
        select: {
          SalesInvoiceItemsWhereSerialisedItem: {
            SalesInvoiceWhereSalesInvoiceItem: {},
          },
        },
      })
    );
  }

  onPostSharedPull(loaded: IPullResult, prefix?: string) {
    this.serialisedItem = loaded.object<SerialisedItem>(prefix);
    this.part = loaded.object<Part>(`${prefix}_part`);

    const requests =
      loaded.collection<RequestForQuote>(`${prefix}_request`) || [];
    if (requests.length > 0) {
      this.request = requests?.reduce(function (a, b) {
        return a.RequestDate > b.RequestDate ? a : b;
      });
    }

    const quotes = loaded.collection<ProductQuote>(`${prefix}_quote`) || [];
    if (quotes.length > 0) {
      this.quote = quotes?.reduce(function (a, b) {
        return a.IssueDate > b.IssueDate ? a : b;
      });
    }

    const orders = loaded.collection<SalesOrder>(`${prefix}_salesOrder`) || [];
    if (orders.length > 0) {
      this.order = orders?.reduce(function (a, b) {
        return a.OrderDate > b.OrderDate ? a : b;
      });
    }

    const shipments =
      loaded.collection<CustomerShipment>(`${prefix}_shipment`) || [];
    if (shipments.length > 0) {
      this.shipment = shipments?.reduce(function (a, b) {
        return a.EstimatedShipDate > b.EstimatedShipDate ? a : b;
      });
    }

    const invoices =
      loaded.collection<SalesInvoice>(`${prefix}_salesInvoice`) || [];
    if (invoices.length > 0) {
      this.invoice = invoices?.reduce(function (a, b) {
        return a.InvoiceDate > b.InvoiceDate ? a : b;
      });
    }
  }
}
