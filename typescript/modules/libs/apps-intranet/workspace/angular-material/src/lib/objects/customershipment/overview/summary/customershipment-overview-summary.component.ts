import { Component, Self } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

import { M } from '@allors/default/workspace/meta';
import { CustomerShipment, SalesOrder } from '@allors/default/workspace/domain';
import {
  NavigationService,
  PanelService,
  RefreshService,
  SaveService,
} from '@allors/workspace/angular/base';

import { PrintService } from '../../../../actions/print/print.service';
import { WorkspaceService } from '@allors/workspace/angular/core';

@Component({
  selector: 'customershipment-overview-summary',
  templateUrl: './customershipment-overview-summary.component.html',
  providers: [PanelService],
})
export class CustomerShipmentOverviewSummaryComponent {
  m: M;

  shipment: CustomerShipment;
  salesOrders: SalesOrder[] = [];

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

    panel.name = 'summary';

    const shipmentPullName = `${panel.name}_${this.m.Shipment.tag}`;

    panel.onPull = (pulls) => {
      const m = this.m;
      const { pullBuilder: pull } = m;
      const x = {};

      pulls.push(
        pull.Shipment({
          name: shipmentPullName,
          objectId: this.panel.manager.id,
          include: {
            ShipmentItems: {
              Good: x,
              Part: x,
            },
            ShipFromParty: x,
            ShipFromContactPerson: x,
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
        pull.Shipment({
          objectId: this.panel.manager.id,
          select: {
            ShipmentItems: {
              OrderShipmentsWhereShipmentItem: {
                OrderItem: {
                  OrderWhereValidOrderItem: x,
                },
              },
            },
          },
        })
      );
    };

    panel.onPulled = (loaded) => {
      this.shipment = loaded.object<CustomerShipment>(shipmentPullName);
      this.salesOrders = loaded.collection<SalesOrder>(
        this.m.OrderItem.OrderWhereValidOrderItem
      );
    };
  }

  public invoice(): void {
    this.panel.manager.context.invoke(this.shipment.Invoice).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully invoiced.', 'close', { duration: 5000 });
    }, this.saveService.errorHandler);
  }

  public cancel(): void {
    this.panel.manager.context.invoke(this.shipment.Cancel).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully cancelled.', 'close', {
        duration: 5000,
      });
    }, this.saveService.errorHandler);
  }

  public hold(): void {
    this.panel.manager.context.invoke(this.shipment.Hold).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully put on hold.', 'close', {
        duration: 5000,
      });
    }, this.saveService.errorHandler);
  }

  public continue(): void {
    this.panel.manager.context.invoke(this.shipment.Continue).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully removed from hold.', 'close', {
        duration: 5000,
      });
    }, this.saveService.errorHandler);
  }

  public pick(): void {
    this.panel.manager.context.invoke(this.shipment.Pick).subscribe(() => {
      this.panel.toggle();
      this.snackBar.open('Successfully picked.', 'close', { duration: 5000 });
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }

  public ship(): void {
    this.panel.manager.context.invoke(this.shipment.Ship).subscribe(() => {
      this.panel.toggle();
      this.snackBar.open('Successfully shipped.', 'close', { duration: 5000 });
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }
}
