import { Sort, SortDirection } from '@angular/material/sort';
import { Action } from '@allors/base/workspace/angular/application';
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
