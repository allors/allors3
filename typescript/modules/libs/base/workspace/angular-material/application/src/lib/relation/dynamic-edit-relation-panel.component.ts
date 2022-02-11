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
  Node,
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
  ObjectService,
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
    return { propertyType: this.propertyType, id: this.objectInfo.id };
  }

  @Input()
  propertyType: PropertyType;

  propertyId: number;

  objectType: Composite;

  table: Table<TableRow>;

  display: RoleType[];

  private subscription: Subscription;

  constructor(
    objectService: ObjectService,
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
    super(
      objectService,
      panelService,
      sharedPullService,
      refreshService,
      workspaceService
    );

    panelService.register(this);
    sharedPullService.register(this);

    this.subscription = this.objectService.objectInfo$
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

    const primary = this.displayService.primary(this.objectType);

    if (primary?.length > 0) {
      this.display = [...primary];
      const secondary = this.displayService.secondary(this.objectType);
      if (secondary?.length > 0) {
        this.display.push(...secondary);
      }
    } else {
      this.display = [this.displayService.name(this.objectType)];
    }

    // const delete = this.deleteService.delete();
    // const edit = this.editRoleService.edit();

    const sort = true;

    const tableConfig: TableConfig = {
      selection: true,
      columns: this.display.map((v) => {
        return { name: v.name, sort };
      }),
      // actions: [edit, delete],
      // defaultAction: this.edit,
      autoSort: true,
      autoFilter: true,
    };

    this.table = new Table(tableConfig);

    this.refreshService.refresh();
  }

  onPreScopedPull(pulls: Pull[], scope?: string) {
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
          name: scope,
          include: this.display.map((v) => {
            return { propertyType: v };
          }),
        },
      ],
    };

    pulls.push(pull);
  }

  onPostScopedPull(pullResult: IPullResult, scope?: string) {
    const objects = pullResult.collection<IObject>(scope);

    this.table.total = objects.length;
    this.table.data = objects.map((v) => {
      const row: TableRow = {
        object: v,
      };

      for (const w of this.display) {
        if (w.objectType.isUnit) {
          row[w.name] = v.strategy.getUnitRole(w);
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
