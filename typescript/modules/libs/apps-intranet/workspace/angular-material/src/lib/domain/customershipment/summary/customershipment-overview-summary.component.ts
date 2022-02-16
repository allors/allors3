import { Component, Self } from '@angular/core';
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
  PanelService,
  ScopedService,
} from '@allors/base/workspace/angular/application';
import { AllorsMaterialPanelService } from '@allors/base/workspace/angular-material/application';
import { IPullResult, Pull } from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import { CustomerShipment, SalesOrder } from '@allors/default/workspace/domain';

@Component({
  selector: 'customershipment-overview-summary',
  templateUrl: './customershipment-overview-summary.component.html',
  providers: [
    ScopedService,
    {
      provide: PanelService,
      useClass: AllorsMaterialPanelService,
    },
  ],
})
export class CustomerShipmentOverviewSummaryComponent extends AllorsViewSummaryPanelComponent {
  m: M;

  shipment: CustomerShipment;
  salesOrders: SalesOrder[] = [];

  constructor(
    @Self() scopedService: ScopedService,
    @Self() panelService: PanelService,
    refreshService: RefreshService,
    sharedPullService: SharedPullService,
    workspaceService: WorkspaceService,
    private snackBar: MatSnackBar,
    private invokeService: InvokeService,
    private errorService: ErrorService
  ) {
    super(scopedService, panelService, sharedPullService, refreshService);
    this.m = workspaceService.workspace.configuration.metaPopulation as M;
  }

  onPreSharedPull(pulls: Pull[], prefix?: string) {
    const {
      m: { pullBuilder: p },
    } = this;

    pulls.push(
      p.Shipment({
        name: prefix,
        objectId: this.scoped.id,
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
        name: `${prefix}2`,
        objectId: this.scoped.id,
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
    this.shipment = loaded.object<CustomerShipment>(prefix);
    this.salesOrders = loaded.collection<SalesOrder>(`${prefix}2`);
  }

  invoice(): void {
    this.invokeService.invoke(this.shipment.Invoice).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully invoiced.', 'close', { duration: 5000 });
    }, this.errorService.errorHandler);
  }

  cancel(): void {
    this.invokeService.invoke(this.shipment.Cancel).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully cancelled.', 'close', {
        duration: 5000,
      });
    }, this.errorService.errorHandler);
  }

  hold(): void {
    this.invokeService.invoke(this.shipment.Hold).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully put on hold.', 'close', {
        duration: 5000,
      });
    }, this.errorService.errorHandler);
  }

  continue(): void {
    this.invokeService.invoke(this.shipment.Continue).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully removed from hold.', 'close', {
        duration: 5000,
      });
    }, this.errorService.errorHandler);
  }

  pick(): void {
    this.invokeService.invoke(this.shipment.Pick).subscribe(() => {
      this.snackBar.open('Successfully picked.', 'close', { duration: 5000 });
      this.refreshService.refresh();
    }, this.errorService.errorHandler);
  }

  ship(): void {
    this.invokeService.invoke(this.shipment.Ship).subscribe(() => {
      this.snackBar.open('Successfully shipped.', 'close', { duration: 5000 });
      this.refreshService.refresh();
    }, this.errorService.errorHandler);
  }
}
