// tslint:disable: directive-selector
// tslint:disable: directive-class-suffix
import { PageEvent } from '@angular/material/paginator';
import { Sort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { SelectionModel } from '@angular/cdk/collections';
import { BehaviorSubject } from 'rxjs';

import { IObject } from '@allors/workspace/domain/system';

import { Action } from '../../../components/actions/Action';
import { Column } from './column';
import { TableRow } from './TableRow';

export interface BaseTable {
  columns: Column[];
  dataSource: MatTableDataSource<TableRow>;
  selection: SelectionModel<TableRow>;
  actions: Action[];
  defaultAction?: Action;
  pageIndex: number;
  pageSize?: number;
  pageSizeOptions?: number[];

  sort$: BehaviorSubject<Sort | null>;
  pager$: BehaviorSubject<PageEvent>;

  total: number;

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
