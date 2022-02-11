import {
  DisplayService,
  RefreshService,
  SharedPullService,
  Table,
  TableConfig,
  TableRow,
} from '@allors/base/workspace/angular/foundation';
import {
  And,
  Contains,
  Equals,
  IObject,
  IPullResult,
  Predicate,
  Pull,
  ScopedPullHandler,
} from '@allors/system/workspace/domain';
import {
  AssociationType,
  Composite,
  PropertyType,
  RoleType,
} from '@allors/system/workspace/meta';
import { Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'a-mat-dyn-table',
  templateUrl: './dynamic-table.component.html',
})
export class AllorsMaterialDynamicTableComponent
  implements ScopedPullHandler, OnInit
{
  @Input()
  objectType: Composite;

  @Input()
  propertyType: PropertyType;

  @Input()
  propertyId: number;

  table: Table<TableRow>;

  constructor(
    sharedPullService: SharedPullService,
    private displayService: DisplayService,
    private refreshService: RefreshService
  ) {
    sharedPullService.register(this);
  }

  ngOnInit(): void {
    // const delete = this.deleteService.delete();
    // const edit = this.editRoleService.edit();

    const sort = true;

    const tableConfig: TableConfig = {
      selection: true,
      columns: [{ name: 'display', sort }],
      // actions: [edit, delete],
      // defaultAction: this.edit,
      autoSort: true,
      autoFilter: false,
    };

    this.table = new Table(tableConfig);

    this.refreshService.refresh();
  }

  onPreScopedPull(pulls: Pull[], scope?: string) {
    const and: And = { kind: 'And', operands: [] };

    if (this.propertyType) {
      const propertyType = this.propertyType.isRoleType
        ? (this.propertyType as RoleType).associationType
        : (this.propertyType as AssociationType).roleType;

      const predicate: Predicate = this.propertyType.isOne
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

      and.operands.push(predicate);
    }

    const pull: Pull = {
      extent: {
        kind: 'Filter',
        objectType: this.objectType,
        predicate: and,
      },
      results: [{ name: scope }],
    };

    pulls.push(pull);
  }

  onPostScopedPull(pullResult: IPullResult, scope?: string) {
    const objects = pullResult.collection<IObject>(scope);

    this.table.total = objects.length;
    this.table.data = objects.map((v) => {
      const display = this.displayService.display(v);

      return {
        object: v,
        display,
      } as TableRow;
    });
  }
}
