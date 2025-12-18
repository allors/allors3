import { format } from 'date-fns';
import { Component, HostBinding, OnInit } from '@angular/core';
import {
  AssociationType,
  Composite,
  humanize,
  RoleType,
  Unit,
} from '@allors/system/workspace/meta';
import {
  IObject,
  IPullResult,
  Pull,
  Initializer,
  Node,
  SharedPullHandler,
  selectLeaf,
  toSelect,
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
  MetaService,
  ActionService,
} from '@allors/base/workspace/angular/foundation';
import {
  NavigationService,
  AllorsDynamicEditExtentPanelComponent,
  PanelService,
  ScopedService,
} from '@allors/base/workspace/angular/application';
import { DeleteActionService } from '../actions/delete/delete-action.service';
import { IconService } from '../icon/icon.service';
import { M } from '@allors/default/workspace/meta';
import { PeriodSelection } from '@allors/base/workspace/angular-material/foundation';
import { ViewActionService } from '../actions/view/view-action.service';

interface Row extends TableRow {
  object: IObject;
}

@Component({
  selector: 'a-mat-dyn-edit-extent-panel',
  templateUrl: './dynamic-edit-extent-panel.component.html',
})
export class AllorsMaterialDynamicEditExtentPanelComponent
  extends AllorsDynamicEditExtentPanelComponent
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
    return this.objectType?.supertypes.has(this.m.Period);
  }

  m: M;

  periodSelection: PeriodSelection = PeriodSelection.Current;

  table: Table<Row>;
  delete: Action;
  view: Action;

  objects: IObject[];
  filtered: IObject[];

  display: RoleType[];
  includeDisplay: RoleType[];

  constructor(
    scopedService: ScopedService,
    panelService: PanelService,
    sharedPullService: SharedPullService,
    refreshService: RefreshService,
    metaService: MetaService,
    workspaceService: WorkspaceService,
    public navigation: NavigationService,
    public deleteService: DeleteActionService,
    public viewService: ViewActionService,
    private iconService: IconService,
    private displayService: DisplayService,
    private actionService: ActionService
  ) {
    super(
      scopedService,
      panelService,
      sharedPullService,
      refreshService,
      metaService
    );
    this.m = workspaceService.workspace.configuration.metaPopulation as M;

    panelService.register(this);
    sharedPullService.register(this);
  }

  ngOnInit() {
    this.display = this.displayService.primary(this.objectType);

    if (this.include) {
      const includeObjectType = this.include.objectType as Composite;
      this.includeDisplay = this.displayService.primary(includeObjectType);
    } else {
      this.includeDisplay = [];
    }

    this.delete = this.deleteService.delete();
    this.view = this.viewService.view();

    const sort = true;

    const objectTypeActions = this.actionService.action(this.objectType);
    const actions = [this.view, ...objectTypeActions, this.delete];

    const tableConfig: TableConfig = {
      selection: true,
      columns: (this.objectType.isInterface ? [{ name: 'type', sort }] : [])
        .concat(this.display.length === 0 ? [{ name: 'name', sort }] : [])
        .concat(
          [...this.includeDisplay, ...this.display].map((v) => {
            return { name: v.name, sort };
          })
        ),
      actions,
      defaultAction: this.view,
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
    if (this.panelEnabled) {
      const id = this.scoped.id;

      const displayInclude: Node[] = this.display
        ?.filter((v) => v.objectType.isComposite)
        .map((v) => {
          return {
            propertyType: v,
          };
        });

      let include = displayInclude ? [...displayInclude] : [];

      if (this.include) {
        include = include.concat({
          propertyType: this.include,
          nodes:
            this.includeDisplay?.length > 0
              ? this.includeDisplay
                  .filter((v) => v.objectType.isComposite)
                  .map((v) => {
                    return {
                      propertyType: v,
                    };
                  })
              : null,
        });
      }

      const results = this.selectAsPaths.map((v) => {
        const select = toSelect(v);
        const leaf = selectLeaf(select);
        leaf.include = include;
        return {
          name: prefix,
          select,
        };
      });

      const pull: Pull = {
        objectId: id,
        results,
      };

      pulls.push(pull);
    }
  }

  onPostSharedPull(pullResult: IPullResult, prefix?: string) {
    if (this.panelEnabled) {
      this.enabled = this.enabler ? this.enabler() : true;
      this.creatable = this.creatableFn ? this.creatableFn() : true;

      this.objects = pullResult.collection<IObject>(prefix) ?? [];
      this.updateFilter();
      this.refreshTable();
    }
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
    this.table.data = this.filtered.map((v) => {
      const row: TableRow = {
        object: v,
      };
      if (this.objectType.isInterface) {
        row['type'] = humanize(v.strategy.cls.singularName);
      }

      if (this.display.length === 0) {
        const display = this.displayService.name(v.strategy.cls);
        if (display != null) {
          row['name'] = v.strategy.getUnitRole(display);
        }
      }

      if (this.include && this.include.isOne) {
        const include = this.include.isRoleType
          ? v.strategy.getCompositeRole(this.include as RoleType)
          : v.strategy.getCompositeAssociation(this.include as AssociationType);
        for (const w of this.includeDisplay) {
          if (w.objectType.isUnit) {
            const unit = w.objectType as Unit;
            const value = include.strategy.getUnitRole(w) as any;
            row[w.name] = unit.isDateTime ? format(value, 'dd-MM-yyyy') : value;
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
          const unit = w.objectType as Unit;
          const value = v.strategy.getUnitRole(w) as any;
          row[w.name] = unit.isDateTime ? format(value, 'dd-MM-yyyy') : value;
        } else {
          if (w.isOne) {
            const composite = v.strategy.getCompositeRole(w);
            if (composite) {
              const roleName = this.displayService.name(composite.strategy.cls);
              row[w.name] = composite.strategy.getUnitRole(roleName);
            } else {
              row[w.name] = '';
            }
          } else {
            const composites = v.strategy.getCompositesRole(w);
            if (composites.length > 0) {
              row[w.name] = composites
                .map((v) => {
                  const display = this.displayService.name(v.strategy.cls);
                  return v.strategy.getUnitRole(display);
                })
                .join(', ');
            } else {
              row[w.name] = '';
            }
          }
        }
      }
      if (this.hasPeriod) {
        const fromDate = v.strategy.getUnitRole(this.m.Period.FromDate) as Date;
        row['from'] = fromDate != null ? format(fromDate, 'dd-MM-yyyy') : '';
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
