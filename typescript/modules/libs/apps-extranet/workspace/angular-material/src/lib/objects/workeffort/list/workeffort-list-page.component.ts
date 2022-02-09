import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { PageEvent } from '@angular/material/paginator';
import { Sort } from '@angular/material/sort';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';
import { formatDistance } from 'date-fns';

import {
  DeleteService,
  OverviewService,
  SorterService,
  Table,
  TableRow,
} from '@allors/base/workspace/angular-material/application';
import { M } from '@allors/default/workspace/meta';
import {
  Filter,
  FilterField,
  FilterService,
  MediaService,
  RefreshService,
} from '@allors/base/workspace/angular/foundation';
import {
  NavigationService,
  Action,
  AllorsListPageComponent,
} from '@allors/base/workspace/angular/application';

import { ContextService } from '@allors/base/workspace/angular/foundation';

import { And } from '@allors/system/workspace/domain';

import { WorkEffort } from '@allors/default/workspace/domain';

interface Row extends TableRow {
  object: WorkEffort;
  number: string;
  name: string;
  state: string;
  equipment: string;
  executedBy: string;
  lastModifiedDate: string;
}

@Component({
  templateUrl: './workeffort-list-page.component.html',
  providers: [ContextService],
})
export class WorkEffortListPageComponent
  extends AllorsListPageComponent
  implements OnInit, OnDestroy
{
  table: Table<Row>;

  delete: Action;

  private subscription: Subscription;
  filter: Filter;

  constructor(
    @Self() allors: ContextService,
    titleService: Title,
    public refreshService: RefreshService,
    public overviewService: OverviewService,
    public deleteService: DeleteService,
    public navigation: NavigationService,
    public mediaService: MediaService,
    public filterService: FilterService,
    public sorterService: SorterService
  ) {
    super(allors, titleService);
    this.objectType = (this.m as unknown as M).WorkEffort;

    this.delete = deleteService.delete();
    this.delete.result.subscribe(() => {
      this.table.selection.clear();
    });

    this.table = new Table({
      selection: true,
      columns: [
        { name: 'number', sort: true },
        { name: 'name', sort: true },
        { name: 'state' },
        { name: 'executedBy' },
        { name: 'equipment' },
        { name: 'lastModifiedDate', sort: true },
      ],
      actions: [overviewService.overview(), this.delete],
      defaultAction: overviewService.overview(),
      pageSize: 50,
      initialSort: 'number',
      initialSortDirection: 'desc',
    });
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.filter = this.filterService.filter(m.WorkEffort);

    const predicate: And = {
      kind: 'And',
      operands: [this.filter.definition.predicate],
    };

    this.subscription = combineLatest([
      this.refreshService.refresh$,
      this.filter.fields$,
      this.table.sort$,
      this.table.pager$,
    ])
      .pipe(
        scan(
          (
            [previousRefresh, previousFilterFields],
            [refresh, filterFields, sort, pageEvent]
          ) => {
            pageEvent =
              previousRefresh !== refresh ||
              filterFields !== previousFilterFields
                ? {
                    ...pageEvent,
                    pageIndex: 0,
                  }
                : pageEvent;

            if (pageEvent.pageIndex === 0) {
              this.table.pageIndex = 0;
            }

            return [refresh, filterFields, sort, pageEvent];
          }
        ),
        switchMap(
          ([, filterFields, sort, pageEvent]: [
            Date,
            FilterField[],
            Sort,
            PageEvent
          ]) => {
            const pulls = [
              pull.WorkEffort({
                predicate,
                sorting: sort
                  ? this.sorterService.sorter(m.WorkEffort)?.create(sort)
                  : null,
                include: {
                  Customer: x,
                  ExecutedBy: x,
                  PrintDocument: {
                    Media: x,
                  },
                  WorkEffortState: x,
                  WorkEffortPurposes: x,
                  WorkEffortFixedAssetAssignmentsWhereAssignment: {
                    FixedAsset: x,
                  },
                  WorkEffortPartyAssignmentsWhereAssignment: {
                    Party: x,
                  },
                },
                arguments: this.filter.parameters(filterFields),
                skip: pageEvent.pageIndex * pageEvent.pageSize,
                take: pageEvent.pageSize,
              }),
            ];

            return this.allors.context.pull(pulls);
          }
        )
      )
      .subscribe((loaded) => {
        this.allors.context.reset();
        const workEfforts = loaded.collection<WorkEffort>(m.WorkEffort);
        this.table.total = loaded.value('WorkEfforts_total') as number;
        this.table.data = workEfforts
          ?.filter((v) => v.canReadWorkEffortNumber)
          ?.map((v) => {
            return {
              object: v,
              number: v.WorkEffortNumber,
              name: v.Name,
              state: v.WorkEffortState ? v.WorkEffortState.Name : '',
              executedBy: v.ExecutedBy ? v.ExecutedBy.DisplayName : '',
              equipment: v.WorkEffortFixedAssetAssignmentsWhereAssignment
                ? v.WorkEffortFixedAssetAssignmentsWhereAssignment?.map(
                    (w) => w.FixedAsset.DisplayName
                  ).join(', ')
                : '',
              lastModifiedDate: formatDistance(
                new Date(v.LastModifiedDate),
                new Date()
              ),
            } as Row;
          });
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
