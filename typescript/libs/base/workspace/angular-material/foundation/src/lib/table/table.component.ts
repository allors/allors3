import { Component, Input, ViewChild, AfterViewInit } from '@angular/core';
import { MatSort } from '@angular/material/sort';
import {
  AllorsComponent,
  BaseTable,
  Column,
  TableRow,
} from '@allors/base/workspace/angular/foundation';

@Component({
  selector: 'a-mat-table',
  templateUrl: './table.component.html',
  styleUrls: ['./table.component.scss'],
})
export class AllorsMaterialTableComponent
  extends AllorsComponent
  implements AfterViewInit
{
  @Input()
  public table: BaseTable;

  @ViewChild(MatSort) sort: MatSort;

  ngAfterViewInit(): void {
    this.table.init(this.sort);
  }

  cellStyle(row: TableRow, column: Column) {
    return this.action(row, column) ? 'pointer' : null;
  }

  onCellClick(row: TableRow, column: Column) {
    const action = this.action(row, column);
    if (action && !action.disabled(row.object)) {
      action.execute(row.object);
    }
  }

  get dataAllorsActions() {
    return this.table && this.table.actions
      ? this.table.actions.map((v) => v.name).join()
      : '';
  }

  private action(row: TableRow, column: Column) {
    return column.action || this.table.defaultAction;
  }
}
