import { Component, OnInit, Self, HostBinding } from '@angular/core';

import { M } from '@allors/default/workspace/meta';
import {
  NonSerialisedInventoryItem,
  InventoryItem,
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
  OverviewService,
  ActionTarget,
} from '@allors/base/workspace/angular/foundation';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';

interface Row extends TableRow {
  object: InventoryItem;
  facility: string;
  part: string;
  uom: string;
  location: string;
  qoh: string;
  atp: string;
  committedOut: string;
}

@Component({
  selector: 'nonserialisedinventoryitem-overview-panel',
  templateUrl: './nonserialisedinventoryitem-overview-panel.component.html',
  providers: [OldPanelService],
})
export class NonSerialisedInventoryItemComponent implements OnInit {
  @HostBinding('class.expanded-panel') get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  m: M;

  table: Table<Row>;

  edit: Action;
  changeInventory: Action;

  objects: NonSerialisedInventoryItem[];

  get createData(): ObjectData {
    return {
      associationId: this.panel.manager.id,
      associationObjectType: this.panel.manager.objectType,
    };
  }

  constructor(
    @Self() public panel: OldPanelService,
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

    this.panel.name = 'nonserialised Inventory item';
    this.panel.title = 'Nonserialised Inventory items';
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
        { name: 'facility' },
        { name: 'part' },
        { name: 'uom' },
        { name: 'location' },
        'qoh',
        'atp',
        'committedOut',
      ],
      actions: [this.edit, this.changeInventory],
      defaultAction: this.edit,
      autoSort: true,
      autoFilter: true,
    });

    const pullName = `${this.panel.name}_${this.m.NonSerialisedInventoryItem.tag}`;

    this.panel.onPull = (pulls) => {
      const id = this.panel.manager.id;

      pulls.push(
        pull.Part({
          name: pullName,
          objectId: id,
          select: {
            InventoryItemsWherePart: {
              include: {
                InventoryItemTransactionsWhereInventoryItem: {
                  Reason: x,
                },
                NonSerialisedInventoryItem_NonSerialisedInventoryItemState: x,
                Facility: x,
                UnitOfMeasure: x,
              },
            },
          },
        })
      );

      this.panel.onPulled = (loaded) => {
        this.objects = loaded.collection<NonSerialisedInventoryItem>(pullName);

        this.table.total = (loaded.value(`${pullName}_total`) ??
          this.objects?.length ??
          0) as number;
        this.table.data = this.objects?.map((v) => {
          return {
            object: v,
            facility: v.Facility.Name,
            part: v.Part.Name,
            uom: v.UnitOfMeasure.Abbreviation || v.UnitOfMeasure.Name,
            location: v.PartLocation,
            qoh: v.QuantityOnHand,
            atp: v.AvailableToPromise,
            committedOut: v.QuantityCommittedOut,
          } as Row;
        });
      };
    };
  }
}