import { format } from 'date-fns';
import { Component, Self, OnInit, HostBinding, Input } from '@angular/core';
import { Composite, RoleType } from '@allors/system/workspace/meta';
import { IObject } from '@allors/system/workspace/domain';
import { RefreshService } from '@allors/base/workspace/angular/foundation';
import {
  Action,
  NavigationService,
  PanelService,
  AllorsPanelRelationshipComponent,
  CreateData,
} from '@allors/base/workspace/angular/application';
import { TableRow } from '../table/table-row';
import { Table } from '../table/table';
import { DeleteService } from '../actions/delete/delete.service';
import { EditRoleService } from '../actions/edit-role/edit-role.service';
import { angularIcon } from '../meta/angular-icon';
import { PeriodToggle } from '@allors/base/workspace/angular-material/foundation';

interface Row extends TableRow {
  object: IObject;
}

@Component({
  selector: 'a-mat-dyn-relationship-panel',
  templateUrl: './dynamic-relationship-panel.component.html',
  providers: [PanelService],
})
export class AllorsMaterialDynamicRelationshipPanelComponent
  extends AllorsPanelRelationshipComponent
  implements OnInit
{
  @HostBinding('class.expanded-panel')
  get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  @Input()
  self: RoleType;

  @Input()
  other: RoleType;

  period: PeriodToggle = 'Current';

  table: Table<Row>;
  delete: Action;
  edit: Action;

  all: IObject[];
  active: IObject[];
  inactive: IObject[];

  constructor(
    @Self() panel: PanelService,
    public refreshService: RefreshService,
    public navigationService: NavigationService,
    public deleteService: DeleteService,
    public editRoleService: EditRoleService
  ) {
    super(panel);
  }

  ngOnInit() {
    const objectType =
      this.self?.associationType.objectType ??
      this.other?.associationType.objectType;
    this.panel.name = this.other?.pluralName;
    this.panel.title = this.other?.pluralName;
    this.panel.icon = angularIcon(objectType as Composite);
    this.panel.expandable = true;

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
                propertyType: this.self,
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
      this.all = loaded.collection<IObject>(pullName) ?? [];
      this.active = this.all;
      this.inactive = this.all;
      // this.active = this.all.filter((v) =>
      //   v.Employer?.ActiveEmployments.includes(v)
      // );
      // this.inactive = this.all.filter((v) =>
      //   v.Employer?.InactiveEmployments.includes(v)
      // );

      this.table.total = (loaded.value(`${pullName}_total`) ??
        this.active?.length ??
        0) as number;
      this.refreshTable();
    };
  }

  public refreshTable() {
    this.table.data = this.relationships?.map((v) => {
      return {
        object: v,
        type: v.strategy.cls.singularName,
        // from: format(new Date(v.FromDate), 'dd-MM-yyyy'),
        // through:
        //   v.ThroughDate != null
        //     ? format(new Date(v.ThroughDate), 'dd-MM-yyyy')
        //     : '',
      } as Row;
    });
  }

  get relationships() {
    switch (this.period) {
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
      objectType: this.self.associationType.objectType as Composite,
      // associationId: this.panel.manager.id,
      // associationObjectType: this.panel.manager.objectType,
    };
  }
}
