import { Component, Self, OnInit, HostBinding } from '@angular/core';

import { M } from '@allors/workspace/meta/default';
import { SerialisedItem } from '@allors/workspace/domain/default';
import { Action, DeleteService, NavigationService, ObjectData, ObjectService, PanelService, RefreshService, Table, TableRow, TestScope, OverviewService, ActionTarget } from '@allors/workspace/angular/base';
import { WorkspaceService } from '@allors/workspace/angular/core';

interface Row extends TableRow {
  object: SerialisedItem;
  number: string;
  name: string;
  availability: string;
  onWebsite: string;
  ownership: string;
  ownedBy: string;
}

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'serialiseditem-overview-panel',
  templateUrl: './serialiseditem-overview-panel.component.html',
  providers: [PanelService],
})
export class SerialisedItemOverviewPanelComponent extends TestScope implements OnInit {
  @HostBinding('class.expanded-panel') get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  m: M;

  objects: SerialisedItem[] = [];
  table: Table<Row>;

  delete: Action;

  get createData(): ObjectData {
    return {
      associationId: this.panel.manager.id,
      associationObjectType: this.panel.manager.objectType,
    };
  }

  constructor(
    @Self() public panel: PanelService,
    public workspaceService: WorkspaceService,

    public factoryService: ObjectService,
    public refreshService: RefreshService,
    public navigationService: NavigationService,
    public overviewService: OverviewService,
    public deleteService: DeleteService
  ) {
    super();

    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;
  }

  ngOnInit() {
    this.panel.name = 'serialiseditem';
    this.panel.title = 'Serialised Assets';
    this.panel.icon = 'link';
    this.panel.expandable = true;

    this.delete = this.deleteService.delete(this.panel.manager.context);

    this.table = new Table({
      selection: true,
      columns: [{ name: 'number' }, { name: 'name' }, { name: 'availability' }, { name: 'onWebsite' }, { name: 'ownership' }, { name: 'ownedBy' }],
      actions: [
        {
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
        },
        this.overviewService.overview(),
        this.delete,
      ],
      defaultAction: this.overviewService.overview(),
      autoSort: true,
      autoFilter: true,
    });

    const partSerialisedItemsName = `${this.panel.name}_${this.m.SerialisedItem.tag}`;
    const ownedSerialisedItemsName = `${this.panel.name}_${this.m.SerialisedItem.tag}_OwnedSerialisedItemsName`;
    const rentedSerialisedItemsName = `${this.panel.name}_${this.m.SerialisedItem.tag}_RentedSerialisedItems`;

    this.panel.onPull = (pulls) => {
      const m = this.m;
      const { pullBuilder: pull } = m;
      const x = {};
      const id = this.panel.manager.id;

      pulls.push(
        pull.Part({
          name: partSerialisedItemsName,
          objectId: id,
          select: {
            SerialisedItems: {
              include: {
                OwnedBy: x,
                Ownership: x,
                SerialisedItemAvailability: x,
                SerialisedItemState: x,
              },
            },
          },
        }),
        pull.Party({
          objectId: id,
          name: ownedSerialisedItemsName,
          select: {
            SerialisedItemsWhereOwnedBy: {
              include: {
                OwnedBy: x,
                Ownership: x,
                SerialisedItemAvailability: x,
                SerialisedItemState: x,
              },
            },
          },
        }),
        pull.Party({
          objectId: id,
          name: rentedSerialisedItemsName,
          select: {
            SerialisedItemsWhereRentedBy: {
              include: {
                OwnedBy: x,
                Ownership: x,
                SerialisedItemAvailability: x,
                SerialisedItemState: x,
              },
            },
          },
        })
      );

      this.panel.onPulled = (loaded) => {
        const partSerialisedItems = loaded.collection<SerialisedItem>(partSerialisedItemsName);
        const ownedSerialisedItems = loaded.collection<SerialisedItem>(ownedSerialisedItemsName);
        const rentedSerialisedItems = loaded.collection<SerialisedItem>(rentedSerialisedItemsName);

        this.objects = [];

        if (ownedSerialisedItems != null) {
          this.objects = this.objects.concat(ownedSerialisedItems);
        }

        if (rentedSerialisedItems != null) {
          this.objects = this.objects.concat(rentedSerialisedItems);
        }

        if (partSerialisedItems != null) {
          this.objects = this.objects.concat(partSerialisedItems);
        }

        this.table.total = this.objects?.length ?? 0;
        this.table.data = this.objects?.map((v) => {
          return {
            object: v,
            number: v.ItemNumber,
            name: v.DisplayName,
            availability: v.SerialisedItemAvailability ? v.SerialisedItemAvailability.Name : '',
            onWebsite: v.AvailableForSale ? 'Yes' : 'No',
            ownership: v.Ownership ? v.Ownership.Name : '',
            ownedBy: v.OwnedBy ? v.OwnedBy.DisplayName : '',
          } as Row;
        });
      };
    };
  }
}
