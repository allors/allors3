import { Component, Self } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

import { M } from '@allors/default/workspace/meta';
import {
  PurchaseOrder,
  Shipment,
  PurchaseInvoice,
} from '@allors/default/workspace/domain';
import {
  Action,
  NavigationService,
  PanelService,
  RefreshService,
  ErrorService,
} from '@allors/base/workspace/angular/foundation';

import { PrintService } from '../../../../actions/print/print.service';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';

@Component({
  selector: 'purchaseorder-overview-summary',
  templateUrl: './purchaseorder-overview-summary.component.html',
  providers: [PanelService],
})
export class PurchaseOrderOverviewSummaryComponent {
  m: M;

  order: PurchaseOrder;
  purchaseInvoices: PurchaseInvoice[] = [];

  print: Action;
  shipments: Shipment[];

  constructor(
    @Self() public panel: PanelService,
    public workspaceService: WorkspaceService,

    public navigation: NavigationService,
    public printService: PrintService,
    private errorService: ErrorService,

    public refreshService: RefreshService,
    public snackBar: MatSnackBar
  ) {
    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;

    this.print = printService.print();

    panel.name = 'summary';

    const puchaseOrderPullName = `${panel.name}_${this.m.PurchaseOrder.tag}`;
    const shipmentPullName = `${panel.name}_${this.m.Shipment.tag}`;
    const purchaseInvoicePullName = `${panel.name}_${this.m.PurchaseInvoice.tag}`;

    panel.onPull = (pulls) => {
      const m = this.m;
      const { pullBuilder: pull } = m;
      const x = {};

      pulls.push(
        pull.PurchaseOrder({
          name: puchaseOrderPullName,
          objectId: this.panel.manager.id,
          include: {
            TakenViaSupplier: x,
            PurchaseOrderState: x,
            PurchaseOrderShipmentState: x,
            PurchaseOrderPaymentState: x,
            CreatedBy: x,
            LastModifiedBy: x,
            PrintDocument: {
              Media: x,
            },
          },
        }),
        pull.PurchaseOrder({
          name: shipmentPullName,
          objectId: this.panel.manager.id,
          select: {
            PurchaseOrderItems: {
              OrderShipmentsWhereOrderItem: {
                ShipmentItem: {
                  ShipmentWhereShipmentItem: x,
                },
              },
            },
          },
        }),
        pull.PurchaseOrder({
          name: purchaseInvoicePullName,
          objectId: this.panel.manager.id,
          select: { PurchaseInvoicesWherePurchaseOrder: x },
        })
      );
    };

    panel.onPulled = (loaded) => {
      this.order = loaded.object<PurchaseOrder>(puchaseOrderPullName);
      this.purchaseInvoices = loaded.collection<PurchaseInvoice>(
        purchaseInvoicePullName
      );
      this.shipments = loaded.collection<Shipment>(shipmentPullName);
    };
  }

  public approve(): void {
    this.panel.manager.context.invoke(this.order.Approve).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully approved.', 'close', { duration: 5000 });
    }, this.errorService.errorHandler);
  }

  public cancel(): void {
    this.panel.manager.context.invoke(this.order.Cancel).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully cancelled.', 'close', {
        duration: 5000,
      });
    }, this.errorService.errorHandler);
  }

  public reject(): void {
    this.panel.manager.context.invoke(this.order.Reject).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully rejected.', 'close', { duration: 5000 });
    }, this.errorService.errorHandler);
  }

  public hold(): void {
    this.panel.manager.context.invoke(this.order.Hold).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully put on hold.', 'close', {
        duration: 5000,
      });
    }, this.errorService.errorHandler);
  }

  public continue(): void {
    this.panel.manager.context.invoke(this.order.Continue).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully removed from hold.', 'close', {
        duration: 5000,
      });
    }, this.errorService.errorHandler);
  }

  public setReadyForProcessing(): void {
    this.panel.manager.context
      .invoke(this.order.SetReadyForProcessing)
      .subscribe(() => {
        this.refreshService.refresh();
        this.snackBar.open('Successfully set ready for processing.', 'close', {
          duration: 5000,
        });
      }, this.errorService.errorHandler);
  }

  public reopen(): void {
    this.panel.manager.context.invoke(this.order.Reopen).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully reopened.', 'close', { duration: 5000 });
    }, this.errorService.errorHandler);
  }

  public revise(): void {
    this.panel.manager.context.invoke(this.order.Revise).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully revised.', 'close', { duration: 5000 });
    }, this.errorService.errorHandler);
  }

  public send(): void {
    this.panel.manager.context.invoke(this.order.Send).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully send.', 'close', { duration: 5000 });
    }, this.errorService.errorHandler);
  }

  public invoice(): void {
    this.panel.manager.context.invoke(this.order.Invoice).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully created purchase invoice', 'close', {
        duration: 5000,
      });
    }, this.errorService.errorHandler);
  }

  public quickReceive(): void {
    this.panel.manager.context.invoke(this.order.QuickReceive).subscribe(() => {
      this.panel.toggle();
      this.snackBar.open('inventory created for appropriate items', 'close', {
        duration: 5000,
      });
      this.refreshService.refresh();
    }, this.errorService.errorHandler);
  }
}