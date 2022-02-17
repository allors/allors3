import { format } from 'date-fns';
import { Component, HostBinding, Input } from '@angular/core';
import { Composite, RoleType } from '@allors/system/workspace/meta';
import {
  IObject,
  IPullResult,
  Pull,
  Initializer,
  Node,
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
  AllorsEditRelationshipPanelComponent,
  PanelService,
  ScopedService,
} from '@allors/base/workspace/angular/application';
import { DeleteService } from '../actions/delete/delete.service';
import { EditRoleService } from '../actions/edit-role/edit-role.service';
import { PeriodSelection } from '@allors/base/workspace/angular-material/foundation';
import { IconService } from '../icon/icon.service';
import { M } from '@allors/default/workspace/meta';

interface Row extends TableRow {
  object: IObject;
}

@Component({
  selector: 'a-mat-dyn-edit-relationship-panel',
  templateUrl: './dynamic-edit-relationship-panel.component.html',
})
export class AllorsMaterialDynamicEditRelationshipPanelComponent extends AllorsEditRelationshipPanelComponent {
  @HostBinding('class.expanded-panel')
  get expandedPanelClass() {
    return true;
    // return this.panel.isExpanded;
  }

  get panelId() {
    return this.target.name;
  }

  get icon() {
    return this.iconService.icon(this.objectType);
  }

  get titel() {
    return this.target.pluralName;
  }

  get initializer(): Initializer {
    return { propertyType: this.anchor, id: this.scoped.id };
  }

  @Input()
  anchor: RoleType;

  @Input()
  target: RoleType;

  m: M;

  objectType: Composite;
  targetObjectType: Composite;

  hasPeriod: boolean;
  periodSelection: PeriodSelection = PeriodSelection.Current;

  table: Table<Row>;
  delete: Action;
  edit: Action;

  objects: IObject[];
  filtered: IObject[];

  display: RoleType[];
  targetDisplay: RoleType[];

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
    this.objectType = this.target.associationType.objectType as Composite;
    this.targetObjectType = this.target.objectType as Composite;

    this.display = this.displayService.primary(this.objectType);
    const targetPrimaryDisplay = this.displayService.primary(
      this.targetObjectType
    );
    this.targetDisplay =
      targetPrimaryDisplay.length > 0
        ? targetPrimaryDisplay
        : [this.displayService.name(this.targetObjectType)];

    this.hasPeriod = this.objectType.supertypes.has(this.m.Period);

    this.delete = this.deleteService.delete();
    this.edit = this.editRoleService.edit();

    const sort = true;

    const tableConfig: TableConfig = {
      selection: true,
      columns: (this.objectType.isInterface
        ? [{ name: 'type', sort }]
        : []
      ).concat(
        [...this.targetDisplay, ...this.display].map((v) => {
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

    const displayInclude: Node[] = this.display
      .filter((v) => v.objectType.isComposite)
      .map((v) => {
        return {
          propertyType: v,
        };
      });

    const targetDisplyInclude: Node[] = this.targetDisplay
      .filter((v) => v.objectType.isComposite)
      .map((v) => {
        return {
          propertyType: v,
        };
      });

    const include = [
      ...displayInclude,
      {
        propertyType: this.target,
        nodes: targetDisplyInclude,
      },
    ];

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
          include,
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

      const target = v.strategy.getCompositeRole(this.target);

      for (const w of this.targetDisplay) {
        if (w.objectType.isUnit) {
          row[w.name] = target.strategy.getUnitRole(w);
        } else {
          const role = target.strategy.getCompositeRole(w);
          if (role) {
            const roleName = this.displayService.name(role.strategy.cls);
            row[w.name] = role.strategy.getUnitRole(roleName);
          } else {
            row[w.name] = '';
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
