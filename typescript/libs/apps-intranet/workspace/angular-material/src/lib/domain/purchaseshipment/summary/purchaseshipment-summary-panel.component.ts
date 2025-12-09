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
  PurchaseOrder,
  PurchaseShipment,
} from '@allors/default/workspace/domain';

@Component({
  selector: 'purchaseshipment-summary-panel',
  templateUrl: './purchaseshipment-summary-panel.component.html',
})
export class PurchaseShipmentSummaryPanelComponent extends AllorsViewSummaryPanelComponent {
  m: M;

  shipment: PurchaseShipment;
  purchaseOrders: PurchaseOrder[] = [];

  constructor(
    scopedService: ScopedService,
    panelService: PanelService,
    refreshService: RefreshService,
    sharedPullService: SharedPullService,
    workspaceService: WorkspaceService,
    private snackBar: MatSnackBar,
    private invokeService: InvokeService,
    private errorService: ErrorService,
    public navigation: NavigationService
  ) {
    super(scopedService, panelService, sharedPullService, refreshService);
    this.m = workspaceService.workspace.configuration.metaPopulation as M;
  }

  onPreSharedPull(pulls: Pull[], prefix?: string) {
    const {
      m: { pullBuilder: p },
    } = this;

    const id = this.scoped.id;

    pulls.push(
      p.Shipment({
        name: prefix,
        objectId: id,
        include: {
          ShipmentItems: {
            Good: {},
            Part: {},
          },
          ShipFromParty: {},
          ShipFromContactPerson: {},
          ShipToParty: {},
          ShipToContactPerson: {},
          ShipmentState: {},
          CreatedBy: {},
          LastModifiedBy: {},
          ShipToAddress: {
            Country: {},
          },
        },
      }),
      p.Shipment({
        name: `${prefix}_orders`,
        objectId: id,
        select: {
          ShipmentItems: {
            OrderShipmentsWhereShipmentItem: {
              OrderItem: {
                OrderWhereValidOrderItem: {},
              },
            },
          },
        },
      })
    );
  }

  onPostSharedPull(loaded: IPullResult, prefix?: string) {
    this.shipment = loaded.object<PurchaseShipment>(prefix);
    this.purchaseOrders = loaded.collection<PurchaseOrder>(`${prefix}_orders`);
  }

  public receive(): void {
    this.invokeService.invoke(this.shipment.Receive).subscribe(() => {
      this.snackBar.open('Successfully received.', 'close', { duration: 5000 });
      this.refreshService.refresh();
    }, this.errorService.errorHandler);
  }
}
