import { Component, Input, ViewChild, OnInit } from '@angular/core';
import { MatSort } from '@angular/material/sort';

import { BaseTable } from './base-table';
import { Column } from './column';
import { TableRow } from './table-row';

@Component({
  // eslint-disable-next-line @angular-eslint/component-selector
  selector: 'a-mat-table',
  templateUrl: './table.component.html',
  styleUrls: ['./table.component.scss'],
})
export class AllorsMaterialTableComponent implements OnInit {
  @Input()
  public table: BaseTable;

  @ViewChild(MatSort, { static: true }) matSort: MatSort;

  ngOnInit(): void {
    this.table.init(this.matSort);
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
    return this.table && this.table.actions ? this.table.actions.map((v) => v.name).join() : '';
  }

  private action(row: TableRow, column: Column) {
    return column.action || this.table.defaultAction;
  }
}
