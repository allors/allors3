import { Component, Self } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

import { M } from '@allors/default/workspace/meta';
import {
  WorkEffort,
  SalesOrder,
  SalesInvoice,
  RepeatingSalesInvoice,
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
  selector: 'salesinvoice-overview-summary',
  templateUrl: './salesinvoice-overview-summary.component.html',
  providers: [PanelService],
})
export class SalesInvoiceOverviewSummaryComponent {
  m: M;

  invoice: SalesInvoice;
  orders: SalesOrder[];
  repeatingInvoices: RepeatingSalesInvoice[];
  repeatingInvoice: RepeatingSalesInvoice;
  print: Action;
  workEfforts: WorkEffort[];
  public hasIrpf: boolean;
  creditNote: SalesInvoice;

  get totalIrpfIsPositive(): boolean {
    return +this.invoice.TotalIrpf > 0;
  }

  constructor(
    @Self() public panel: PanelService,
    public workspaceService: WorkspaceService,
    public navigation: NavigationService,
    public printService: PrintService,
    public refreshService: RefreshService,
    private errorService: ErrorService,
    public snackBar: MatSnackBar
  ) {
    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;
    const m = this.m;

    this.print = printService.print();

    panel.name = 'summary';

    const salesInvoicePullName = `${panel.name}_${this.m.PurchaseInvoice.tag}`;
    const salesOrderPullName = `${panel.name}_${this.m.PurchaseOrder.tag}`;
    const workEffortPullName = `${panel.name}_${this.m.WorkEffort.tag}`;
    const repeatingSalesInvoicePullName = `${panel.name}_${this.m.Good.tag}`;
    const creditNotePullName = `${panel.name}_${this.m.SalesInvoice.tag}`;

    panel.onPull = (pulls) => {
      const { pullBuilder: pull } = m;
      const x = {};

      const { id } = this.panel.manager;

      pulls.push(
        pull.SalesInvoice({
          name: salesInvoicePullName,
          objectId: id,
          include: {
            SalesInvoiceItems: {
              Product: x,
              InvoiceItemType: x,
            },
            SalesTerms: {
              TermType: x,
            },
            PrintDocument: {
              Media: x,
            },
            CreditedFromInvoice: x,
            BillToCustomer: x,
            BillToContactPerson: x,
            ShipToCustomer: x,
            ShipToContactPerson: x,
            ShipToEndCustomer: x,
            ShipToEndCustomerContactPerson: x,
            SalesInvoiceState: x,
            CreatedBy: x,
            LastModifiedBy: x,
            DerivedBillToContactMechanism: {
              PostalAddress_Country: x,
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
          },
        }),
        pull.SalesInvoice({
          name: salesOrderPullName,
          objectId: id,
          select: {
            SalesOrders: x,
          },
        }),
        pull.SalesInvoice({
          name: workEffortPullName,
          objectId: id,
          select: {
            WorkEfforts: x,
          },
        }),
        pull.SalesInvoice({
          name: creditNotePullName,
          objectId: id,
          select: {
            SalesInvoiceWhereCreditedFromInvoice: x,
          },
        }),
        pull.RepeatingSalesInvoice({
          name: repeatingSalesInvoicePullName,
          predicate: {
            kind: 'Equals',
            propertyType: m.RepeatingSalesInvoice.Source,
            value: id,
          },
          include: {
            Frequency: x,
            DayOfWeek: x,
          },
        })
      );
    };

    panel.onPulled = (loaded) => {
      this.orders = loaded.collection<SalesOrder>(salesOrderPullName);
      this.workEfforts = loaded.collection<WorkEffort>(workEffortPullName);
      this.invoice = loaded.object<SalesInvoice>(m.SalesInvoice);
      this.repeatingInvoices = loaded.collection<RepeatingSalesInvoice>(
        m.RepeatingSalesInvoice
      );
      this.hasIrpf = Number(this.invoice.TotalIrpf) !== 0;
      this.creditNote = loaded.object<SalesInvoice>(creditNotePullName);

      if (this.repeatingInvoices) {
        this.repeatingInvoice = this.repeatingInvoices[0];
      } else {
        this.repeatingInvoice = undefined;
      }
    };
  }

  send() {
    this.panel.manager.context.invoke(this.invoice.Send).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully sent.', 'close', { duration: 5000 });
    }, this.errorService.errorHandler);
  }

  public cancel(): void {
    this.panel.manager.context
      .invoke(this.invoice.CancelInvoice)
      .subscribe(() => {
        this.refreshService.refresh();
        this.snackBar.open('Successfully cancelled.', 'close', {
          duration: 5000,
        });
      }, this.errorService.errorHandler);
  }

  public writeOff(): void {
    this.panel.manager.context.invoke(this.invoice.WriteOff).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully written off.', 'close', {
        duration: 5000,
      });
    }, this.errorService.errorHandler);
  }

  public reopen(): void {
    this.panel.manager.context.invoke(this.invoice.Reopen).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully Reopened.', 'close', { duration: 5000 });
    }, this.errorService.errorHandler);
  }

  public credit(): void {
    this.panel.manager.context.invoke(this.invoice.Credit).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully Credited.', 'close', { duration: 5000 });
    }, this.errorService.errorHandler);
  }

  public revise(): void {
    this.panel.manager.context.invoke(this.invoice.Revise).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully Reopened.', 'close', { duration: 5000 });
    }, this.errorService.errorHandler);
  }

  public copy(): void {
    this.panel.manager.context.invoke(this.invoice.Copy).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully copied.', 'close', { duration: 5000 });
    }, this.errorService.errorHandler);
  }
}