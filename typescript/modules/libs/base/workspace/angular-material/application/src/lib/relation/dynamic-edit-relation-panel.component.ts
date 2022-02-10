import { Component, HostBinding, Input } from '@angular/core';
import {
  AssociationType,
  Composite,
  PropertyType,
  RoleType,
} from '@allors/system/workspace/meta';
import {
  IObject,
  IPullResult,
  Pull,
  Initializer,
} from '@allors/system/workspace/domain';
import {
  SharedPullService,
  RefreshService,
  WorkspaceService,
  DisplayService,
} from '@allors/base/workspace/angular/foundation';
import {
  Action,
  NavigationService,
  AllorsEditRelationPanelComponent,
  PanelService,
  ObjectService,
} from '@allors/base/workspace/angular/application';
import { Table } from '../table/table';
import { TableRow } from '../table/table-row';
import { DeleteService } from '../actions/delete/delete.service';
import { EditRoleService } from '../actions/edit-role/edit-role.service';
import { TableConfig } from '../table/table-config';
import { IconService } from '../icon/icon.service';

interface Row extends TableRow {
  object: IObject;
}

@Component({
  selector: 'a-mat-dyn-edit-relation-panel',
  templateUrl: './dynamic-edit-relation-panel.component.html',
})
export class AllorsMaterialDynamicEditRelationPanelComponent extends AllorsEditRelationPanelComponent {
  @HostBinding('class.expanded-panel')
  get expandedPanelClass() {
    return true;
    // return this.panel.isExpanded;
  }

  @Input()
  propertyType: PropertyType;

  objectType: Composite;

  table: Table<Row>;
  delete: Action;
  edit: Action;

  object: IObject;
  properties: IObject[];

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
  }

  ngOnInit() {
    this.objectType = this.propertyType.objectType as Composite;

    this.delete = this.deleteService.delete();
    this.edit = this.editRoleService.edit();

    const sort = true;

    const tableConfig: TableConfig = {
      selection: true,
      columns: [{ name: this.propertyType.name, sort }],
      actions: [this.edit, this.delete],
      defaultAction: this.edit,
      autoSort: true,
      autoFilter: true,
    };

    this.table = new Table(tableConfig);
  }

  onPreScopedPull(pulls: Pull[], scope?: string) {
    const id = this.objectInfo.id;

    const pull: Pull = {
      objectId: id,
      results: [
        {
          name: scope,
          include: [
            {
              propertyType: this.propertyType,
            },
          ],
        },
      ],
    };

    pulls.push(pull);
  }

  onPostScopedPull(pullResult: IPullResult, scope?: string) {
    this.object = pullResult.object<IObject>(scope);

    if (this.propertyType.isAssociationType) {
      this.properties = this.object.strategy.getCompositesAssociation(
        this.propertyType as AssociationType
      ) as IObject[];
    } else {
      this.properties = this.object.strategy.getCompositesRole(
        this.propertyType as RoleType
      ) as IObject[];
    }

    this.refreshTable();
  }

  toggle() {
    this.panelService.stopEdit().subscribe();
  }

  private refreshTable() {
    this.table.total = this.properties.length;
    this.table.data = this.properties.map((v) => {
      const display = this.displayService.display(v);

      return {
        object: v,
        [this.propertyType.name]: display,
        type: v.strategy.cls.singularName,
      } as Row;
    });
  }
}
