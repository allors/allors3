import { Component, Self, HostBinding } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

import { M } from '@allors/workspace/meta/default';
import { Shipment, ShipmentItem } from '@allors/workspace/domain/default';
import { Action, DeleteService, EditService, MethodService, NavigationService, ObjectData, ObjectService, PanelService, RefreshService, Table, TableRow, TestScope } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';

interface Row extends TableRow {
  object: ShipmentItem;
  item: string;
  state: string;
  quantity: string;
  picked: string;
  shipped: string;
}

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'shipmentitem-overview-panel',
  templateUrl: './shipmentitem-overview-panel.component.html',
  providers: [ContextService, PanelService],
})
export class ShipmentItemOverviewPanelComponent extends TestScope {
  @HostBinding('class.expanded-panel') get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  m: M;

  shipment: Shipment;
  shipmentItems: ShipmentItem[];
  table: Table<Row>;

  delete: Action;
  edit: Action;

  get createData(): ObjectData {
    return {
      associationId: this.panel.manager.id,
      associationObjectType: this.panel.manager.objectType,
      associationRoleType: this.m.Shipment.ShipmentItems,
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
    super();

    this.m = this.allors.context.configuration.metaPopulation as M;

    panel.name = 'shipmentitem';
    panel.title = 'Shipment Items';
    panel.icon = 'shopping_cart';
    panel.expandable = true;

    this.delete = deleteService.delete(panel.manager.context);
    this.edit = editService.edit();

    const sort = true;
    this.table = new Table({
      selection: true,
      columns: [
        { name: 'item', sort },
        { name: 'state', sort },
        { name: 'quantity', sort },
        { name: 'picked', sort },
        { name: 'shipped', sort },
      ],
      actions: [this.edit, this.delete],
      defaultAction: this.edit,
      autoSort: true,
      autoFilter: true,
    });

    const pullName = `${panel.name}_${this.m.ShipmentItem.tag}`;
    const shipmentPullName = `${panel.name}_${this.m.Shipment.tag}`;

    panel.onPull = (pulls) => {
      const m = this.m;
      const { pullBuilder: pull } = m;
      const x = {};

      const id = this.panel.manager.id;

      pulls.push(
        pull.Shipment({
          name: pullName,
          objectId: id,
          select: {
            ShipmentItems: {
              include: {
                ShipmentItemState: x,
                Good: x,
                Part: x,
              },
            },
          },
        }),
        pull.Shipment({
          name: shipmentPullName,
          objectId: id,
        })
      );
    };

    panel.onPulled = (loaded) => {
      this.shipmentItems = loaded.collection<ShipmentItem>(pullName);
      this.shipment = loaded.object<Shipment>(shipmentPullName);
      this.table.total = ((loaded.value(`${pullName}_total`) as number) ?? this.shipmentItems.length) as number;
      this.table.data = this.shipmentItems.map((v) => {
        return {
          object: v,
          item: (v.Good && v.Good.Name) || (v.Part && v.Part.Name) || '',
          state: `${v.ShipmentItemState && v.ShipmentItemState.Name}`,
          quantity: v.Quantity,
          picked: v.QuantityPicked,
          shipped: v.QuantityShipped,
        } as Row;
      });
    };
  }
}
