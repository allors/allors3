import { Component, Self, HostBinding } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

import { M } from '@allors/workspace/meta/default';
import { Organisation, PurchaseOrder, PurchaseOrderItem, InvoiceItemType, PurchaseInvoice, OrderItemBilling, PurchaseInvoiceItem, InternalOrganisation } from '@allors/workspace/domain/default';
import { Action, DeleteService, MethodService, NavigationService, ObjectData, ObjectService, PanelService, RefreshService, SaveService, Table, TableRow, TestScope, OverviewService } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';

import { PrintService } from '../../../../actions/print/print.service';
import { FetcherService } from '../../../../services/fetcher/fetcher-service';

interface Row extends TableRow {
  object: PurchaseOrder;
  number: string;
  description: string;
  reference: string;
  totalExVat: string;
  state: string;
  shipmentState: string;
  paymentState: string;
}

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'purchaseorderinvoice-overview-panel',
  templateUrl: './purchaseorderinvoice-overview-panel.component.html',
  providers: [ContextService, PanelService],
})
export class PurchaseOrderInvoiceOverviewPanelComponent extends TestScope {
  internalOrganisation: InternalOrganisation;
  purchaseInvoice: PurchaseInvoice;

  @HostBinding('class.expanded-panel') get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  m: M;

  objects: PurchaseOrder[];
  table: Table<Row>;
  partItem: InvoiceItemType;
  workItem: InvoiceItemType;
  orderItemBillings: OrderItemBilling[];

  delete: Action;
  edit: Action;
  invoice: Action;
  addToInvoice: Action;
  removeFromInvoice: Action;

  get createData(): ObjectData {
    return {
      associationId: this.panel.manager.id,
      associationObjectType: this.panel.manager.objectType,
    };
  }

  constructor(
    @Self() public allors: ContextService,
    @Self() public panel: PanelService,
    public objectService: ObjectService,
    public factoryService: ObjectService,
    public methodService: MethodService,
    public refreshService: RefreshService,
    public navigationService: NavigationService,
    public overviewService: OverviewService,
    public deleteService: DeleteService,
    public printService: PrintService,
    private saveService: SaveService,
    private snackBar: MatSnackBar,
    private fetcher: FetcherService
  ) {
    super();

    this.m = this.allors.context.configuration.metaPopulation as M;

    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.panel.name = 'purchaseorder';
    this.panel.title = 'Purchase Orders';
    this.panel.icon = 'message';
    this.panel.expandable = true;

    this.delete = this.deleteService.delete(this.panel.manager.context);

    this.addToInvoice = {
      name: 'addtoinvoice',
      displayName: () => 'Add to invoice',
      description: () => '',
      disabled: (target: PurchaseOrder) => {
        return !target.canExecuteInvoice;
      },
      execute: (target: PurchaseOrder) => {
        if (!Array.isArray(target)) {
          this.addFromPurchaseOrder(target);
        }
      },
      result: null,
    };

    this.removeFromInvoice = {
      name: 'removefrominvoice',
      displayName: () => 'Remove from invoice',
      description: () => '',
      disabled: (target: PurchaseOrder) => {
        return !this.purchaseInvoice.PurchaseOrders.includes(target);
      },
      execute: (target: PurchaseOrder) => {
        if (!Array.isArray(target)) {
          this.removeFromPurchaseOrder(target);
        }
      },
      result: null,
    };

    // this.invoice.result.subscribe((v) => {
    //   this.table.selection.clear();
    // });

    const sort = true;
    this.table = new Table({
      selection: true,
      columns: [
        { name: 'number', sort },
        { name: 'description', sort },
        { name: 'reference', sort },
        { name: 'totalExVat', sort },
        { name: 'state', sort },
        { name: 'shipmentState', sort },
        { name: 'paymentState', sort },
      ],
      actions: [this.overviewService.overview(), this.printService.print(), this.addToInvoice, this.removeFromInvoice],
      defaultAction: this.overviewService.overview(),
      autoSort: true,
      autoFilter: true,
    });

    if (this.panel.manager.objectType === this.m.PurchaseInvoice) {
      this.table.actions.push(this.delete);
    }

    const pullName = `${this.panel.name}_${this.m.PurchaseOrder.tag}`;
    const invoicePullName = `${this.panel.name}_${this.m.PurchaseInvoice.tag}`;

    this.panel.onPull = (pulls) => {
      const { id } = this.panel.manager;

      pulls.push(
        this.fetcher.internalOrganisation,
        pull.InvoiceItemType({}),
        pull.PurchaseInvoice({
          name: pullName,
          objectId: id,
          select: {
            BilledFrom: {
              Organisation_PurchaseOrdersWhereTakenViaSupplier: {
                include: {
                  PurchaseOrderState: x,
                  PurchaseOrderShipmentState: x,
                  PurchaseOrderPaymentState: x,
                  ValidOrderItems: {
                    PurchaseOrderItem_Part: x,
                    PurchaseOrderItem_SerialisedItem: x,
                  },
                  PrintDocument: {
                    Media: x,
                  },
                },
              },
            },
          },
        }),
        pull.OrderItemBilling({
          predicate: {
            kind: 'ContainedIn',
            propertyType: m.OrderItemBilling.InvoiceItem,
            extent: {
              kind: 'Filter',
              objectType: m.InvoiceItem,
              predicate: {
                kind: 'ContainedIn',
                propertyType: m.InvoiceItem.InvoiceWhereValidInvoiceItem,
                objectIds: [id],
              },
            },
          },
        }),
        pull.PurchaseInvoice({
          name: invoicePullName,
          objectId: id,
          include: {
            PurchaseOrders: x,
            PurchaseInvoiceItems: {
              Part: x,
              InvoiceItemType: x,
              SerialisedItem: x,
            },
          },
        })
      );
    };

    this.panel.onPulled = (loaded) => {
      this.internalOrganisation = this.fetcher.getInternalOrganisation(loaded);
      this.purchaseInvoice = loaded.object<PurchaseInvoice>(invoicePullName);
      this.orderItemBillings = loaded.collection<OrderItemBilling>(m.OrderItemBilling);

      const invoiceItemTypes = loaded.collection<InvoiceItemType>(m.InvoiceItemType);
      this.partItem = invoiceItemTypes?.find((v: InvoiceItemType) => v.UniqueId === 'ff2b943d-57c9-4311-9c56-9ff37959653b');
      this.workItem = invoiceItemTypes?.find((v: InvoiceItemType) => v.UniqueId === 'a4d2e6d0-c6c1-46ec-a1cf-3a64822e7a9e');

      const purchaseOrders = loaded.collection<PurchaseOrder>(pullName);
      this.objects = purchaseOrders.filter(
        (v) =>
          (v.canExecuteInvoice && (this.purchaseInvoice.PurchaseInvoiceState.UniqueId === '102f4080-1d12-4090-9196-f42c094c38ca' || this.purchaseInvoice.PurchaseInvoiceState.UniqueId === '639ba038-d8f3-4672-80b5-c8eb96e3275d')) ||
          v.PurchaseInvoicesWherePurchaseOrder.includes(this.purchaseInvoice)
      );

      if (this.objects) {
        this.table.total = (loaded.value(`${pullName}_total`) ?? this.objects?.length ?? 0) as number;
        this.table.data = this.objects?.map((v) => {
          return {
            object: v,
            number: v.OrderNumber,
            description: v.Description,
            reference: v.CustomerReference,
            totalExVat: v.TotalExVat.toString(),
            state: v.PurchaseOrderState.Name,
            shipmentState: v.PurchaseOrderShipmentState.Name,
            paymentState: v.PurchaseOrderPaymentState.Name,
          } as Row;
        });
      }
    };
  }

