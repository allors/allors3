// tslint:disable: directive-selector
// tslint:disable: directive-class-suffix
import { PageEvent } from '@angular/material/paginator';
import { Sort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { SelectionModel } from '@angular/cdk/collections';
import { BehaviorSubject } from 'rxjs';
import { IObject } from '@allors/system/workspace/domain';
import { Column } from './column';
import { TableRow } from './table-row';
import { Action } from '../action/action';

export interface BaseTable {
  columns: Column[];
  dataSource: MatTableDataSource<TableRow>;
  selection: SelectionModel<TableRow>;
  actions: Action[];
  defaultAction?: Action;

  sort$: BehaviorSubject<Sort | null>;

  pageIndex: number;
  pageFill: number;
  pageSize?: number;
  pageSizeOptions?: number[];
  pager$: BehaviorSubject<PageEvent>;

  autoFilter: boolean;

  sortValue: Sort | null;

  hasActions: boolean;

  columnNames: string[];

  anySelected: boolean;

  allSelected: boolean;

  selected: IObject[];

  init(matSort: any): void;

  masterToggle(): void;

  page(event: PageEvent): void;

  sort(event: Sort): void;

  filter(event: any): void;
}
