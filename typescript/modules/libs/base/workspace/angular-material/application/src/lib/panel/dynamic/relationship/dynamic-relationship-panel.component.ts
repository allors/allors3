import { format } from 'date-fns';
import { Component, Self, OnInit, HostBinding, Input } from '@angular/core';
import { Composite, RoleType } from '@allors/system/workspace/meta';
import { IObject, Pull } from '@allors/system/workspace/domain';
import {
  CreateRequest,
  CreateRequestArgument,
  RefreshService,
} from '@allors/base/workspace/angular/foundation';
import {
  Action,
  NavigationService,
  PanelService,
  AllorsPanelRelationshipComponent,
} from '@allors/base/workspace/angular/application';
import { PeriodSelection } from '@allors/base/workspace/angular-material/foundation';
import { angularIcon } from '../../../meta/angular-icon';
import { Table } from '../../../table/table';
import { TableRow } from '../../../table/table-row';
import { DeleteService } from '../../../actions/delete/delete.service';
import { EditRoleService } from '../../../actions/edit-role/edit-role.service';
import { Period } from '@allors/default/workspace/domain';
import { TableConfig } from '../../../table/table-config';

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
  anchor: RoleType;

  @Input()
  target: RoleType;

  @Input()
  display: RoleType;

  objectType: Composite;

  hasPeriod: boolean;
  periodSelection: PeriodSelection = PeriodSelection.Current;

  table: Table<Row>;
  delete: Action;
  edit: Action;

  objects: IObject[];
  filtered: IObject[];

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
    this.objectType = this.target.associationType.objectType as Composite;
    this.hasPeriod = this.objectType.supertypes.has(this.m.Period);

    this.panel.name = this.target.pluralName;
    this.panel.title = this.target.pluralName;
    this.panel.icon = angularIcon(this.objectType);
    this.panel.expandable = true;

    this.delete = this.deleteService.delete(this.panel.manager.context);
    this.edit = this.editRoleService.edit();

    const sort = true;

    const tableConfig: TableConfig = {
      selection: true,
      columns: [{ name: this.target.name, sort }],
      actions: [this.edit, this.delete],
      defaultAction: this.edit,
      autoSort: true,
      autoFilter: true,
    };

    if (this.hasPeriod) {
      tableConfig.columns.push(
        ...[
          { name: 'from', sort },
          { name: 'through', sort },
        ]
      );
    }

    this.table = new Table(tableConfig);

    const pullName = `${this.panel.name}_${this.m.Employment.tag}`;

    this.panel.onPull = (pulls) => {
      const id = this.panel.manager.id;

      const pull: Pull = {
        extent: {
          kind: 'Filter',
          objectType: this.objectType,
          predicate: {
            kind: 'Equals',
            propertyType: this.anchor,
            value: id,
          },
        },
        results: [
          {
            name: pullName,
            include: [
              {
                propertyType: this.anchor,
              },
              {
                propertyType: this.target,
              },
            ],
          },
        ],
      };

      pulls.push(pull);
    };

    this.panel.onPulled = (loaded) => {
      this.objects = loaded.collection<IObject>(pullName) ?? [];
      this.updateFilter();
      this.refreshTable();
    };
  }

  onPeriodSelectionChange(newPeriodSelection: PeriodSelection) {
    this.periodSelection = newPeriodSelection;
    this.updateFilter();
    this.refreshTable();
  }

  updateFilter() {
    if (!this.hasPeriod) {
      this.filtered = this.objects;
      return;
    }

    const now = new Date(Date.now());
    switch (this.periodSelection) {
      case PeriodSelection.Current:
        this.filtered = this.objects.filter((v: Period) => {
          if (v.ThroughDate) {
            return v.FromDate < now && v.ThroughDate > now;
          } else {
            return v.FromDate < now;
          }
        });
        break;
      case PeriodSelection.Inactive:
        this.filtered = this.objects.filter((v: Period) => {
          if (v.ThroughDate) {
            return v.FromDate > now || v.ThroughDate < now;
          } else {
            return v.FromDate > now;
          }
        });
        break;
      default:
        this.filtered = this.objects;
        break;
    }
  }

  refreshTable() {
    this.table.total = this.filtered.length;
    this.table.data = this.filtered.map((v) => {
      const target = v.strategy.getCompositeRole(this.target);
      const targetDisplay = target.strategy
        .getUnitRole(this.display)
        ?.toString(); // TODO: Use relation

      let from: string;
      let through: string;
      if (this.hasPeriod) {
        const fromDate = v.strategy.getUnitRole(this.m.Period.FromDate) as Date;
        from = format(fromDate, 'dd-MM-yyyy');
        const throughDate = v.strategy.getUnitRole(
          this.m.Period.ThroughDate
        ) as Date;
        through = throughDate != null ? format(throughDate, 'dd-MM-yyyy') : '';
      }

      return {
        object: v,
        [this.target.name]: targetDisplay,
        type: v.strategy.cls.singularName,
        from,
        through,
      } as Row;
    });
  }

  get createRequestArguments(): CreateRequestArgument[] {
    return [
      {
        kind: 'RoleArgument',
        roleType: this.anchor,
        role: this.panel.manager.id,
      },
    ];
  }
}
