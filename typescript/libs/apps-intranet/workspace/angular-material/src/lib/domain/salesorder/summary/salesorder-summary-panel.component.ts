import { Component } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

import {
  Action,
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
  BillingProcess,
  ProductQuote,
  SalesInvoice,
  SalesOrder,
  SalesOrderItem,
  SerialisedInventoryItemState,
  Shipment,
} from '@allors/default/workspace/domain';
import { PrintService } from '../../../actions/print/print.service';

@Component({
  selector: 'salesorder-summary-panel',
  templateUrl: './salesorder-summary-panel.component.html',
})
export class SalesOrderSummaryPanelComponent extends AllorsViewSummaryPanelComponent {
  m: M;

  order: SalesOrder;
  quote: ProductQuote;
  orderItems: SalesOrderItem[] = [];
  shipments: Shipment[] = [];
  salesInvoices: SalesInvoice[] = [];
  billingProcesses: BillingProcess[];
  billingForOrderItems: BillingProcess;
  selectedSerialisedInventoryState: string;
  inventoryItemStates: SerialisedInventoryItemState[];

  print: Action;

  constructor(
    scopedService: ScopedService,
    panelService: PanelService,
    refreshService: RefreshService,
    sharedPullService: SharedPullService,
    workspaceService: WorkspaceService,
    private snackBar: MatSnackBar,
    private invokeService: InvokeService,
    private errorService: ErrorService,
    public printService: PrintService,
    public navigation: NavigationService
  ) {
    super(scopedService, panelService, sharedPullService, refreshService);
    this.m = workspaceService.workspace.configuration.metaPopulation as M;
    this.print = printService.print();
  }

  onPreSharedPull(pulls: Pull[], prefix?: string) {
    const { m } = this;
    const { pullBuilder: p } = m;

    const id = this.scoped.id;

    pulls.push(
      p.SalesOrder({
        name: prefix,
        objectId: id,
        include: {
          SalesOrderItems: {
            Product: {},
            InvoiceItemType: {},
            SalesOrderItemState: {},
            SalesOrderItemShipmentState: {},
            SalesOrderItemPaymentState: {},
            SalesOrderItemInvoiceState: {},
          },
          SalesTerms: {
            TermType: {},
          },
          BillToCustomer: {},
          BillToContactPerson: {},
          ShipToCustomer: {},
          ShipToContactPerson: {},
          ShipToEndCustomer: {},
          ShipToEndCustomerContactPerson: {},
          BillToEndCustomer: {},
          BillToEndCustomerContactPerson: {},
          SalesOrderState: {},
          SalesOrderShipmentState: {},
          SalesOrderInvoiceState: {},
          SalesOrderPaymentState: {},
          CreatedBy: {},
          LastModifiedBy: {},
          Quote: {
            Receiver: {},
          },
          PrintDocument: {
            Media: {},
          },
          DerivedShipToAddress: {
            Country: {},
          },
          DerivedBillToEndCustomerContactMechanism: {
            PostalAddress_Country: {},
          },
          DerivedShipToEndCustomerAddress: {
            Country: {},
          },
          DerivedBillToContactMechanism: {
            PostalAddress_Country: {},
          },
        },
      }),
      p.SalesOrder({
        name: `${prefix}_salesInvoice`,
        objectId: id,
        select: { SalesInvoicesWhereSalesOrder: {} },
      }),
      p.SalesOrder({
        name: `${prefix}_shipment`,
        objectId: id,
        select: {
          SalesOrderItems: {
            OrderShipmentsWhereOrderItem: {
              ShipmentItem: {
                ShipmentWhereShipmentItem: {},
              },
            },
          },
        },
      }),
      p.BillingProcess({
        name: `${prefix}_billingProcess`,
        sorting: [{ roleType: m.BillingProcess.Name }],
      }),
      p.SerialisedInventoryItemState({
        name: `${prefix}_serialisedInventoryItemState`,
        predicate: {
          kind: 'Equals',
          propertyType: m.SerialisedInventoryItemState.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.SerialisedInventoryItemState.Name }],
      })
    );
  }

  onPostSharedPull(loaded: IPullResult, prefix?: string) {
    this.order = loaded.object<SalesOrder>(prefix);
    this.billingProcesses = loaded.collection<BillingProcess>(
      `${prefix}_billingProcess`
    );
    this.billingForOrderItems = this.billingProcesses?.find(
      (v: BillingProcess) =>
        v.UniqueId === 'ab01ccc2-6480-4fc0-b20e-265afd41fae2'
    );
    this.inventoryItemStates = loaded.collection<SerialisedInventoryItemState>(
      `${prefix}_serialisedInventoryItemState`
    );

    this.salesInvoices = loaded.collection<SalesInvoice>(
      `${prefix}_salesInvoice`
    );
    this.shipments = loaded.collection<Shipment>(`${prefix}_shipment`);
  }

  public approve(): void {
    this.invokeService.invoke(this.order.Approve).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully approved.', 'close', { duration: 5000 });
    }, this.errorService.errorHandler);
  }

  public setReadyForPosting() {
    this.invokeService.invoke(this.order.SetReadyForPosting).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully set ready for posting.', 'close', {
        duration: 5000,
      });
    }, this.errorService.errorHandler);
  }

  public reopen() {
    this.invokeService.invoke(this.order.Reopen).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully reopened.', 'close', { duration: 5000 });
    }, this.errorService.errorHandler);
  }

  public post() {
    this.invokeService.invoke(this.order.Post).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully posted.', 'close', { duration: 5000 });
    }, this.errorService.errorHandler);
  }

  public accept() {
    this.invokeService.invoke(this.order.Accept).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully accepted.', 'close', { duration: 5000 });
    }, this.errorService.errorHandler);
  }

  public revise(): void {
    this.invokeService.invoke(this.order.Revise).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully revised.', 'close', { duration: 5000 });
    }, this.errorService.errorHandler);
  }

  public cancel(): void {
    this.invokeService.invoke(this.order.Cancel).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully cancelled.', 'close', {
        duration: 5000,
      });
    }, this.errorService.errorHandler);
  }

  public reject(): void {
    this.invokeService.invoke(this.order.Reject).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully rejected.', 'close', { duration: 5000 });
    }, this.errorService.errorHandler);
  }

  public hold(): void {
    this.invokeService.invoke(this.order.Hold).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully put on hold.', 'close', {
        duration: 5000,
      });
    }, this.errorService.errorHandler);
  }

  public continue(): void {
    this.invokeService.invoke(this.order.Continue).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully removed from hold.', 'close', {
        duration: 5000,
      });
    }, this.errorService.errorHandler);
  }

  public finish(): void {
    this.invokeService.invoke(this.order.Continue).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully finished.', 'close', { duration: 5000 });
    }, this.errorService.errorHandler);
  }

  public ship(): void {
    this.invokeService.invoke(this.order.Ship).subscribe(() => {
      this.snackBar.open('Customer shipment successfully created.', 'close', {
        duration: 5000,
      });
      this.refreshService.refresh();
    }, this.errorService.errorHandler);
  }

  public invoice(): void {
    this.invokeService.invoke(this.order.Invoice).subscribe(() => {
      this.snackBar.open('Sales invoice successfully created.', 'close', {
        duration: 5000,
      });
      this.refreshService.refresh();
    }, this.errorService.errorHandler);
  }
}
