import { Component, Self, HostBinding } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

import { M } from '@allors/default/workspace/meta';
import {
  PurchaseInvoice,
  PurchaseInvoiceItem,
} from '@allors/default/workspace/domain';
import {
  Action,
  DeleteService,
  EditService,
  NavigationService,
  ObjectData,
  ObjectService,
  OldPanelService,
  RefreshService,
  Table,
  TableRow,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

interface Row extends TableRow {
  object: PurchaseInvoiceItem;
  item: string;
  itemId: string;
  type: string;
  state: string;
  quantity: string;
  totalExVat: string;
}

@Component({
  selector: 'purchaseinvoiceitem-overview-panel',
  templateUrl: './purchaseinvoiceitem-overview-panel.component.html',
  providers: [ContextService, OldPanelService],
})
export class PurchaseInvoiceItemOverviewPanelComponent {
  purchaseInvoiceItems: PurchaseInvoiceItem[];
  @HostBinding('class.expanded-panel') get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  m: M;

  invoice: PurchaseInvoice;
  table: Table<Row>;

  delete: Action;
  edit: Action;

  get createData(): ObjectData {
    return {
      associationId: this.panel.manager.id,
      associationObjectType: this.panel.manager.objectType,
      associationRoleType: this.m.PurchaseInvoice.PurchaseInvoiceItems,
    };
  }

  constructor(
    @Self() public allors: ContextService,
    @Self() public panel: OldPanelService,
    public objectService: ObjectService,

    public refreshService: RefreshService,
    public navigation: NavigationService,

    public editService: EditService,
    public deleteService: DeleteService,
    public snackBar: MatSnackBar
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;

    panel.name = 'purchaseinvoicetitem';
    panel.title = 'Purchase Invoice Items';
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

    const invoicePullName = `${panel.name}_${this.m.PurchaseInvoice.tag}`;

    panel.onPull = (pulls) => {
      const m = this.m;
      const { pullBuilder: pull } = m;
      const x = {};

      const id = this.panel.manager.id;

      pulls.push(
        pull.PurchaseInvoice({
          name: invoicePullName,
          objectId: id,
          include: {
            PurchaseInvoiceItems: {
              PurchaseInvoiceItemState: x,
              Part: x,
              InvoiceItemType: x,
            },
          },
        })
      );
    };

    panel.onPulled = (loaded) => {
      this.invoice = loaded.object<PurchaseInvoice>(invoicePullName);
      this.purchaseInvoiceItems = this.invoice?.PurchaseInvoiceItems;

      this.table.total = (this.purchaseInvoiceItems?.length ?? 0) as number;
      this.table.data = this.purchaseInvoiceItems?.map((v) => {
        return {
          object: v,
          item: (v.Part && v.Part.Name) || '',
          itemId: v.SerialisedItem && v.SerialisedItem.ItemNumber,
          type: `${v.InvoiceItemType && v.InvoiceItemType.Name}`,
          state: `${
            v.PurchaseInvoiceItemState && v.PurchaseInvoiceItemState.Name
          }`,
          quantity: v.Quantity,
          totalExVat: v.TotalExVat,
        } as Row;
      });
    };
  }
}
