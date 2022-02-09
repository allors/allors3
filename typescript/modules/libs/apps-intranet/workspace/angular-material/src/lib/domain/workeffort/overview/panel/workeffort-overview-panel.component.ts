import { Component, OnInit, Self, HostBinding } from '@angular/core';

import { M } from '@allors/default/workspace/meta';
import { WorkEffort } from '@allors/default/workspace/domain';
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
  OverviewService,
} from '@allors/base/workspace/angular/foundation';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';

interface Row extends TableRow {
  object: WorkEffort;
  id: string;
  takenBy: string;
  name: string;
  description: string;
}

@Component({
  selector: 'workeffort-overview-panel',
  templateUrl: './workeffort-overview-panel.component.html',
  providers: [OldPanelService],
})
export class WorkEffortOverviewPanelComponent implements OnInit {
  @HostBinding('class.expanded-panel') get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  m: M;

  objects: WorkEffort[];
  table: Table<Row>;

  edit: Action;

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
    public navigationService: NavigationService,

    public deleteService: DeleteService,
    public editService: EditService,
    public overviewService: OverviewService
  ) {
    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;
  }

  ngOnInit() {
    this.panel.name = 'workeffort';
    // this.panel.title = 'Child Work Orders';
    this.panel.title = 'Child Work Orders';
    this.panel.icon = 'business';
    this.panel.expandable = true;

    this.edit = this.editService.edit();

    const sort = true;
    this.table = new Table({
      selection: true,
      columns: [
        { name: 'id', sort },
        { name: 'takenBy', sort },
        { name: 'name', sort },
        { name: 'description', sort },
      ],
      actions: [this.edit],
      defaultAction: this.overviewService.overview(),
      autoSort: true,
      autoFilter: true,
    });

    const pullName = `${this.panel.name}_${this.m.WorkEffort.tag}`;

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
            Children: {
              include: {
                TakenBy: x,
              },
            },
          },
        })
      );
    };

    this.panel.onPulled = (loaded) => {
      this.objects = loaded.collection<WorkEffort>(pullName);

      this.table.total =
        (loaded.value(`${pullName}_total`) as number) ??
        this.objects?.length ??
        0;
      this.table.data = this.objects?.map((v) => {
        return {
          object: v,
          id: v.WorkEffortNumber,
          takenBy: v.TakenBy.DisplayName,
          name: v.Name,
          description: v.Description,
        } as Row;
      });
    };
  }
}
