import { Component, OnInit, Self, HostBinding } from '@angular/core';
import { isBefore, isAfter } from 'date-fns';

import { M } from '@allors/workspace/meta/default';
import { WorkEffort, WorkEffortAssignmentRate } from '@allors/workspace/domain/default';
import { Action, DeleteService, EditService, NavigationService, ObjectData, PanelService, RefreshService, Table, TableRow, TestScope } from '@allors/workspace/angular/base';
import { WorkspaceService } from '@allors/workspace/angular/core';

interface Row extends TableRow {
  object: WorkEffortAssignmentRate;
  // partyAssignment: string;
  // from: string;
  // through: string;
  rateType: string;
  rate: number;
  frequency: string;
}

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'workeffortassignmentrate-overview-panel',
  templateUrl: './workeffortassignmentrate-overview-panel.component.html',
  providers: [PanelService],
})
export class WorkEffortAssignmentRateOverviewPanelComponent extends TestScope implements OnInit {
  workEffort: WorkEffort;

  @HostBinding('class.expanded-panel') get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  m: M;

  objects: WorkEffortAssignmentRate[];
  table: Table<Row>;

  delete: Action;
  edit: Action;

  get createData(): ObjectData {
    return {
      associationId: this.panel.manager.id,
      associationObjectType: this.panel.manager.objectType,
    };
  }

  collection = 'All';
  currentRates: WorkEffortAssignmentRate[];
  inactiveRates: WorkEffortAssignmentRate[];
  allRates: WorkEffortAssignmentRate[] = [];

  constructor(
    @Self() public panel: PanelService,
    public workspaceService: WorkspaceService,

    public refreshService: RefreshService,
    public navigationService: NavigationService,

    public deleteService: DeleteService,
    public editService: EditService
  ) {
    super();

    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;
  }

  ngOnInit() {
    const m = this.m;

    this.panel.name = 'workeffortrate';
    this.panel.title = 'Rates';
    this.panel.icon = 'contacts';
    this.panel.expandable = true;

    this.delete = this.deleteService.delete(this.panel.manager.context);
    this.edit = this.editService.edit();

    const sort = true;
    this.table = new Table({
      selection: true,
      columns: [
        { name: 'rateType' },
        // { name: 'partyAssignment' },
        // { name: 'from', sort },
        // { name: 'through', sort },
        { name: 'rate', sort },
        { name: 'frequency' },
      ],
      actions: [this.edit, this.delete],
      defaultAction: this.edit,
      autoSort: true,
      autoFilter: true,
    });

    const pullName = `${this.panel.name}_${this.m.WorkEffortAssignmentRate.tag}`;

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
            WorkEffortAssignmentRatesWhereWorkEffort: {
              include: {
                RateType: x,
                Frequency: x,
                WorkEffortPartyAssignment: {
                  Party: x,
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
      this.objects = loaded.collection<WorkEffortAssignmentRate>(pullName);

      if (this.objects) {
        this.table.total = (loaded.value(`${pullName}_total`) as number) ?? this.objects?.length ?? 0;
        this.refreshTable();
      }
    };
  }

  public refreshTable() {
    this.table.data = this.workEffortAssignmentRates?.map((v) => {
      return {
        object: v,
        // partyAssignment: v.WorkEffortPartyAssignment.DisplayName,
        // from: format(new Date(v.FromDate), 'dd-MM-yyyy'),
        // through: v.ThroughDate != null ? format(new Date(v.ThroughDate), 'dd-MM-yyyy') : '',
        rateType: v.RateType.Name,
        rate: v.Rate,
        frequency: v.Frequency.Name,
      } as Row;
    });
  }

  get workEffortAssignmentRates(): any {
    switch (this.collection) {
      case 'Current':
        return this.objects && this.objects?.filter((v) => isBefore(new Date(v.FromDate), new Date()) && (!v.ThroughDate || isAfter(new Date(v.ThroughDate), new Date())));
      case 'Inactive':
        return this.objects && this.objects?.filter((v) => isAfter(new Date(v.FromDate), new Date()) || (v.ThroughDate && isBefore(new Date(v.ThroughDate), new Date())));
      case 'All':
      default:
        return this.objects;
    }
  }
}
