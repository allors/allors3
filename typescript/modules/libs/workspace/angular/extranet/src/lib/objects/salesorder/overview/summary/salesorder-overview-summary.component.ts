import { Component, Self } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

import { M } from '@allors/workspace/meta/default';
import { SerialisedInventoryItemState, Shipment, SalesOrderItem, ProductQuote, SalesOrder, SalesInvoice, BillingProcess } from '@allors/workspace/domain/default';
import { Action, NavigationService, PanelService, RefreshService, SaveService } from '@allors/workspace/angular/base';
import { PrintService } from '../../../../actions/print/print.service';
import { WorkspaceService } from '@allors/workspace/angular/core';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'salesorder-overview-summary',
  templateUrl: './salesorder-overview-summary.component.html',
  providers: [PanelService],
})
export class SalesOrderOverviewSummaryComponent {
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
    @Self() public panel: PanelService,
    public workspaceService: WorkspaceService,
    public navigation: NavigationService,
    public printService: PrintService,
    public refreshService: RefreshService,
    private saveService: SaveService,
    public snackBar: MatSnackBar
  ) {
    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;
    const m = this.m;
    const { pullBuilder: pull, treeBuilder: tree } = m;
    const x = {};

    this.print = printService.print();

    panel.name = 'summary';

    const salesOrderPullName = `${panel.name}_${this.m.SalesOrder.tag}`;
    const salesInvoicePullName = `${panel.name}_${this.m.SalesInvoice.tag}`;
    const shipmentPullName = `${panel.name}_${this.m.Shipment.tag}`;
    const goodPullName = `${panel.name}_${this.m.Good.tag}`;
    const billingProcessPullName = `${panel.name}_${this.m.BillingProcess.tag}`;
    const serialisedInventoryItemStatePullName = `${panel.name}_${this.m.SerialisedInventoryItemState.tag}`;

    panel.onPull = (pulls) => {
      pulls.push(
        pull.SalesOrder({
          name: salesOrderPullName,
          objectId: this.panel.manager.id,
          include: {
            SalesOrderItems: {
              Product: x,
              InvoiceItemType: x,
              SalesOrderItemState: x,
              SalesOrderItemShipmentState: x,
              SalesOrderItemPaymentState: x,
              SalesOrderItemInvoiceState: x,
            },
            SalesTerms: {
              TermType: x,
            },
            BillToCustomer: x,
            BillToContactPerson: x,
            ShipToCustomer: x,
            ShipToContactPerson: x,
            ShipToEndCustomer: x,
            ShipToEndCustomerContactPerson: x,
            BillToEndCustomer: x,
            BillToEndCustomerContactPerson: x,
            SalesOrderState: x,
            SalesOrderShipmentState: x,
            SalesOrderInvoiceState: x,
            SalesOrderPaymentState: x,
            CreatedBy: x,
            LastModifiedBy: x,
            Quote: {
              Receiver: x,
            },
            PrintDocument: {
              Media: x,
            },
            DerivedShipToAddress: {
              Country: x,
            },
            DerivedBillToEndCustomerContactMechanism: {
              PostalAddress_Country: x,
            },
            DerivedShipToEndCustomerAddress: {
              Country: x,
            },
            DerivedBillToContactMechanism: {
              PostalAddress_Country: x,
            },
          },
        }),
        pull.SalesOrder({
          name: salesInvoicePullName,
          objectId: this.panel.manager.id,
          select: { SalesInvoicesWhereSalesOrder: x },
        }),
        pull.SalesOrder({
          name: shipmentPullName,
          objectId: this.panel.manager.id,
          select: {
            SalesOrderItems: {
              OrderShipmentsWhereOrderItem: {
                ShipmentItem: {
                  ShipmentWhereShipmentItem: x,
                },
              },
            },
          },
        }),
        pull.BillingProcess({
          name: billingProcessPullName,
          sorting: [{ roleType: m.BillingProcess.Name }],
        }),
        pull.SerialisedInventoryItemState({
          name: serialisedInventoryItemStatePullName,
          predicate: { kind: 'Equals', propertyType: m.SerialisedInventoryItemState.IsActive, value: true },
          sorting: [{ roleType: m.SerialisedInventoryItemState.Name }],
        })
      );
    };

    panel.onPulled = (loaded) => {
      this.order = loaded.object<SalesOrder>(salesOrderPullName);
      this.orderItems = loaded.collection<SalesOrderItem>(salesOrderPullName);
      this.billingProcesses = loaded.collection<BillingProcess>(billingProcessPullName);
      this.billingForOrderItems = this.billingProcesses?.find((v: BillingProcess) => v.UniqueId === 'ab01ccc2-6480-4fc0-b20e-265afd41fae2');
      this.inventoryItemStates = loaded.collection<SerialisedInventoryItemState>(serialisedInventoryItemStatePullName);

      this.salesInvoices = loaded.collection<SalesInvoice>(salesInvoicePullName);
      this.shipments = loaded.collection<Shipment>(shipmentPullName);
    };
  }

  public approve(): void {
    this.panel.manager.context.invoke(this.order.Approve).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully approved.', 'close', { duration: 5000 });
    }, this.saveService.errorHandler);
  }

  public setReadyForPosting() {
    this.panel.manager.context.invoke(this.order.SetReadyForPosting).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully set ready for posting.', 'close', { duration: 5000 });
    }, this.saveService.errorHandler);
  }

  public reopen() {
    this.panel.manager.context.invoke(this.order.Reopen).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully reopened.', 'close', { duration: 5000 });
    }, this.saveService.errorHandler);
  }

  public post() {
    this.panel.manager.context.invoke(this.order.Post).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully posted.', 'close', { duration: 5000 });
    }, this.saveService.errorHandler);
  }

  public accept() {
    this.panel.manager.context.invoke(this.order.Accept).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully accepted.', 'close', { duration: 5000 });
    }, this.saveService.errorHandler);
  }

  public revise(): void {
    this.panel.manager.context.invoke(this.order.Revise).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully revised.', 'close', { duration: 5000 });
    }, this.saveService.errorHandler);
  }

  public cancel(): void {
    this.panel.manager.context.invoke(this.order.Cancel).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully cancelled.', 'close', { duration: 5000 });
    }, this.saveService.errorHandler);
  }

  public reject(): void {
    this.panel.manager.context.invoke(this.order.Reject).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully rejected.', 'close', { duration: 5000 });
    }, this.saveService.errorHandler);
  }

  public hold(): void {
    this.panel.manager.context.invoke(this.order.Hold).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully put on hold.', 'close', { duration: 5000 });
    }, this.saveService.errorHandler);
  }

  public continue(): void {
    this.panel.manager.context.invoke(this.order.Continue).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully removed from hold.', 'close', { duration: 5000 });
    }, this.saveService.errorHandler);
  }

  public finish(): void {
    this.panel.manager.context.invoke(this.order.Continue).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully finished.', 'close', { duration: 5000 });
    }, this.saveService.errorHandler);
  }

  public ship(): void {
    this.panel.manager.context.invoke(this.order.Ship).subscribe(() => {
      this.panel.toggle();
      this.snackBar.open('Customer shipment successfully created.', 'close', { duration: 5000 });
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }

  public invoice(): void {
    this.panel.manager.context.invoke(this.order.Invoice).subscribe(() => {
      this.panel.toggle();
      this.snackBar.open('Sales invoice successfully created.', 'close', { duration: 5000 });
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }
}
