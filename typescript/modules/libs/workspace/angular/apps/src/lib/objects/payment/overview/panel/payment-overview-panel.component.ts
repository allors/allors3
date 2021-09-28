import { Component, Self, HostBinding } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { format } from 'date-fns';

import { M } from '@allors/workspace/meta/default';
import { SalesInvoice, PurchaseInvoice, Payment, Invoice } from '@allors/workspace/domain/default';
import { Action, DeleteService, EditService, MethodService, NavigationService, ObjectData, ObjectService, PanelService, RefreshService, Table, TableRow, TestScope } from '@allors/workspace/angular/base';
import { SessionService } from '@allors/workspace/angular/core';

interface Row extends TableRow {
  object: Payment;
  date: string;
  amount: string;
}

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'payment-overview-panel',
  templateUrl: './payment-overview-panel.component.html',
  providers: [SessionService, PanelService],
})
export class PaymentOverviewPanelComponent extends TestScope {
  @HostBinding('class.expanded-panel') get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  m: M;

  payments: Payment[];
  table: Table<Row>;
  receive: boolean;

  delete: Action;
  edit: Action;

  get createData(): ObjectData {
    return {
      associationId: this.panel.manager.id,
      associationObjectType: this.panel.manager.objectType,
    };
  }

  constructor(
    @Self() public allors: SessionService,
    @Self() public panel: PanelService,
    public objectService: ObjectService,

    public refreshService: RefreshService,
    public navigation: NavigationService,
    public methodService: MethodService,
    public deleteService: DeleteService,
    public editService: EditService,
    public snackBar: MatSnackBar
  ) {
    super();

    this.m = this.allors.workspace.configuration.metaPopulation as M;

    panel.name = 'payment';
    panel.title = 'Payments';
    panel.icon = 'money';
    panel.expandable = true;

    this.delete = deleteService.delete(panel.manager.session);
    this.edit = editService.edit();

    this.table = new Table({
      selection: true,
      columns: [{ name: 'date' }, { name: 'amount' }],
      actions: [this.edit, this.delete],
      defaultAction: this.edit,
      autoSort: true,
      autoFilter: true,
    });

    const pullName = `${panel.name}_${this.m.Payment.tag}`;

    panel.onPull = (pulls) => {
      const m = this.m;  const { pullBuilder: pull } = m; const x = {};

      const id = this.panel.manager.id;

      pulls.push(
        pull.PaymentApplication({
          name: pullName,
          predicate: { kind: 'Equals', propertyType: m.PaymentApplication.Invoice, objectId: id },
          select: {
            PaymentWherePaymentApplication: {
              include: {
                Sender: x,
                PaymentMethod: x,
              },
            },
          },
        }),
        pull.Invoice({
          object: this.panel.manager.id,
          include: {
            SalesInvoice_SalesInvoiceType: x,
            PurchaseInvoice_PurchaseInvoiceType: x,
          },
        })
      );
    };

    panel.onPulled = (loaded) => {
      const invoice = loaded.object<Invoice>(m.Invoice);

      if (invoice.objectType.name === this.m.SalesInvoice.name) {
        const salesInvoice = invoice as SalesInvoice;
        this.receive = salesInvoice.SalesInvoiceType.UniqueId === '92411bf1-835e-41f8-80af-6611efce5b32';
      }

      if (invoice.objectType.name === this.m.PurchaseInvoice.name) {
        const salesInvoice = invoice as PurchaseInvoice;
        this.receive = salesInvoice.PurchaseInvoiceType.UniqueId === '0187d927-81f5-4d6a-9847-58b674ad3e6a';
      }

      this.payments = loaded.collection<Payment>(pullName);

      this.table.total = loaded.values[`${pullName}_total`] || this.payments.length;
      this.table.data = this.payments.map((v) => {
        return {
          object: v,
          date: v.EffectiveDate && format(new Date(v.EffectiveDate), 'dd-MM-yyyy'),
          amount: v.Amount,
        } as Row;
      });
    };
  }
}
