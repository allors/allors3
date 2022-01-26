import { format } from 'date-fns';
import { Component, Self, OnInit, HostBinding } from '@angular/core';
import {
  Employment,
  Organisation,
  Person,
} from '@allors/default/workspace/domain';
import { RefreshService } from '@allors/base/workspace/angular/foundation';
import {
  Action,
  NavigationService,
  PanelService,
  AllorsPanelRelationshipComponent,
  CreateData,
} from '@allors/base/workspace/angular/application';
import {
  DeleteService,
  EditRoleService,
  Table,
  TableRow,
} from '@allors/base/workspace/angular-material/application';

interface Row extends TableRow {
  object: Employment;
  type: string;
  parties: string;
  from: string;
  through: string;
}

@Component({
  selector: 'employment',
  templateUrl: './employment.component.html',
  providers: [PanelService],
})
export class EmploymentComponent
  extends AllorsPanelRelationshipComponent<Organisation | Person>
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
    public editRoleService: EditRoleService
  ) {
    super(panel);

    const employment = this.m.Employment;
    this.objectType = employment;
    this.associationRoleType = employment.Employer;
    this.roleRoleType = employment.Employee;
  }

  ngOnInit() {
    this.delete = this.deleteService.delete(this.panel.manager.context);
    this.edit = this.editRoleService.edit();

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

  get createData(): CreateData {
    return {
      kind: 'CreateData',
      objectType: this.m.Employment,
      // associationId: this.panel.manager.id,
      // associationObjectType: this.panel.manager.objectType,
    };
  }
}