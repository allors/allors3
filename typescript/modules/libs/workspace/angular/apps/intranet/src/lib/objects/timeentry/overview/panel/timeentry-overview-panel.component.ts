import { Component, OnInit, Self, HostBinding } from '@angular/core';
import { format } from 'date-fns';

import { M } from '@allors/workspace/meta/default';
import { WorkEffort, TimeEntry } from '@allors/workspace/domain/default';
import { Action, DeleteService, EditService, NavigationService, ObjectData, PanelService, RefreshService, Table, TableRow } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';

interface Row extends TableRow {
  object: TimeEntry;
  person: string;
  from: string;
  through: string;
  time: string;
}

@Component({
  selector: 'timeentry-overview-panel',
  templateUrl: './timeentry-overview-panel.component.html',
  providers: [PanelService, ContextService],
})
export class TimeEntryOverviewPanelComponent {
  workEffort: WorkEffort;
  @HostBinding('class.expanded-panel') get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  m: M;

  objects: TimeEntry[];
  table: Table<Row>;

  delete: Action;
  edit: Action;

  get createData(): ObjectData {
    return {
      associationId: this.panel.manager.id,
      associationObjectType: this.panel.manager.objectType,
    };
  }

  constructor(
    @Self() public allors: ContextService,
    @Self() public panel: PanelService,
    public refreshService: RefreshService,
    public navigationService: NavigationService,
    public deleteService: DeleteService,
    public editService: EditService
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.panel.name = 'timeentry';
    this.panel.title = 'Time Entries';
    this.panel.icon = 'timer';
    this.panel.expandable = true;

    this.delete = this.deleteService.delete(this.panel.manager.context);
    this.edit = this.editService.edit();

    this.table = new Table({
      selection: true,
      columns: [{ name: 'person' }, { name: 'from', sort: true }, { name: 'through', sort: true }, { name: 'time', sort: true }],
      actions: [this.edit, this.delete],
      defaultAction: this.edit,
      autoSort: true,
      autoFilter: true,
    });

    this.panel.onPull = (pulls) => {
      const id = this.panel.manager.id;

      pulls.push(
        pull.WorkEffort({
          objectId: id,
          select: {
            ServiceEntriesWhereWorkEffort: {
              include: {
                TimeEntry_Worker: x,
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
      this.workEffort = loaded.object<WorkEffort>(m.WorkEffort);
      this.objects = loaded.collection<TimeEntry>(m.WorkEffort.ServiceEntriesWhereWorkEffort);

      this.table.total = this.objects?.length ?? 0;
      this.table.data = this.objects?.map((v) => {
        return {
          object: v,
          person: v.Worker && v.Worker.DisplayName,
          from: format(new Date(v.FromDate), 'dd-MM-yyyy'),
          through: v.ThroughDate != null ? format(new Date(v.ThroughDate), 'dd-MM-yyyy') : '',
          time: v.AmountOfTime,
        } as Row;
      });
    };
  }
}