  public addFromPurchaseOrder(panelPurchaseOrder: PurchaseOrder): void {
    const purchaseInvoice = this.allors.context.instantiate<PurchaseInvoice>(this.purchaseInvoice.id);
    const purchaseOrder = this.allors.context.instantiate<PurchaseOrder>(panelPurchaseOrder.id);

    purchaseOrder.ValidOrderItems.forEach((purchaseOrderItem: PurchaseOrderItem) => {
      if (purchaseOrderItem.CanInvoice) {
        const invoiceItem = this.allors.context.create<PurchaseInvoiceItem>(this.m.PurchaseInvoiceItem);
        invoiceItem.AssignedUnitPrice = purchaseOrderItem.UnitPrice;
        invoiceItem.Part = purchaseOrderItem.Part;
        invoiceItem.Quantity = purchaseOrderItem.QuantityOrdered;
        invoiceItem.Description = purchaseOrderItem.Description;
        invoiceItem.InternalComment = purchaseOrderItem.InternalComment;
        invoiceItem.Message = purchaseOrderItem.Message;

        if (purchaseOrderItem.SerialisedItem) {
          invoiceItem.SerialisedItem = purchaseOrderItem.SerialisedItem;
        }

        if (invoiceItem.Part) {
          invoiceItem.InvoiceItemType = purchaseOrderItem.InvoiceItemType;
        } else {
          invoiceItem.InvoiceItemType = this.workItem;
        }

        purchaseInvoice.addPurchaseInvoiceItem(invoiceItem);

        const orderItemBilling = this.allors.context.create<OrderItemBilling>(this.m.OrderItemBilling);
        orderItemBilling.Quantity = purchaseOrderItem.QuantityOrdered;
        orderItemBilling.Amount = purchaseOrderItem.TotalBasePrice;
        orderItemBilling.OrderItem = purchaseOrderItem;
        orderItemBilling.InvoiceItem = invoiceItem;
      }
    });

    this.allors.context.push().subscribe(() => {
      this.allors.context.reset();
      this.snackBar.open('Successfully saved.', 'close', { duration: 5000 });
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }

  public removeFromPurchaseOrder(panelPurchaseOrder: PurchaseOrder): void {
    const { context } = this.allors;

    const purchaseOrder = context.instantiate<PurchaseOrder>(panelPurchaseOrder.id);

    purchaseOrder.ValidOrderItems.forEach((purchaseOrderItem: PurchaseOrderItem) => {
      const orderItemBilling = this.orderItemBillings?.find((v) => v.OrderItem.id === purchaseOrderItem.id);
      if (orderItemBilling) {
        context.invoke(orderItemBilling.InvoiceItem.Delete).subscribe(() => {
          context.reset();
          this.refreshService.refresh();
          this.snackBar.open('Successfully removed from invoice.', 'close', { duration: 5000 });
        }, this.saveService.errorHandler);
      }
    });
  }

  public addFromPurchaseOrders(purchaseOrders: PurchaseOrder[]): void {
    purchaseOrders.forEach((element) => {
      this.addFromPurchaseOrder(element);
    });
  }
}
