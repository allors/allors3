import { format } from 'date-fns';
import { Component, HostBinding, OnInit } from '@angular/core';
import {
  AssociationType,
  Composite,
  RoleType,
} from '@allors/system/workspace/meta';
import {
  IObject,
  IPullResult,
  Pull,
  Initializer,
  Node,
  isPath,
  toNode,
  SharedPullHandler,
  selectLeaf,
} from '@allors/system/workspace/domain';
import { Period } from '@allors/default/workspace/domain';
import {
  Action,
  SharedPullService,
  RefreshService,
  WorkspaceService,
  DisplayService,
  TableRow,
  Table,
  TableConfig,
} from '@allors/base/workspace/angular/foundation';
import {
  NavigationService,
  AllorsEditExtentPanelComponent,
  PanelService,
  ScopedService,
} from '@allors/base/workspace/angular/application';
import { DeleteService } from '../actions/delete/delete.service';
import { EditRoleService } from '../actions/edit-role/edit-role.service';
import { IconService } from '../icon/icon.service';
import { M } from '@allors/default/workspace/meta';
import { PeriodSelection } from '@allors/base/workspace/angular-material/foundation';

interface Row extends TableRow {
  object: IObject;
}

@Component({
  selector: 'a-mat-dyn-edit-extent-panel',
  templateUrl: './dynamic-edit-extent-panel.component.html',
})
export class AllorsMaterialDynamicEditExtentPanelComponent
  extends AllorsEditExtentPanelComponent
  implements SharedPullHandler, OnInit
{
  @HostBinding('class.expanded-panel')
  get expandedPanelClass() {
    return true;
    // return this.panel.isExpanded;
  }

  get icon() {
    return this.iconService.icon(this.objectType);
  }

  get initializer(): Initializer {
    return { propertyType: this.init, id: this.scoped.id };
  }

  get hasPeriod(): boolean {
    return this.objectType.supertypes.has(this.m.Period);
  }

  m: M;

  periodSelection: PeriodSelection = PeriodSelection.Current;

  table: Table<Row>;
  delete: Action;
  edit: Action;

  objects: IObject[];
  filtered: IObject[];

  display: RoleType[];
  includeDisplay: RoleType[];

  constructor(
    scopedService: ScopedService,
    panelService: PanelService,
    sharedPullService: SharedPullService,
    refreshService: RefreshService,
    workspaceService: WorkspaceService,
    public navigationService: NavigationService,
    public deleteService: DeleteService,
    public editRoleService: EditRoleService,
    private iconService: IconService,
    private displayService: DisplayService
  ) {
    super(scopedService, panelService, sharedPullService, refreshService);
    this.m = workspaceService.workspace.configuration.metaPopulation as M;

    panelService.register(this);
    sharedPullService.register(this);
  }

  ngOnInit() {
    this.display = this.displayService.primary(this.objectType);

    if (this.include) {
      const includeObjectType = this.include.objectType as Composite;
      const includePrimaryDisplay =
        this.displayService.primary(includeObjectType);

      this.includeDisplay =
        includePrimaryDisplay.length > 0
          ? includePrimaryDisplay
          : [this.displayService.name(includeObjectType)];
    } else {
      this.includeDisplay = [];
    }

    this.delete = this.deleteService.delete();
    this.edit = this.editRoleService.edit();

    const sort = true;

    const tableConfig: TableConfig = {
      selection: true,
      columns: (this.objectType.isInterface
        ? [{ name: 'type', sort }]
        : []
      ).concat(
        [...this.includeDisplay, ...this.display].map((v) => {
          return { name: v.name, sort };
        })
      ),
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

  onPreSharedPull(pulls: Pull[], prefix?: string) {
    const id = this.scoped.id;

    const select: Node = isPath(this.select)
      ? toNode(this.select)
      : { propertyType: this.select };

    const displayInclude: Node[] = this.display
      .filter((v) => v.objectType.isComposite)
      .map((v) => {
        return {
          propertyType: v,
        };
      });

    const includeDisplyInclude: Node[] = this.includeDisplay
      .filter((v) => v.objectType.isComposite)
      .map((v) => {
        return {
          propertyType: v,
        };
      });

    const include = [...displayInclude];

    if (this.include) {
      include.concat({
        propertyType: this.include,
        nodes: includeDisplyInclude,
      });
    }

    const leaf = selectLeaf(select);
    leaf.include = include;

    const pull: Pull = {
      objectId: id,
      results: [
        {
          name: prefix,
          select,
        },
      ],
    };

    pulls.push(pull);
  }

  onPostSharedPull(pullResult: IPullResult, prefix?: string) {
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

  toggle() {
    this.panelService.stopEdit().subscribe();
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
      const row: TableRow = {
        object: v,
      };
      if (this.objectType.isInterface) {
        row['type'] = v.strategy.cls.singularName;
      }

      if (this.include) {
        const include = this.include.isRoleType
          ? v.strategy.getCompositeRole(this.include as RoleType)
          : v.strategy.getCompositeAssociation(this.include as AssociationType);
        for (const w of this.includeDisplay) {
          if (w.objectType.isUnit) {
            row[w.name] = include.strategy.getUnitRole(w);
          } else {
            const role = include.strategy.getCompositeRole(w);
            if (role) {
              const roleName = this.displayService.name(role.strategy.cls);
              row[w.name] = role.strategy.getUnitRole(roleName);
            } else {
              row[w.name] = '';
            }
          }
        }
      }

      for (const w of this.display) {
        if (w.objectType.isUnit) {
          row[w.name] = v.strategy.getUnitRole(w);
        } else {
          const role = v.strategy.getCompositeRole(w);
          if (role) {
            const roleName = this.displayService.name(role.strategy.cls);
            row[w.name] = role.strategy.getUnitRole(roleName);
          } else {
            row[w.name] = '';
          }
        }
      }
      if (this.hasPeriod) {
        const fromDate = v.strategy.getUnitRole(this.m.Period.FromDate) as Date;
        row['from'] = format(fromDate, 'dd-MM-yyyy');
        const throughDate = v.strategy.getUnitRole(
          this.m.Period.ThroughDate
        ) as Date;
        row['through'] =
          throughDate != null ? format(throughDate, 'dd-MM-yyyy') : '';
      }
      return row;
    });
  }
}
