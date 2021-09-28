import { Component, OnInit, Self, HostBinding } from '@angular/core';
import { format } from 'date-fns';

import { M } from '@allors/workspace/meta/default';
import { WorkEffort, WorkEffortPartyAssignment } from '@allors/workspace/domain/default';
import { Action, DeleteService, EditService, NavigationService, ObjectData, PanelService, RefreshService, Table, TableRow, TestScope } from '@allors/workspace/angular/base';
import { SessionService } from '@allors/workspace/angular/core';

interface Row extends TableRow {
  object: WorkEffortPartyAssignment;
  number: string;
  name: string;
  status: string;
  party: string;
  from: string;
  through: string;
}

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'workeffortpartyassignment-overview-panel',
  templateUrl: './workeffortpartyassignment-overview-panel.component.html',
  providers: [PanelService, SessionService],
})
export class WorkEffortPartyAssignmentOverviewPanelComponent extends TestScope implements OnInit {
  workEffort: WorkEffort;
  fromParty: WorkEffortPartyAssignment[];
  fromWorkEffort: WorkEffortPartyAssignment[];

  @HostBinding('class.expanded-panel') get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  m: M;

  objects: WorkEffortPartyAssignment[] = [];

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
    @Self() public allors: SessionService,
    @Self() public panel: PanelService,

    public refreshService: RefreshService,
    public navigation: NavigationService,

    public deleteService: DeleteService,
    public editService: EditService
  ) {
    super();

    this.m = this.allors.workspace.configuration.metaPopulation as M;

    this.delete = deleteService.delete(allors);
    this.delete.result.subscribe(() => {
      this.table.selection.clear();
    });
  }

  ngOnInit() {
    this.delete = this.deleteService.delete(this.panel.manager.context);
    this.edit = this.editService.edit();

    this.panel.name = 'workeffortpartyassignment';
    this.panel.title = 'Party Assignment';
    this.panel.icon = 'work';
    this.panel.expandable = true;

    const sort = true;
    this.table = new Table({
      selection: true,
      columns: [
        { name: 'number', sort },
        { name: 'name', sort },
        { name: 'state', sort },
        { name: 'party', sort },
        { name: 'from', sort },
        { name: 'through', sort },
      ],
      actions: [this.edit, this.delete],
      defaultAction: this.edit,
      autoSort: true,
      autoFilter: true,
    });

    const partypullName = `${this.panel.name}_${this.m.WorkEffortPartyAssignment.name}_party`;
    const workeffortpullName = `${this.panel.name}_${this.m.WorkEffortPartyAssignment.name}_workeffort`;

    this.panel.onPull = (pulls) => {
      const m = this.m;
      const { pullBuilder: pull } = m;
      const x = {};

      const id = this.panel.manager.id;

      pulls.push(
        pull.Person({
          name: partypullName,
          objectId: id,
          select: {
            WorkEffortPartyAssignmentsWhereParty: {
              include: {
                Party: x,
                Assignment: {
                  WorkEffortState: x,
                  Priority: x,
                },
              },
            },
          },
        }),
        pull.WorkEffort({
          name: workeffortpullName,
          objectId: id,
          select: {
            WorkEffortPartyAssignmentsWhereAssignment: {
              include: {
                Party: x,
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
      this.workEffort = loaded.object<WorkEffort>(m.WorkEffort);
      this.fromParty = loaded.collections[partypullName] as WorkEffortPartyAssignment[];
      this.fromWorkEffort = loaded.collections[workeffortpullName] as WorkEffortPartyAssignment[];

      if (this.fromParty !== undefined && this.fromParty.length > 0) {
        this.objects = this.fromParty;
      }

      if (this.fromWorkEffort !== undefined && this.fromWorkEffort.length > 0) {
        this.objects = this.fromWorkEffort;
      }

      this.objects = this.fromParty || this.fromWorkEffort;

      if (this.objects) {
        this.table.total = this.objects.length;
        this.table.data = this.objects.map((v) => {
          return {
            object: v,
            number: v.Assignment.WorkEffortNumber,
            name: v.Assignment.Name,
            party: v.Party.displayName,
            status: v.Assignment.WorkEffortState ? v.Assignment.WorkEffortState.Name : '',
            from: format(new Date(v.FromDate), 'dd-MM-yyyy'),
            through: v.ThroughDate !== null ? format(new Date(v.ThroughDate), 'dd-MM-yyyy') : '',
          } as Row;
        });
      }
    };
  }
}
