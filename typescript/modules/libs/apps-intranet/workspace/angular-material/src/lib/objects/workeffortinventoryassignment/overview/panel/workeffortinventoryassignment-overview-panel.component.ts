import { Component, OnInit, Self, HostBinding } from '@angular/core';

import { M } from '@allors/default/workspace/meta';
import {
  WorkEffort,
  WorkEffortInventoryAssignment,
} from '@allors/default/workspace/domain';
import {
  Action,
  DeleteService,
  EditService,
  NavigationService,
  ObjectData,
  OldPanelService,
  RefreshService,
  Table,
  TableRow,
} from '@allors/base/workspace/angular/foundation';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';

interface Row extends TableRow {
  object: WorkEffortInventoryAssignment;
  part: string;
  facility: string;
  quantity: string;
  uom: string;
}

@Component({
  selector: 'workeffortinventoryassignment-overview-panel',
  templateUrl: './workeffortinventoryassignment-overview-panel.component.html',
  providers: [OldPanelService],
})
export class WorkEffortInventoryAssignmentOverviewPanelComponent
  implements OnInit
{
  workEffort: WorkEffort;

  @HostBinding('class.expanded-panel') get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  m: M;

  objects: WorkEffortInventoryAssignment[] = [];

  edit: Action;
  table: Table<TableRow>;

  get createData(): ObjectData {
    return {
      associationId: this.panel.manager.id,
      associationObjectType: this.panel.manager.objectType,
    };
  }

  constructor(
    @Self() public panel: OldPanelService,
    public workspaceService: WorkspaceService,

    public refreshService: RefreshService,
    public navigation: NavigationService,

    public deleteService: DeleteService,
    public editService: EditService
  ) {
    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;
  }

  ngOnInit() {
    this.edit = this.editService.edit();

    this.panel.name = 'workeffortinventoryassignment';
    // this.panel.title = 'Inventory Assignment';
    this.panel.title = 'Parts Used';
    this.panel.icon = 'work';
    this.panel.expandable = true;

    this.table = new Table({
      selection: true,
      columns: [
        { name: 'part' },
        { name: 'facility' },
        { name: 'quantity' },
        { name: 'uom' },
      ],
      actions: [this.edit],
      defaultAction: this.edit,
      autoSort: true,
      autoFilter: true,
    });

    const pullName = `${this.panel.name}_${this.m.WorkEffortInventoryAssignment.tag}`;

    this.panel.onPull = (pulls) => {
      const m = this.m;
      const { pullBuilder: pull } = m;
      const x = {};

      const id = this.panel.manager.id;

      pulls.push(
        pull.WorkEffort({
          name: pullName,
          objectId: id,
          select: {
            WorkEffortInventoryAssignmentsWhereAssignment: {
              include: {
                InventoryItem: {
                  Part: x,
                  Facility: x,
                  UnitOfMeasure: x,
                  NonSerialisedInventoryItem_NonSerialisedInventoryItemState: x,
                  SerialisedInventoryItem_SerialisedInventoryItemState: x,
                },
              },
            },
          },
        }),
        pull.WorkEffort({
          objectId: id,
        })
      );
    };

    this.panel.onPulled = (loaded) => {
      this.workEffort = loaded.object<WorkEffort>(this.m.WorkEffort);
      this.objects = loaded.collection<WorkEffortInventoryAssignment>(pullName);

      this.table.total = this.objects?.length ?? 0;
      this.table.data = this.objects?.map((v) => {
        return {
          object: v,
          part: v.InventoryItem.Part.Name,
          facility: v.InventoryItem.Facility.Name,
          quantity: v.Quantity,
          uom: v.InventoryItem.UnitOfMeasure.Name,
        } as Row;
      });
    };
  }
}
