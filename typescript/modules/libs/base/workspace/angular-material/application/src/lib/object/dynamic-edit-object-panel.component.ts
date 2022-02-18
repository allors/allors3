import { Component, HostBinding, Input } from '@angular/core';
import {
  Composite,
  PropertyType,
  RoleType,
} from '@allors/system/workspace/meta';
import {
  IObject,
  IPullResult,
  Pull,
  Initializer,
  Node,
  Path,
  leafPath,
} from '@allors/system/workspace/domain';
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
  AllorsEditObjectPanelComponent,
  PanelService,
  ScopedService,
} from '@allors/base/workspace/angular/application';
import { DeleteService } from '../actions/delete/delete.service';
import { EditRoleService } from '../actions/edit-role/edit-role.service';
import { IconService } from '../icon/icon.service';

interface Row extends TableRow {
  object: IObject;
}

@Component({
  selector: 'a-mat-dyn-edit-object-panel',
  templateUrl: './dynamic-edit-object-panel.component.html',
})
export class AllorsMaterialDynamicEditObjectPanelComponent extends AllorsEditObjectPanelComponent {
  @HostBinding('class.expanded-panel')
  get expandedPanelClass() {
    return true;
    // return this.panel.isExpanded;
  }

  get panelId() {
    return `${this.tag}`;
  }
  get icon() {
    return this.iconService.icon(this.objectType);
  }

  get titel() {
    return this.objectType.pluralName;
  }

  get initializer(): Initializer[] {
    if (Array.isArray(this.anchor)) {
      return this.anchor.map((v) => {
        return { propertyType: v, id: this.scoped.id };
      });
    } else {
      return [{ propertyType: this.anchor, id: this.scoped.id }];
    }
  }

  @Input()
  anchor: PropertyType | PropertyType[];

  @Input()
  target: PropertyType | Path | (PropertyType | Path)[];

  leaf: Path;
  objectType: Composite;
  tag: string;

  title: string;
  description: string;

  table: Table<Row>;
  delete: Action;
  edit: Action;

  objects: IObject[];

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

    panelService.register(this);
    sharedPullService.register(this);
  }

  ngOnInit() {
    this.leaf = leafPath(this.target);
    this.objectType = this.leaf.propertyType.objectType as Composite;

    this.display = this.displayService.primary(this.objectType);
    const targetPrimaryDisplay = this.displayService.primary(this.objectType);
    this.targetDisplay =
      targetPrimaryDisplay.length > 0
        ? targetPrimaryDisplay
        : [this.displayService.name(this.objectType)];

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
          // include,
        },
      ],
    };

    pulls.push(pull);
  }

  onPostSharedPull(pullResult: IPullResult, prefix?: string) {
    this.objects = pullResult.collection<IObject>(prefix) ?? [];
    this.refreshTable();
  }

  toggle() {
    this.panelService.stopEdit().subscribe();
  }

  private refreshTable() {
    this.table.total = this.objects.length;

    this.table.data = this.objects.map((v) => {
      const row: TableRow = {
        object: v,
      };

      if (this.objectType.isInterface) {
        row['type'] = v.strategy.cls.singularName;
      }

      // const target = v.strategy.getCompositeRole(this.target);
      const target = v;

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

      return row;
    });
  }
}
