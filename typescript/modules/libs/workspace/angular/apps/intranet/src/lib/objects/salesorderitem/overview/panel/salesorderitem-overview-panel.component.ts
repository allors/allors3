import { Component, Self, HostBinding } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { formatDistance } from 'date-fns';

import { M } from '@allors/workspace/meta/default';
import { SalesOrderItem, SalesOrder } from '@allors/workspace/domain/default';
import { Action, DeleteService, EditService, MethodService, NavigationService, ObjectData, ObjectService, PanelService, RefreshService, Table, TableRow } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';

interface Row extends TableRow {
  object: SalesOrderItem;
  item: string;
  itemId: string;
  type: string;
  state: string;
  ordered: string;
  shipped: string;
  reserved: string;
  short: string;
  returned: string;
  lastModifiedDate: string;
}

@Component({
  
  selector: 'salesorderitem-overview-panel',
  templateUrl: './salesorderitem-overview-panel.component.html',
  providers: [ContextService, PanelService],
})
export class SalesOrderItemOverviewPanelComponent {
  @HostBinding('class.expanded-panel') get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  m: M;

  order: SalesOrder;
  salesOrderItems: SalesOrderItem[];
  table: Table<Row>;

  delete: Action;
  edit: Action;
  cancel: Action;
  reject: Action;
  reopen: Action;

  get createData(): ObjectData {
    return {
      associationId: this.panel.manager.id,
      associationObjectType: this.panel.manager.objectType,
      associationRoleType: this.m.SalesOrder.SalesOrderItems,
    };
  }

  constructor(
    @Self() public allors: ContextService,
    @Self() public panel: PanelService,
    public objectService: ObjectService,
    public refreshService: RefreshService,
    public navigation: NavigationService,
    public methodService: MethodService,
    public deleteService: DeleteService,
    public editService: EditService,
    public snackBar: MatSnackBar
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;

    panel.name = 'salesordertitem';
    panel.title = 'Sales Order Items';
    panel.icon = 'contacts';
    panel.expandable = true;

    this.delete = deleteService.delete(panel.manager.context);
    this.edit = editService.edit();
    this.cancel = methodService.create(allors.context, this.m.SalesOrderItem.Cancel, { name: 'Cancel' });
    this.reject = methodService.create(allors.context, this.m.SalesOrderItem.Reject, { name: 'Reject' });
    this.reopen = methodService.create(allors.context, this.m.SalesOrderItem.Reopen, { name: 'Reopen' });

    const sort = true;
    this.table = new Table({
      selection: true,
      columns: [
        { name: 'item', sort },
        { name: 'itemId' },
        { name: 'type', sort },
        { name: 'state', sort },
        { name: 'ordered', sort },
        { name: 'shipped', sort },
        { name: 'reserved', sort },
        { name: 'short', sort },
        { name: 'returned', sort },
        { name: 'lastModifiedDate', sort },
      ],
      actions: [this.edit, this.delete, this.cancel, this.reject, this.reopen],
      defaultAction: this.edit,
      autoSort: true,
      autoFilter: true,
    });

    const pullName = `${panel.name}_${this.m.SalesOrderItem.tag}`;
    const orderPullName = `${panel.name}_${this.m.SalesOrder.tag}`;

    panel.onPull = (pulls) => {
      const m = this.m;
      const { pullBuilder: pull } = m;
      const x = {};

      const id = this.panel.manager.id;

      pulls.push(
        pull.SalesOrder({
          name: pullName,
          objectId: id,
          select: {
            SalesOrderItems: {
              include: {
                InvoiceItemType: x,
                SalesOrderItemState: x,
                Product: x,
                SerialisedItem: x,
              },
            },
          },
        }),
        pull.SalesOrder({
          name: orderPullName,
          objectId: id,
        })
      );
    };

    panel.onPulled = (loaded) => {
      this.salesOrderItems = loaded.collection<SalesOrderItem>(pullName);
      this.order = loaded.object<SalesOrder>(orderPullName);
      this.table.total = (loaded.value(`${pullName}_total`) as number) ?? this.salesOrderItems?.length ?? 0;
      this.table.data = this.salesOrderItems?.map((v) => {
        return {
          object: v,
          item: (v.Product && v.Product.Name) || (v.SerialisedItem && v.SerialisedItem.Name) || '',
          itemId: `${v.SerialisedItem && v.SerialisedItem.ItemNumber}`,
          type: `${v.InvoiceItemType && v.InvoiceItemType.Name}`,
          state: `${v.SalesOrderItemState && v.SalesOrderItemState.Name}`,
          ordered: v.QuantityOrdered,
          shipped: v.QuantityShipped,
          reserved: v.QuantityReserved,
          short: v.QuantityShortFalled,
          returned: v.QuantityReturned,
          lastModifiedDate: formatDistance(new Date(v.LastModifiedDate), new Date()),
        } as Row;
      });
    };
  }
}
