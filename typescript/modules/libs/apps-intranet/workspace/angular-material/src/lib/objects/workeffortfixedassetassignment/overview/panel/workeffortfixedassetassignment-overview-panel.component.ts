import { Component, OnInit, Self, HostBinding } from '@angular/core';
import { format } from 'date-fns';

import { M } from '@allors/default/workspace/meta';
import {
  WorkEffort,
  WorkEffortFixedAssetAssignment,
} from '@allors/default/workspace/domain';
import {
  Action,
  DeleteService,
  EditService,
  NavigationService,
  ObjectData,
  PanelService,
  RefreshService,
  Table,
  TableRow,
} from '@allors/base/workspace/angular/foundation';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';

interface Row extends TableRow {
  object: WorkEffortFixedAssetAssignment;
  number: string;
  name: string;
  status: string;
  asset: string;
  from: string;
  through: string;
}

@Component({
  selector: 'workeffortfixedassetassignment-overview-panel',
  templateUrl: './workeffortfixedassetassignment-overview-panel.component.html',
  providers: [PanelService],
})
export class WorkEffortFAAssignmentOverviewPanelComponent implements OnInit {
  workEffort: WorkEffort;

  @HostBinding('class.expanded-panel') get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  m: M;

  objects: WorkEffortFixedAssetAssignment[] = [];

  delete: Action;
  edit: Action;
  table: Table<TableRow>;

  get createData(): ObjectData {
    return {
      associationId: this.panel.manager.id,
      associationObjectType: this.panel.manager.objectType,
    };
  }

  constructor(
    @Self() public panel: PanelService,
    public workspaceService: WorkspaceService,

    public refreshService: RefreshService,
    public navigation: NavigationService,

    public deleteService: DeleteService,
    public editService: EditService
  ) {
    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;
  }

  ngOnInit() {
    this.delete = this.deleteService.delete(this.panel.manager.context);
    this.edit = this.editService.edit();

    this.panel.name = 'workeffortfixedassetassignment';
    // this.panel.title = 'Fixed Asset Assignments';
    this.panel.title = 'Equipment';
    this.panel.icon = 'work';
    this.panel.expandable = true;

    const sort = true;
    this.table = new Table({
      selection: true,
      columns: [
        { name: 'number', sort },
        { name: 'name', sort },
        { name: 'state', sort },
        { name: 'asset', sort },
        { name: 'from', sort },
        { name: 'through', sort },
      ],
      actions: [this.edit, this.delete],
      defaultAction: this.edit,
      autoSort: true,
      autoFilter: true,
    });

    const workeffortpullName = `${this.panel.name}_${this.m.WorkEffortFixedAssetAssignment.tag}_workeffort`;

    this.panel.onPull = (pulls) => {
      const m = this.m;
      const { pullBuilder: pull } = m;
      const x = {};

      const id = this.panel.manager.id;

      pulls.push(
        pull.WorkEffort({
          name: workeffortpullName,
          objectId: id,
          select: {
            WorkEffortFixedAssetAssignmentsWhereAssignment: {
              include: {
                FixedAsset: x,
                Assignment: {
                  WorkEffortState: x,
                  Priority: x,
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
      this.objects =
        loaded.collection<WorkEffortFixedAssetAssignment>(workeffortpullName);

      this.table.total = this.objects?.length ?? 0;
      this.table.data = this.objects?.map((v) => {
        return {
          object: v,
          number: v.Assignment.WorkEffortNumber,
          name: v.Assignment.Name,
          status: v.Assignment.WorkEffortState
            ? v.Assignment.WorkEffortState.Name
            : '',
          asset: v.FixedAsset.Name,
          from: format(new Date(v.FromDate), 'dd-MM-yyyy'),
          through:
            v.ThroughDate != null
              ? format(new Date(v.ThroughDate), 'dd-MM-yyyy')
              : '',
        } as Row;
      });
    };
  }
}
