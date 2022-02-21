import { Component, Self } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

import {
  Action,
  ErrorService,
  InvokeService,
  MediaService,
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
import { AllorsMaterialPanelService } from '@allors/base/workspace/angular-material/application';
import { IPullResult, Pull } from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  Organisation,
  OrganisationContactKind,
  OrganisationContactRelationship,
  Person,
  ProductQuote,
  PurchaseInvoice,
  PurchaseOrder,
  RequestForQuote,
  SalesOrder,
  User,
} from '@allors/default/workspace/domain';

@Component({
  selector: 'purchaseinvoice-summary-panel',
  templateUrl: './purchaseinvoice-summary-panel.component.html',
  providers: [
    ScopedService,
    {
      provide: PanelService,
      useClass: AllorsMaterialPanelService,
    },
  ],
})
export class PurchasInvoiceSummaryPanelComponent extends AllorsViewSummaryPanelComponent {
  m: M;

  orders: PurchaseOrder[];
  invoice: PurchaseInvoice;

  print: Action;
  orderTotalExVat: number;
  hasIrpf: boolean;
  get totalIrpfIsPositive(): boolean {
    return +this.invoice.TotalIrpf > 0;
  }

  constructor(
    @Self() scopedService: ScopedService,
    @Self() panelService: PanelService,
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
      p.PurchaseInvoice({
        name: prefix,
        objectId: id,
        include: {
          PurchaseInvoiceItems: {
            InvoiceItemType: {},
          },
          BilledFrom: {},
          BilledFromContactPerson: {},
          ShipToCustomer: {},
          BillToEndCustomer: {},
          BillToEndCustomerContactPerson: {},
          ShipToEndCustomer: {},
          ShipToEndCustomerContactPerson: {},
          PurchaseInvoiceState: {},
          CreatedBy: {},
          LastModifiedBy: {},
          DerivedBillToEndCustomerContactMechanism: {
            PostalAddress_Country: {},
          },
          DerivedShipToEndCustomerAddress: {
            Country: {},
          },
          PrintDocument: {
            Media: {},
          },
        },
      }),
      p.PurchaseInvoice({
        name: `${prefix}_purchaseOrder`,
        objectId: id,
        select: {
          PurchaseOrders: {},
        },
      })
    );
  }

  onPostSharedPull(loaded: IPullResult, prefix?: string) {
    this.invoice = loaded.object<PurchaseInvoice>(prefix);
    this.orders = loaded.collection<PurchaseOrder>(`${prefix}_purchaseOrder`);

    this.orderTotalExVat = this.orders?.reduce(
      (partialOrderTotal, order) =>
        partialOrderTotal +
        order.ValidOrderItems?.reduce(
          (partialItemTotal, item) =>
            partialItemTotal + parseFloat(item.TotalExVat),
          0
        ),
      0
    );

    this.hasIrpf = Number(this.invoice.TotalIrpf) !== 0;
  }

  public confirm(): void {
    this.invokeService.invoke(this.invoice.Confirm).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully confirmed.', 'close', {
        duration: 5000,
      });
    }, this.errorService.errorHandler);
  }

  public cancel(): void {
    this.invokeService.invoke(this.invoice.Cancel).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully cancelled.', 'close', {
        duration: 5000,
      });
    }, this.errorService.errorHandler);
  }

  public reopen(): void {
    this.invokeService.invoke(this.invoice.Reopen).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully reopened.', 'close', { duration: 5000 });
    }, this.errorService.errorHandler);
  }

  public revise(): void {
    this.invokeService.invoke(this.invoice.Revise).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully reopened.', 'close', { duration: 5000 });
    }, this.errorService.errorHandler);
  }

  public finishRevising(): void {
    this.invokeService.invoke(this.invoice.FinishRevising).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully finished revising.', 'close', {
        duration: 5000,
      });
    }, this.errorService.errorHandler);
  }

  public approve(): void {
    this.invokeService.invoke(this.invoice.Approve).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully approved.', 'close', { duration: 5000 });
    }, this.errorService.errorHandler);
  }

  public reject(): void {
    this.invokeService.invoke(this.invoice.Reject).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully rejected.', 'close', { duration: 5000 });
    }, this.errorService.errorHandler);
  }

  public createSalesInvoice(invoice: PurchaseInvoice): void {
    this.invokeService.invoke(invoice.CreateSalesInvoice).subscribe(() => {
      this.snackBar.open('Successfully created a sales invoice.', 'close', {
        duration: 5000,
      });
      this.refreshService.refresh();
    }, this.errorService.errorHandler);
  }
}
