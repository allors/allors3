import { Component, Self, HostBinding } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

import { M } from '@allors/default/workspace/meta';
import {
  SalesInvoice,
  SalesInvoiceItem,
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
import { ContextService } from '@allors/base/workspace/angular/foundation';

interface Row extends TableRow {
  object: SalesInvoiceItem;
  item: string;
  itemId: string;
  type: string;
  state: string;
  quantity: string;
  totalExVat: string;
}

@Component({
  selector: 'salesinvoiceitem-overview-panel',
  templateUrl: './salesinvoiceitem-overview-panel.component.html',
  providers: [ContextService, OldPanelService],
})
export class SalesInvoiceItemOverviewPanelComponent {
  @HostBinding('class.expanded-panel') get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  m: M;

  salesInvoiceItems: SalesInvoiceItem[];
  invoice: SalesInvoice;
  table: Table<Row>;

  delete: Action;
  edit: Action;

  get createData(): ObjectData {
    return {
      associationId: this.panel.manager.id,
      associationObjectType: this.panel.manager.objectType,
      associationRoleType: this.m.SalesInvoice.SalesInvoiceItems,
    };
  }

  constructor(
    @Self() public allors: ContextService,
    @Self() public panel: OldPanelService,
    public scopedService: ScopedService,
    public refreshService: RefreshService,
    public navigation: NavigationService,
    public editService: EditService,
    public deleteService: DeleteService,
    public snackBar: MatSnackBar
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;

    panel.name = 'salesinvoicetitem';
    panel.title = 'Sales Invoice Items';
    panel.icon = 'business';
    panel.expandable = true;

    this.delete = deleteService.delete(panel.manager.context);
    this.edit = this.editService.edit();

    const sort = true;
    this.table = new Table({
      selection: true,
      columns: [
        { name: 'item', sort },
        { name: 'itemId' },
        { name: 'type', sort },
        { name: 'state', sort },
        { name: 'quantity', sort },
        { name: 'totalExVat', sort },
      ],
      actions: [this.edit, this.delete],
      defaultAction: this.edit,
      autoSort: true,
      autoFilter: true,
    });

    const pullName = `${panel.name}_${this.m.SalesInvoiceItem.tag}`;
    const invoicePullName = `${panel.name}_${this.m.SalesInvoice.tag}`;

    panel.onPull = (pulls) => {
      const m = this.m;
      const { pullBuilder: pull } = m;
      const x = {};

      const id = this.panel.manager.id;

      pulls.push(
        pull.SalesInvoice({
          name: pullName,
          objectId: id,
          select: {
            SalesInvoiceItems: {
              include: {
                SalesInvoiceItemState: x,
                InvoiceItemType: x,
                SerialisedItem: x,
              },
            },
          },
        }),
        pull.SalesInvoice({
          name: invoicePullName,
          objectId: id,
        })
      );
    };

    panel.onPulled = (loaded) => {
      this.salesInvoiceItems = loaded.collection<SalesInvoiceItem>(pullName);
      this.invoice = loaded.object<SalesInvoice>(invoicePullName);
      this.table.total = ((loaded.value(`${pullName}_total`) as number) ??
        this.salesInvoiceItems?.length ??
        0) as number;
      this.table.data = this.salesInvoiceItems?.map((v) => {
        return {
          object: v,
          item:
            (v.Product && v.Product.Name) ||
            (v.SerialisedItem && v.SerialisedItem.Name) ||
            '',
          itemId: `${v.SerialisedItem && v.SerialisedItem.ItemNumber}`,
          type: `${v.InvoiceItemType && v.InvoiceItemType.Name}`,
          state: `${v.SalesInvoiceItemState && v.SalesInvoiceItemState.Name}`,
          quantity: v.Quantity,
          totalExVat: v.TotalExVat,
        } as Row;
      });
    };
  }
}
