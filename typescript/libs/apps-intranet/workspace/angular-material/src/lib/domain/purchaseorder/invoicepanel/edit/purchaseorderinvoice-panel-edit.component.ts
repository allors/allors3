import { Component, HostBinding, OnInit } from '@angular/core';
import { RoleType } from '@allors/system/workspace/meta';
import {
  IObject,
  IPullResult,
  Pull,
  Initializer,
  SharedPullHandler,
} from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  AllorsCustomEditExtentPanelComponent,
  NavigationService,
  PanelService,
  ScopedService,
} from '@allors/base/workspace/angular/application';
import {
  Action,
  DisplayService,
  ErrorService,
  InvokeService,
  RefreshService,
  SharedPullService,
  Table,
  TableConfig,
  TableRow,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { PeriodSelection } from '@allors/base/workspace/angular-material/foundation';
import {
  IconService,
  ViewActionService,
} from '@allors/base/workspace/angular-material/application';
import {
  InvoiceItemType,
  PurchaseOrder,
  PurchaseInvoice,
  OrderItemBilling,
  PurchaseOrderItem,
  PurchaseInvoiceItem,
} from '@allors/default/workspace/domain';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PrintService } from '../../../../actions/print/print.service';

interface Row extends TableRow {
  object: IObject;
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
  selector: 'purchaseorderinvoice-panel-edit',
  templateUrl: './purchaseorderinvoice-panel-edit.component.html',
})
export class PurchaseOrderInvoicePanelEditComponent
  extends AllorsCustomEditExtentPanelComponent
  implements SharedPullHandler, OnInit
{
  override readonly panelKind = 'Extent';

  override readonly panelMode = 'Edit';
  initialState: boolean;

  override get panelId() {
    return this.m?.PurchaseOrder.tag;
  }

  @HostBinding('class.expanded-panel')
  get expandedPanelClass() {
    return true;
    // return this.panel.isExpanded;
  }

  get icon() {
    return this.iconService.icon(this.m.PurchaseOrder);
  }

  get initializer(): Initializer {
    return null;
    // TODO: Martien
    // return { propertyType: this.init, id: this.scoped.id };
  }

  title = 'Purchase Orders';

  m: M;

  periodSelection: PeriodSelection = PeriodSelection.Current;

  table: Table<Row>;
  view: Action;
  invoice: Action;
  addToInvoice: Action;
  removeFromInvoice: Action;
  objects: PurchaseOrder[] = [];
  display: RoleType[];
  purchaseInvoice: PurchaseInvoice;
  partItem: InvoiceItemType;
  workItem: InvoiceItemType;
  orderItemBillings: OrderItemBilling[];
  orderPullName: string;
  invoicePullName: string;

  constructor(
    scopedService: ScopedService,
    panelService: PanelService,
    sharedPullService: SharedPullService,
    refreshService: RefreshService,
    private workspaceService: WorkspaceService,
    public navigation: NavigationService,
    public viewService: ViewActionService,

    public printService: PrintService,

    private iconService: IconService,
    private displayService: DisplayService,
    private snackBar: MatSnackBar,
    private errorService: ErrorService,
    private invokeService: InvokeService
  ) {
    super(scopedService, panelService, sharedPullService, refreshService);
    this.m = workspaceService.workspace.configuration.metaPopulation as M;

    panelService.register(this);
    sharedPullService.register(this);
  }

  ngOnInit() {
    this.display = this.displayService.primary(this.m.PurchaseOrder);

    this.view = this.viewService.view();

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
        return (
          !this.initialState ||
          !this.purchaseInvoice.PurchaseOrders.includes(target)
        );
      },
      execute: (target: PurchaseOrder) => {
        if (!Array.isArray(target)) {
          this.removeFromPurchaseOrder(target);
        }
      },
      result: null,
    };

    const tableConfig: TableConfig = {
      selection: true,
      columns: [
        { name: 'number' },
        { name: 'description' },
        { name: 'reference' },
        { name: 'totalExVat' },
        { name: 'state' },
        { name: 'shipmentState' },
        { name: 'paymentState' },
      ],
      actions: [
        this.view,
        this.printService.print(),
        this.addToInvoice,
        this.removeFromInvoice,
      ],
      defaultAction: this.view,
      autoSort: true,
      autoFilter: true,
    };

    this.table = new Table(tableConfig);
  }

  onPreSharedPull(pulls: Pull[], prefix?: string) {
    const { m } = this;
    const { pullBuilder: p } = m;

    this.orderPullName = `${prefix}_order`;
    this.invoicePullName = `${prefix}_invoice`;

    if (this.panelEnabled) {
      const id = this.scoped.id;

      pulls.push(
        p.InvoiceItemType({}),
        p.PurchaseInvoice({
          name: this.orderPullName,
          objectId: id,
          select: {
            BilledFrom: {
              PurchaseOrdersWhereTakenViaSupplier: {
                include: {
                  PurchaseOrderState: {},
                  PurchaseOrderShipmentState: {},
                  PurchaseOrderPaymentState: {},
                  ValidOrderItems: {
                    PurchaseOrderItem_Part: {},
                    PurchaseOrderItem_SerialisedItem: {},
                  },
                  PrintDocument: {
                    Media: {},
                  },
                },
              },
            },
          },
        }),
        p.PurchaseInvoice({
          name: this.invoicePullName,
          objectId: id,
          include: {
            PurchaseInvoiceState: {},
            PurchaseOrders: {},
            PurchaseInvoiceItems: {
              Part: {},
              InvoiceItemType: {},
              SerialisedItem: {},
            },
          },
        }),
        p.OrderItemBilling({
          predicate: {
            kind: 'And',
            operands: [
              {
                kind: 'ContainedIn',
                propertyType: this.m.OrderItemBilling.InvoiceItem,
                extent: {
                  kind: 'Filter',
                  objectType: this.m.InvoiceItem,
                  predicate: {
                    kind: 'ContainedIn',
                    propertyType:
                      this.m.InvoiceItem.InvoiceWhereValidInvoiceItem,
                    objectIds: [id],
                  },
                },
              },
            ],
          },
        })
      );
    }
  }

  onPostSharedPull(pullResult: IPullResult, prefix?: string) {
    if (this.panelEnabled) {
      this.enabled = this.enabler ? this.enabler() : true;

      this.purchaseInvoice = pullResult.object<PurchaseInvoice>(
        this.invoicePullName
      );

      this.orderItemBillings = pullResult.collection<OrderItemBilling>(
        this.m.OrderItemBilling
      );

      const invoiceItemTypes = pullResult.collection<InvoiceItemType>(
        this.m.InvoiceItemType
      );

      this.initialState =
        this.purchaseInvoice?.PurchaseInvoiceState.UniqueId ===
          '639ba038-d8f3-4672-80b5-c8eb96e3275d' ||
        this.purchaseInvoice?.PurchaseInvoiceState.UniqueId ===
          '102f4080-1d12-4090-9196-f42c094c38ca';

      this.partItem = invoiceItemTypes?.find(
        (v: InvoiceItemType) =>
          v.UniqueId === 'ff2b943d-57c9-4311-9c56-9ff37959653b'
      );

      this.workItem = invoiceItemTypes?.find(
        (v: InvoiceItemType) =>
          v.UniqueId === 'a4d2e6d0-c6c1-46ec-a1cf-3a64822e7a9e'
      );

      const purchaseOrders = pullResult.collection<PurchaseOrder>(
        this.orderPullName
      );

      this.objects = purchaseOrders?.filter(
        (v) =>
          (v.canExecuteInvoice && this.initialState) ||
          v.PurchaseInvoicesWherePurchaseOrder.includes(this.purchaseInvoice)
      );

      this.table.data = this.objects?.map((v) => {
        const row: Row = {
          object: v,
          number: v.OrderNumber,
          description: v.Description,
          reference: v.CustomerReference,
          totalExVat: v.TotalExVat.toString(),
          state: v.PurchaseOrderState.Name,
          shipmentState: v.PurchaseOrderShipmentState.Name,
          paymentState: v.PurchaseOrderPaymentState.Name,
        };
        return row;
      });
    }
  }

  toggle() {
    this.panelService.stopEdit().subscribe();
  }

  public addFromPurchaseOrder(panelPurchaseOrder: PurchaseOrder): void {
    const context = this.workspaceService.contextBuilder();

    const purchaseInvoice = context.instantiate(this.purchaseInvoice);
    const purchaseOrder = context.instantiate(panelPurchaseOrder);
    const workItem = context.instantiate(this.workItem);

    purchaseOrder.ValidOrderItems.forEach(
      (purchaseOrderItem: PurchaseOrderItem) => {
        if (purchaseOrderItem.CanInvoice) {
          const invoiceItem = context.create<PurchaseInvoiceItem>(
            this.m.PurchaseInvoiceItem
          );

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
            invoiceItem.InvoiceItemType = workItem;
          }

          purchaseInvoice.addPurchaseInvoiceItem(invoiceItem);

          const orderItemBilling = context.create<OrderItemBilling>(
            this.m.OrderItemBilling
          );

          orderItemBilling.Quantity = purchaseOrderItem.QuantityOrdered;
          orderItemBilling.Amount = purchaseOrderItem.TotalBasePrice;
          orderItemBilling.OrderItem = purchaseOrderItem;
          orderItemBilling.InvoiceItem = invoiceItem;
        }
      }
    );

    context.push().subscribe(() => {
      context.reset();
      this.snackBar.open('Successfully saved.', 'close', { duration: 5000 });
      this.refreshService.refresh();
    }, this.errorService.errorHandler);
  }

  public removeFromPurchaseOrder(panelPurchaseOrder: PurchaseOrder): void {
    const context = this.workspaceService.contextBuilder();

    const purchaseOrder = context.instantiate(
      panelPurchaseOrder.id
    ) as PurchaseOrder;

    const methods = [];

    purchaseOrder.ValidOrderItems.forEach(
      (purchaseOrderItem: PurchaseOrderItem) => {
        const orderItemBilling = this.orderItemBillings.find(
          (v) => v.OrderItem.id === purchaseOrderItem.id
        );
        if (orderItemBilling) {
          methods.push(orderItemBilling.InvoiceItem.Delete);
        }
      }
    );

    this.invokeService.invoke(methods).subscribe(() => {
      context.reset();
      this.refreshService.refresh();
      this.snackBar.open('Successfully removed from invoice.', 'close', {
        duration: 5000,
      });
    }, this.errorService.errorHandler);
  }

  public addFromPurchaseOrders(purchaseOrders: PurchaseOrder[]): void {
    purchaseOrders.forEach((element) => {
      this.addFromPurchaseOrder(element);
    });
  }
}
