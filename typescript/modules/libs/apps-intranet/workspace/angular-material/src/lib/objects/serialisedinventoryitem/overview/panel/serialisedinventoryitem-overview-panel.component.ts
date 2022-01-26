import { Component, Self, OnInit, HostBinding } from '@angular/core';

import { M } from '@allors/default/workspace/meta';
import {
  InventoryItem,
  SerialisedInventoryItem,
  SerialisedItem,
} from '@allors/default/workspace/domain';
import {
  Action,
  DeleteService,
  EditService,
  NavigationService,
  ObjectData,
  ObjectService,
  PanelService,
  RefreshService,
  Table,
  TableRow,
  OverviewService,
  ActionTarget,
} from '@allors/base/workspace/angular/foundation';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';

interface Row extends TableRow {
  object: InventoryItem;
  facility: string;
  item: string;
  quantity: number;
  state: string;
}

@Component({
  selector: 'serialisedinventoryitem-overview-panel',
  templateUrl: './serialisedinventoryitem-overview-panel.component.html',
  providers: [PanelService],
})
export class SerialisedInventoryItemComponent implements OnInit {
  serialisedItem: SerialisedItem;

  @HostBinding('class.expanded-panel') get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  m: M;

  table: Table<Row>;

  edit: Action;
  changeInventory: Action;

  objects: SerialisedInventoryItem[];

  get createData(): ObjectData {
    return {
      associationId: this.panel.manager.id,
      associationObjectType: this.panel.manager.objectType,
    };
  }

  constructor(
    @Self() public panel: PanelService,
    public workspaceService: WorkspaceService,
    public objectService: ObjectService,
    public factoryService: ObjectService,
    public refreshService: RefreshService,
    public navigationService: NavigationService,
    public overviewService: OverviewService,
    public deleteService: DeleteService,
    public editService: EditService
  ) {
    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;
  }

  ngOnInit() {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.panel.name = 'serialised Inventory item';
    this.panel.title = 'Serialised Inventory items';
    this.panel.icon = 'link';
    this.panel.expandable = true;

    this.edit = this.editService.edit();
    this.changeInventory = {
      name: 'changeinventory',
      displayName: () => 'Change Inventory',
      description: () => '',
      disabled: () => false,
      execute: (target: ActionTarget) => {
        if (!Array.isArray(target)) {
          this.factoryService.create(this.m.InventoryItemTransaction, {
            associationId: target.id,
            associationObjectType: target.strategy.cls,
          });
        }
      },
      result: null,
    };

    this.table = new Table({
      selection: false,
      columns: [
        { name: 'facility', sort: true },
        { name: 'item', sort: true },
        { name: 'quantity', sort: true },
        { name: 'state', sort: true },
      ],
      defaultAction: this.changeInventory,
    });

    const inventoryPullName = `${this.panel.name}_${this.m.SerialisedInventoryItem.tag}`;
    const serialiseditemPullName = `${this.panel.name}_${this.m.SerialisedItem.tag}`;

    this.panel.onPull = (pulls) => {
      const id = this.panel.manager.id;

      pulls.push(
        pull.Part({
          name: inventoryPullName,
          objectId: id,
          select: {
            InventoryItemsWherePart: {
              include: {
                Part: {
                  SupplierOfferingsWherePart: {
                    Supplier: x,
                  },
                },
                SerialisedInventoryItem_SerialisedInventoryItemState: x,
                Facility: x,
                UnitOfMeasure: x,
              },
            },
          },
        }),
        pull.SerialisedItem({
          name: serialiseditemPullName,
          objectId: id,
          select: {
            SerialisedInventoryItemsWhereSerialisedItem: {
              include: {
                Part: {
                  SupplierOfferingsWherePart: {
                    Supplier: x,
                  },
                },
                SerialisedInventoryItemState: x,
                Facility: x,
                UnitOfMeasure: x,
              },
            },
          },
        }),
        pull.SerialisedItem({
          objectId: id,
          include: {
            PartWhereSerialisedItem: x,
          },
        })
      );

      this.panel.onPulled = (loaded) => {
        this.serialisedItem = loaded.object<SerialisedItem>(m.SerialisedItem);
        const inventoryObjects =
          loaded.collection<SerialisedInventoryItem>(inventoryPullName) ?? [];

        const serialisedItemobjects =
          loaded.collection<SerialisedInventoryItem>(serialiseditemPullName) ??
          [];
        const serialisedItemobjectsforPart = serialisedItemobjects?.filter(
          (v) => v.Part === this.serialisedItem?.PartWhereSerialisedItem
        );

        this.objects = inventoryObjects.concat(serialisedItemobjectsforPart);

        this.table.total =
          (loaded.value(`${this.objects?.length ?? 0}_total`) as number) ??
          this.objects?.length ??
          0;
        this.table.data = this.objects?.map((v) => {
          return {
            object: v,
            facility: v.Facility.Name,
            item: v.SerialisedItem.DisplayName,
            quantity: v.Quantity,
            state: v.SerialisedInventoryItemState
              ? v.SerialisedInventoryItemState.Name
              : '',
          } as Row;
        });
      };
    };
  }
}