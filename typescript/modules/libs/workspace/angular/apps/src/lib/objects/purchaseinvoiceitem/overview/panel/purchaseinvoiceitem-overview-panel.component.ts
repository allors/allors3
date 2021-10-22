import { Component, Self, HostBinding } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

import { M } from '@allors/workspace/meta/default';
import { PurchaseInvoice, PurchaseInvoiceItem } from '@allors/workspace/domain/default';
import { Action, DeleteService, EditService, NavigationService, ObjectData, ObjectService, PanelService, RefreshService, Table, TableRow, TestScope } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';

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
  // tslint:disable-next-line:component-selector
  selector: 'purchaseinvoiceitem-overview-panel',
  templateUrl: './purchaseinvoiceitem-overview-panel.component.html',
  providers: [ContextService, PanelService],
})
export class PurchaseInvoiceItemOverviewPanelComponent extends TestScope {
  @HostBinding('class.expanded-panel') get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  m: M;

  purchaseInvoiceItems: PurchaseInvoiceItem[];
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
    @Self() public panel: PanelService,
    public objectService: ObjectService,

    public refreshService: RefreshService,
    public navigation: NavigationService,

    public editService: EditService,
    public deleteService: DeleteService,
    public snackBar: MatSnackBar
  ) {
    super();

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
      columns: [{ name: 'item', sort }, { name: 'itemId' }, { name: 'type', sort }, { name: 'state', sort }, { name: 'quantity', sort }, { name: 'totalExVat', sort }],
      actions: [this.edit, this.delete],
      defaultAction: this.edit,
      autoSort: true,
      autoFilter: true,
    });

    const pullName = `${panel.name}_${this.m.PurchaseInvoiceItem.tag}`;
    const invoicePullName = `${panel.name}_${this.m.PurchaseInvoice.tag}`;

    panel.onPull = (pulls) => {
      const m = this.m;
      const { pullBuilder: pull } = m;
      const x = {};

      const id = this.panel.manager.id;

      pulls.push(
        pull.PurchaseInvoice({
          name: pullName,
          objectId: id,
          select: {
            PurchaseInvoiceItems: {
              include: {
                PurchaseInvoiceItemState: x,
                Part: x,
                InvoiceItemType: x,
              },
            },
          },
        }),
        pull.PurchaseInvoice({
          name: invoicePullName,
          objectId: id,
        })
      );
    };

    panel.onPulled = (loaded) => {
      this.purchaseInvoiceItems = loaded.collection<PurchaseInvoiceItem>(pullName);
      this.invoice = loaded.object<PurchaseInvoice>(invoicePullName);
      this.table.total = (loaded.value(`${pullName}_total`) ?? this.purchaseInvoiceItems.length) as number;;
      this.table.data = this.purchaseInvoiceItems?.map((v) => {
        return {
          object: v,
          item: (v.Part && v.Part.Name) || '',
          itemId: v.SerialisedItem && v.SerialisedItem.ItemNumber,
          type: `${v.InvoiceItemType && v.InvoiceItemType.Name}`,
          state: `${v.PurchaseInvoiceItemState && v.PurchaseInvoiceItemState.Name}`,
          quantity: v.Quantity,
          totalExVat: v.TotalExVat,
        } as Row;
      });
    };
  }
}
