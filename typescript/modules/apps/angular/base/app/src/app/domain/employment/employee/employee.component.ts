import { Component, Self, OnInit, HostBinding } from '@angular/core';
import { format } from 'date-fns';

import {
  Employment,
  Organisation,
  Person,
} from '@allors/workspace/domain/default';
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
  AllorsPanelObjectRelationComponent,
} from '@allors/workspace/angular/base';

interface Row extends TableRow {
  object: Employment;
  type: string;
  parties: string;
  from: string;
  through: string;
}

@Component({
  selector: 'employee',
  templateUrl: './employee.component.html',
  providers: [PanelService],
})
export class EmployeeComponent
  extends AllorsPanelObjectRelationComponent<Organisation | Person>
  implements OnInit
{
  @HostBinding('class.expanded-panel')
  get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  collection = 'Current';
  table: Table<Row>;
  delete: Action;
  edit: Action;

  all: Employment[];
  active: Employment[];
  inactive: Employment[];

  constructor(
    @Self() panel: PanelService,
    public refreshService: RefreshService,
    public navigationService: NavigationService,
    public deleteService: DeleteService,
    public editService: EditService
  ) {
    super(panel);

    const employment = this.m.Employment;
    this.objectType = employment;
    this.associationRoleType = employment.Employer;
    this.roleRoleType = employment.Employee;
  }

  ngOnInit() {
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
                propertyType: this.associationRoleType,
                value: id,
              },
              {
                kind: 'Equals',
                propertyType: this.roleRoleType,
                value: id,
              },
            ],
          },
          include: {
            Employer: {
              ActiveEmployments: {},
              InactiveEmployments: {},
            },
            Employee: {},
          },
        })
      );
    };

    this.panel.onPulled = (loaded) => {
      this.all = loaded.collection<Employment>(pullName) ?? [];
      this.active = this.all.filter((v) =>
        v.Employer?.ActiveEmployments.includes(v)
      );
      this.inactive = this.all.filter((v) =>
        v.Employer?.InactiveEmployments.includes(v)
      );

      this.table.total = (loaded.value(`${pullName}_total`) ??
        this.active?.length ??
        0) as number;
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
        through:
          v.ThroughDate != null
            ? format(new Date(v.ThroughDate), 'dd-MM-yyyy')
            : '',
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
