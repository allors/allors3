import { Sort, SortDirection } from '@angular/material/sort';
import { Action } from '@allors/workspace/angular/base';
import { Column } from './column';

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
