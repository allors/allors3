import { Component, Self, HostBinding } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { formatDistance } from 'date-fns';

import { M } from '@allors/default/workspace/meta';
import {
  PurchaseOrder,
  PurchaseOrderItem,
} from '@allors/default/workspace/domain';
import {
  Action,
  DeleteService,
  EditService,
  MethodService,
  NavigationService,
  ObjectData,
  ObjectService,
  PanelService,
  RefreshService,
  Table,
  TableRow,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

interface Row extends TableRow {
  object: PurchaseOrderItem;
  item: string;
  itemId;
  string;
  type: string;
  state: string;
  ordered: string;
  received: string;
  lastModifiedDate: string;
}

@Component({
  selector: 'purchaseorderitem-overview-panel',
  templateUrl: './purchaseorderitem-overview-panel.component.html',
  providers: [ContextService, PanelService],
})
export class PurchaseOrderItemOverviewPanelComponent {
  @HostBinding('class.expanded-panel') get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  m: M;

  order: PurchaseOrder;
  objects: PurchaseOrderItem[];
  table: Table<Row>;

  delete: Action;
  edit: Action;
  cancel: Action;
  reject: Action;
  reopen: Action;
  quickReceive: Action;

  get createData(): ObjectData {
    return {
      associationId: this.panel.manager.id,
      associationObjectType: this.panel.manager.objectType,
      associationRoleType: this.m.PurchaseOrder.PurchaseOrderItems,
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

    panel.name = 'purchaseordertitem';
    panel.title = 'Purchase Order Items';
    panel.icon = 'business';
    panel.expandable = true;

    this.delete = deleteService.delete(panel.manager.context);
    this.edit = editService.edit();
    this.cancel = methodService.create(
      allors.context,
      this.m.PurchaseOrderItem.Cancel,
      { name: 'Cancel' }
    );
    this.reject = methodService.create(
      allors.context,
      this.m.PurchaseOrderItem.Reject,
      { name: 'Reject' }
    );
    this.reopen = methodService.create(
      allors.context,
      this.m.PurchaseOrderItem.Reopen,
      { name: 'Reopen' }
    );
    this.quickReceive = methodService.create(
      allors.context,
      this.m.PurchaseOrderItem.QuickReceive,
      { name: 'QuickReceive' }
    );

    const sort = true;
    this.table = new Table({
      selection: true,
      columns: [
        { name: 'item', sort },
        { name: 'itemId' },
        { name: 'type', sort },
        { name: 'state', sort },
        { name: 'ordered', sort },
        { name: 'received', sort },
        { name: 'lastModifiedDate', sort },
      ],
      actions: [
        this.edit,
        this.delete,
        this.cancel,
        this.reject,
        this.reopen,
        this.quickReceive,
      ],
      defaultAction: this.edit,
      autoSort: true,
      autoFilter: true,
    });

    const pullName = `${panel.name}_${this.m.PurchaseOrderItem.tag}`;
    const orderPullName = `${panel.name}_${this.m.PurchaseOrder.tag}`;

    panel.onPull = (pulls) => {
      const m = this.m;
      const { pullBuilder: pull } = m;
      const x = {};

      const id = this.panel.manager.id;

      pulls.push(
        pull.PurchaseOrder({
          name: pullName,
          objectId: id,
          select: {
            PurchaseOrderItems: {
              include: {
                InvoiceItemType: x,
                PurchaseOrderItemState: x,
                Part: x,
                SerialisedItem: x,
              },
            },
          },
        }),
        pull.PurchaseOrder({
          name: orderPullName,
          objectId: id,
        })
      );
    };

    panel.onPulled = (loaded) => {
      this.objects = loaded.collection<PurchaseOrderItem>(pullName);
      this.order = loaded.object<PurchaseOrder>(orderPullName);
      this.table.total =
        (loaded.value(`${pullName}_total`) as number) ??
        this.objects?.length ??
        0;
      this.table.data = this.objects?.map((v) => {
        return {
          object: v,
          item:
            (v.Part && v.Part.Name) ||
            (v.SerialisedItem && v.SerialisedItem.Name) ||
            v.Description,
          itemId: v.SerialisedItem && v.SerialisedItem.ItemNumber,
          type: `${v.InvoiceItemType && v.InvoiceItemType.Name}`,
          state: `${v.PurchaseOrderItemState && v.PurchaseOrderItemState.Name}`,
          ordered: v.QuantityOrdered,
          received: v.QuantityReceived,
          lastModifiedDate: formatDistance(
            new Date(v.LastModifiedDate),
            new Date()
          ),
        } as Row;
      });
    };
  }
}
