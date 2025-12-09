import { Sort, SortDirection } from '@angular/material/sort';
import { Column } from './column';
import { Action } from '../action/action';

export interface TableConfig {
  columns?: (Partial<Column> | string)[];

  selection?: boolean;

  actions?: Action[];

  defaultAction?: Action;

  autoSort?: boolean;

  initialSort?: Partial<Sort> | string;

  initialSortDirection?: SortDirection;

  pageSize?: number;

  pageSizeOptions?: number[];

  autoFilter?: boolean;
}
