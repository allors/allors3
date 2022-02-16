import { Component, Self, HostBinding } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

import { M } from '@allors/default/workspace/meta';
import {
  Invoice,
  Order,
  OrderAdjustment,
  Quote,
} from '@allors/default/workspace/domain';
import {
  Action,
  DeleteService,
  EditService,
  NavigationService,
  ObjectData,
  ScopedService,
  OldPanelService,
  RefreshService,
  Table,
  TableRow,
} from '@allors/base/workspace/angular/foundation';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';

interface Row extends TableRow {
  object: OrderAdjustment;
  adjustment: string;
  amount: string;
  percentage: string;
}

@Component({
  selector: 'orderadjustment-overview-panel',
  templateUrl: './orderadjustment-overview-panel.component.html',
  providers: [OldPanelService],
})
export class OrderAdjustmentOverviewPanelComponent {
  container: Quote | Order | Invoice;

  @HostBinding('class.expanded-panel') get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  m: M;

  objects: OrderAdjustment[];
  table: Table<Row>;

  delete: Action;
  edit: Action;

  get createData(): ObjectData {
    return {
      associationId: this.panel.manager.id,
      associationObjectType: this.panel.manager.objectType,
      associationRoleType: this.containerRoleType,
    };
  }

  get containerRoleType(): any {
    if (
      this.container.strategy.cls === this.m.ProductQuote ||
      this.container.strategy.cls === this.m.Proposal ||
      this.container.strategy.cls === this.m.StatementOfWork
    ) {
      return this.m.Quote.OrderAdjustments;
    } else if (this.container.strategy.cls === this.m.SalesOrder) {
      return this.m.SalesOrder.OrderAdjustments;
    } else if (
      this.container.strategy.cls === this.m.SalesInvoice ||
      this.container.strategy.cls === this.m.PurchaseInvoice
    ) {
      return this.m.Invoice.OrderAdjustments;
    }
  }

  constructor(
    @Self() public panel: OldPanelService,
    public workspaceService: WorkspaceService,
    public scopedService: ScopedService,
    public refreshService: RefreshService,
    public navigation: NavigationService,
    public editService: EditService,
    public deleteService: DeleteService,
    public snackBar: MatSnackBar
  ) {
    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;

    panel.name = 'orderadjustment';
    panel.title = 'Order/Invoice Adjustments';
    panel.icon = 'money';
    panel.expandable = true;

    this.delete = deleteService.delete(panel.manager.context);
    this.edit = this.editService.edit();

    this.table = new Table({
      selection: true,
      columns: [
        { name: 'adjustment' },
        { name: 'amount' },
        { name: 'percentage' },
      ],
      actions: [this.edit, this.delete],
      defaultAction: this.edit,
      autoSort: true,
      autoFilter: true,
    });

    const quoteOrderAdjustmentsPullName = `${panel.name}_${this.m.Quote.tag}_adjustments`;
    const orderOrderAdjustmentsPullName = `${panel.name}_${this.m.Order.tag}_adjustments`;
    const invoiceOrderAdjustmentsPullName = `${panel.name}_${this.m.Invoice.tag}_adjustments`;
    const quotePullName = `${panel.name}_${this.m.Quote.tag}`;
    const orderPullName = `${panel.name}_${this.m.Order.tag}`;
    const invoicePullName = `${panel.name}_${this.m.Invoice.tag}`;

    panel.onPull = (pulls) => {
      const m = this.m;
      const { pullBuilder: pull } = m;
      const x = {};

      const id = this.panel.manager.id;

      pulls.push(
        pull.Quote({
          name: quoteOrderAdjustmentsPullName,
          objectId: id,
          select: {
            OrderAdjustments: x,
          },
        }),
        pull.Order({
          name: orderOrderAdjustmentsPullName,
          objectId: id,
          select: {
            OrderAdjustments: x,
          },
        }),
        pull.Invoice({
          name: invoiceOrderAdjustmentsPullName,
          objectId: id,
          select: {
            OrderAdjustments: x,
          },
        }),
        pull.Quote({
          name: quotePullName,
          objectId: id,
        }),
        pull.Order({
          name: orderPullName,
          objectId: id,
        }),
        pull.Invoice({
          name: invoicePullName,
          objectId: id,
        })
      );
    };

    panel.onPulled = (loaded) => {
      this.container =
        loaded.object<Quote>(quotePullName) ||
        loaded.object<Order>(orderPullName) ||
        loaded.object<Invoice>(invoicePullName);

      this.objects =
        loaded.collection<OrderAdjustment>(quoteOrderAdjustmentsPullName) ||
        loaded.collection<OrderAdjustment>(orderOrderAdjustmentsPullName) ||
        loaded.collection<OrderAdjustment>(invoiceOrderAdjustmentsPullName) ||
        [];

      this.table.total =
        (loaded.value(`${quoteOrderAdjustmentsPullName}_total`) as number) ??
        (loaded.value(`${orderOrderAdjustmentsPullName}_total`) as number) ??
        (loaded.value(`${invoiceOrderAdjustmentsPullName}_total`) as number) ??
        this.objects?.length ??
        0;

      this.table.data = this.objects?.map((v) => {
        return {
          object: v,
          adjustment: v.strategy.cls.singularName,
          amount: v.Amount,
          percentage: v.Percentage,
        } as Row;
      });
    };
  }
}
