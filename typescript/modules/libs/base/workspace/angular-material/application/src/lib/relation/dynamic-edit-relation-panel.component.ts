import {
  Component,
  HostBinding,
  Input,
  OnDestroy,
  OnInit,
} from '@angular/core';
import {
  AssociationType,
  Composite,
  PropertyType,
  RoleType,
} from '@allors/system/workspace/meta';
import {
  IPullResult,
  Pull,
  Initializer,
  IObject,
  Predicate,
} from '@allors/system/workspace/domain';
import {
  SharedPullService,
  RefreshService,
  WorkspaceService,
  TableConfig,
  Table,
  TableRow,
  DisplayService,
} from '@allors/base/workspace/angular/foundation';
import {
  NavigationService,
  AllorsEditRelationPanelComponent,
  PanelService,
  ScopedService,
} from '@allors/base/workspace/angular/application';
import { DeleteService } from '../actions/delete/delete.service';
import { EditRoleService } from '../actions/edit-role/edit-role.service';
import { IconService } from '../icon/icon.service';
import { delay, pipe, Subscription, tap } from 'rxjs';

@Component({
  selector: 'a-mat-dyn-edit-relation-panel',
  templateUrl: './dynamic-edit-relation-panel.component.html',
})
export class AllorsMaterialDynamicEditRelationPanelComponent
  extends AllorsEditRelationPanelComponent
  implements OnInit, OnDestroy
{
  @HostBinding('class.expanded-panel')
  get expandedPanelClass() {
    return true;
    // return this.panel.isExpanded;
  }

  get panelId() {
    return `${this.propertyType.name}`;
  }

  get icon() {
    return this.iconService.icon(this.propertyType.relationType);
  }

  get titel() {
    return this.propertyType.pluralName;
  }

  get initializer(): Initializer {
    return { propertyType: this.propertyType, id: this.scoped.id };
  }

  @Input()
  propertyType: PropertyType;

  propertyId: number;

  objectType: Composite;

  table: Table<TableRow>;

  display: RoleType[];

  private subscription: Subscription;

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

    this.subscription = this.scopedService.scoped$
      .pipe(
        pipe(delay(1)),
        tap((object) => {
          this.propertyId = object.id;
        }),
        tap(() => {
          this.refreshService.refresh();
        })
      )
      .subscribe();
  }

  ngOnInit() {
    this.objectType = this.propertyType.objectType as Composite;

    this.display = [
      ...this.displayService.primary(this.objectType),
      ...this.displayService.secondary(this.objectType),
    ].reduce((acc, e) => (e && !acc.includes(e) ? [...acc, e] : acc), []);

    // const delete = this.deleteService.delete();
    // const edit = this.editRoleService.edit();

    const sort = true;

    const columns = (
      this.objectType.isInterface ? [{ name: 'type', sort }] : []
    ).concat(
      this.display.map((v) => {
        return { name: v.name, sort };
      })
    );

    const tableConfig: TableConfig = {
      selection: true,
      columns,
      // actions: [edit, delete],
      // defaultAction: this.edit,
      autoSort: true,
      autoFilter: true,
    };

    this.table = new Table(tableConfig);

    this.refreshService.refresh();
  }

  onPreSharedPull(pulls: Pull[], prefix?: string) {
    let predicate: Predicate;

    if (this.propertyType) {
      const propertyType = this.propertyType.isRoleType
        ? (this.propertyType as RoleType).associationType
        : (this.propertyType as AssociationType).roleType;

      predicate = this.propertyType.isOne
        ? {
            kind: 'Equals',
            propertyType,
            objectId: this.propertyId,
          }
        : {
            kind: 'Contains',
            propertyType,
            objectId: this.propertyId,
          };
    }

    const pull: Pull = {
      extent: {
        kind: 'Filter',
        objectType: this.objectType,
        predicate,
      },
      results: [
        {
          name: prefix,
          include: this.display
            .filter((v) => v.objectType.isComposite)
            .map((v) => {
              return {
                propertyType: v,
              };
            }),
        },
      ],
    };

    pulls.push(pull);
  }

  onPostSharedPull(pullResult: IPullResult, prefix?: string) {
    const objects = pullResult.collection<IObject>(prefix);

    this.table.total = objects.length;
    this.table.data = objects.map((v) => {
      const row: TableRow = {
        object: v,
      };

      if (this.objectType.isInterface) {
        row['type'] = v.strategy.cls.singularName;
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

  toggle() {
    this.panelService.stopEdit().pipe().subscribe();
  }

  override ngOnDestroy(): void {
    super.ngOnDestroy();
    this.subscription?.unsubscribe();
  }
}
