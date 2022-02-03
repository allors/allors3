import { format } from 'date-fns';
import { Component, OnInit, HostBinding, Input } from '@angular/core';
import { Composite, RoleType } from '@allors/system/workspace/meta';
import {
  IObject,
  IPullResult,
  OnCreate,
  OnPull,
  Pull,
} from '@allors/system/workspace/domain';
import { Period } from '@allors/default/workspace/domain';
import {
  OnPullService,
  RefreshService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import {
  Action,
  NavigationService,
  AllorsEditRelationshipPanelComponent,
  PanelService,
  OverviewPageService,
  Panel,
} from '@allors/base/workspace/angular/application';
import { PeriodSelection } from '@allors/base/workspace/angular-material/foundation';
import { Table } from '../table/table';
import { TableRow } from '../table/table-row';
import { DeleteService } from '../actions/delete/delete.service';
import { EditRoleService } from '../actions/edit-role/edit-role.service';
import { angularIcon } from '../meta/angular-icon';
import { TableConfig } from '../table/table-config';

interface Row extends TableRow {
  object: IObject;
}

@Component({
  selector: 'a-mat-dyn-edit-rel-panel',
  templateUrl: './dynamic-edit-relationship-panel.component.html',
})
export class AllorsMaterialDynamicEditRelationshipPanelComponent
  extends AllorsEditRelationshipPanelComponent
  implements Panel, OnPull, OnInit
{
  @HostBinding('class.expanded-panel')
  get expandedPanelClass() {
    return true;
    // return this.panel.isExpanded;
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

  get panelId() {
    return this.target.name;
  }

  get icon() {
    return angularIcon(this.objectType);
  }

  get titel() {
    return this.target.pluralName;
  }

  constructor(
    overviewService: OverviewPageService,
    panelService: PanelService,
    onPullService: OnPullService,
    public workspaceService: WorkspaceService,
    public refreshService: RefreshService,
    public navigationService: NavigationService,
    public deleteService: DeleteService,
    public editRoleService: EditRoleService
  ) {
    super(overviewService, panelService, workspaceService);

    panelService.register(this);
    onPullService.register(this);
  }

  ngOnInit() {
    this.objectType = this.target.associationType.objectType as Composite;
    this.hasPeriod = this.objectType.supertypes.has(this.m.Period);

    this.delete = this.deleteService.delete();
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
  }

  onPrePull(pulls: Pull[], prefix?: string) {
    const id = this.overviewService.id;

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
          name: prefix,
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
  }

  onPostPull(pullResult: IPullResult, prefix?: string) {
    this.objects = pullResult.collection<IObject>(prefix) ?? [];
    this.updateFilter();
    this.refreshTable();
  }

  onPeriodSelectionChange(newPeriodSelection: PeriodSelection) {
    this.periodSelection = newPeriodSelection;

    if (this.objects != null) {
      this.updateFilter();
      this.refreshTable();
    }
  }

  private updateFilter() {
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

  private refreshTable() {
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

  get onObjectCreate(): OnCreate {
    return null;
    // return new OnObjectCreateRole(this.anchor, this.panel.manager.id);
  }

  toggle() {
    this.panelService.stopEdit().subscribe();
  }
}
