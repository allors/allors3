import { Component, Self, OnInit, HostBinding } from '@angular/core';
import { format } from 'date-fns';

import { M } from '@allors/workspace/meta/default';
import { Employment } from '@allors/workspace/domain/default';
import { Action, DeleteService, EditService, NavigationService, ObjectData, PanelService, RefreshService, Table, TableRow } from '@allors/workspace/angular/base';
import { WorkspaceService } from '@allors/workspace/angular/core';

interface Row extends TableRow {
  object: Employment;
  type: string;
  parties: string;
  from: string;
  through: string;
}

@Component({
  selector: 'employment-overview-panel',
  templateUrl: './employment-overview-panel.component.html',
  providers: [PanelService],
})
export class EmploymentOverviewPanelComponent implements OnInit {
  @HostBinding('class.expanded-panel') get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  m: M;

  collection = 'Current';
  table: Table<Row>;
  delete: Action;
  edit: Action;

  all: Employment[];
  active: Employment[];
  inactive: Employment[];

  constructor(
    @Self() public panel: PanelService,
    public workspaceService: WorkspaceService,
    public refreshService: RefreshService,
    public navigationService: NavigationService,
    public deleteService: DeleteService,
    public editService: EditService
  ) {
    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;
  }

  ngOnInit() {
    this.panel.name = 'employement';
    this.panel.title = 'Employments';
    this.panel.icon = 'contacts';
    this.panel.expandable = true;

    this.delete = this.deleteService.delete(this.panel.manager.context);
    this.edit = this.editService.edit();

    const sort = true;
    this.table = new Table({
      selection: true,
      columns: [
        { name: 'type', sort },
        { name: 'parties', sort },
        { name: 'from', sort },
        { name: 'through', sort },
      ],
      actions: [this.edit, this.delete],
      defaultAction: this.edit,
      autoSort: true,
      autoFilter: true,
    });

    const pullName = `${this.panel.name}_${this.m.Employment.tag}`;

    this.panel.onPull = (pulls) => {
      const m = this.m;
      const { pullBuilder: pull } = m;
      const id = this.panel.manager.id;

      pulls.push(
        pull.Employment({
          name: pullName,
          predicate: {
            kind: 'Or',
            operands: [
              {
                kind: 'Equals',
                propertyType: m.Employment.Employer,
                value: id,
              },
              {
                kind: 'Equals',
                propertyType: m.Employment.Employee,
                value: id,
              },
            ],
          },
          include: {
            Employer: {
              ActiveEmployments: {},
              InactiveEmployments: {},
            },
          },
        })
      );
    };

    this.panel.onPulled = (loaded) => {
      this.all = loaded.collection<Employment>(pullName) ?? [];
      this.active = this.all.filter((v) => v.Employer?.ActiveEmployments.includes(v));
      this.inactive = this.all.filter((v) => v.Employer?.InactiveEmployments.includes(v));

      this.table.total = (loaded.value(`${pullName}_total`) ?? this.active?.length ?? 0) as number;
      this.refreshTable();
    };
  }

  public refreshTable() {
    this.table.data = this.employments?.map((v) => {
      return {
        object: v,
        type: v.strategy.cls.singularName,
        parties: v.Employer?.Name + ', ' + v.Employee.FirstName,
        from: format(new Date(v.FromDate), 'dd-MM-yyyy'),
        through: v.ThroughDate != null ? format(new Date(v.ThroughDate), 'dd-MM-yyyy') : '',
      } as Row;
    });
  }

  get employments() {
    switch (this.collection) {
      case 'Current':
        return this.active;
      case 'Inactive':
        return this.inactive;
      case 'All':
      default:
        return this.all;
    }
  }

  get createData(): ObjectData {
    return {
      associationId: this.panel.manager.id,
      associationObjectType: this.panel.manager.objectType,
    };
  }
}
