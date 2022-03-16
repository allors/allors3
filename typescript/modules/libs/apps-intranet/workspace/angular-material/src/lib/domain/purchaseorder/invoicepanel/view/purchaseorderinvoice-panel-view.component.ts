import { Component, HostBinding, OnInit } from '@angular/core';
import { RoleType } from '@allors/system/workspace/meta';
import {
  IPullResult,
  Pull,
  Initializer,
  SharedPullHandler,
} from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  PurchaseInvoice,
  PurchaseOrder,
} from '@allors/default/workspace/domain';
import {
  AllorsCustomViewExtentPanelComponent,
  PanelService,
  ScopedService,
} from '@allors/base/workspace/angular/application';
import {
  DisplayService,
  RefreshService,
  SharedPullService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { IconService } from '@allors/base/workspace/angular-material/application';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'purchaseorderinvoice-panel-view',
  templateUrl: './purchaseorderinvoice-panel-view.component.html',
})
export class PurchaseOrderInvoicePanelViewComponent
  extends AllorsCustomViewExtentPanelComponent
  implements SharedPullHandler, OnInit
{
  override readonly panelKind = 'Extent';
  orderPullName: string;
  invoicePullName: string;

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

  objects: PurchaseOrder[];

  display: RoleType[];
  description: string;

  constructor(
    scopedService: ScopedService,
    panelService: PanelService,
    sharedPullService: SharedPullService,
    refreshService: RefreshService,
    workspaceService: WorkspaceService,
    private displayService: DisplayService,
    private iconService: IconService
  ) {
    super(scopedService, panelService, sharedPullService, refreshService);

    this.m = workspaceService.workspace.configuration.metaPopulation as M;
    panelService.register(this);
    sharedPullService.register(this);
  }

  ngOnInit() {
    this.display = this.displayService.primary(this.m.PurchaseOrder);
  }

  onPreSharedPull(pulls: Pull[], prefix?: string) {
    const { m } = this;
    const { pullBuilder: p } = m;

    this.orderPullName = `${prefix}_order`;
    this.invoicePullName = `${prefix}_invoice`;

    if (this.panelEnabled) {
      const id = this.scoped.id;

      pulls.push(
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
                },
              },
            },
          },
        }),
        p.PurchaseInvoice({
          name: this.invoicePullName,
          objectId: id,
          include: {
            PurchaseOrders: {},
            PurchaseInvoiceItems: {
              Part: {},
              InvoiceItemType: {},
              SerialisedItem: {},
            },
          },
        })
      );
    }
  }

  onPostSharedPull(pullResult: IPullResult, prefix?: string) {
    this.enabled = this.enabler ? this.enabler() : true;

    const purchaseInvoice = pullResult.object<PurchaseInvoice>(
      this.invoicePullName
    );

    const purchaseOrders = pullResult.collection<PurchaseOrder>(
      this.orderPullName
    );

    this.objects = purchaseOrders.filter(
      (v) =>
        (v.canExecuteInvoice &&
          (purchaseInvoice.PurchaseInvoiceState.UniqueId ===
            '102f4080-1d12-4090-9196-f42c094c38ca' ||
            purchaseInvoice.PurchaseInvoiceState.UniqueId ===
              '639ba038-d8f3-4672-80b5-c8eb96e3275d')) ||
        v.PurchaseInvoicesWherePurchaseOrder.includes(purchaseInvoice)
    );

    this.description = `${this.objects.length} payments`;
  }

  toggle() {
    this.panelService.startEdit(this.panelId).subscribe();
  }
}
