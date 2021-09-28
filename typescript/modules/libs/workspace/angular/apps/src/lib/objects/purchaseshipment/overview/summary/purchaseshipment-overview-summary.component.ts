import { Component, Self } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

import { MetaService, NavigationService, PanelService, RefreshService,  Invoked } from '@allors/angular/services/core';
import { PurchaseOrder, PurchaseShipment, ShipmentItem } from '@allors/domain/generated';
import { Meta } from '@allors/meta/generated';
import { SaveService } from '@allors/angular/material/services/core';
import { PrintService } from '@allors/angular/base';
import { ActionTarget } from '@allors/angular/core';


@Component({
  // tslint:disable-next-line:component-selector
  selector: 'purchaseshipment-overview-summary',
  templateUrl: './purchaseshipment-overview-summary.component.html',
  providers: [PanelService]
})
export class PurchaseShipmentOverviewSummaryComponent {

  m: M;

  shipment: PurchaseShipment;
  purchaseOrders: PurchaseOrder[] = [];
  shipmentItems: ShipmentItem[] = [];

  constructor(
    @Self() public panel: PanelService,
    
    public navigation: NavigationService,
    public printService: PrintService,
    public refreshService: RefreshService,
    private saveService: SaveService,
    public snackBar: MatSnackBar) {

    this.m = this.allors.workspace.configuration.metaPopulation as M;

    panel.name = 'summary';

    const shipmentPullName = `${panel.name}_${this.m.Shipment.name}`;

    panel.onPull = (pulls) => {
      const m = this.m; const { pullBuilder: pull } = m; const x = {};

      pulls.push(

        pull.Shipment({
          name: shipmentPullName,
          object: this.panel.manager.id,
          include: {
            ShipmentItems: {
              Good: x,
              Part: x
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
          }
        }),
        pull.Shipment({
          object: this.panel.manager.id,
          select: {
            ShipmentItems: {
              OrderShipmentsWhereShipmentItem: {
                OrderItem: {
                  OrderWhereValidOrderItem: x
                }
              }
            }
          }
        }),
      );
    };

    panel.onPulled = (loaded) => {
      this.shipment = loaded.objects[shipmentPullName] as PurchaseShipment;
      this.shipmentItems = loaded.collections[shipmentPullName] as ShipmentItem[];
      this.purchaseOrders = loaded.collection<PurchaseOrder>(m.PurchaseOrder);
    };
  }

  public receive(): void {

    this.panel.manager.context.invoke(this.shipment.Receive)
      .subscribe(() => {
        this.panel.toggle();
        this.snackBar.open('Successfully received.', 'close', { duration: 5000 });
        this.refreshService.refresh();
      },
      this.saveService.errorHandler);
  }
}
