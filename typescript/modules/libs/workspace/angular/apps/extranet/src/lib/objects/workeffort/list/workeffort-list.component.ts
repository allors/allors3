import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';
import { formatDistance } from 'date-fns';

import { M } from '@allors/workspace/meta/default';
import { WorkEffort } from '@allors/workspace/domain/default';
import { Action, DeleteService, Filter, MediaService, NavigationService, ObjectService, RefreshService, Table, TableRow, TestScope, OverviewService, angularFilterFromDefinition, angularSorter } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';

import { And, Equals } from '@allors/workspace/domain/system';

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
  templateUrl: './workeffort-list.component.html',
  providers: [ContextService],
})
export class WorkEffortListComponent extends TestScope implements OnInit, OnDestroy {
  public title = 'Work Orders';

  table: Table<Row>;

  delete: Action;

  private subscription: Subscription;
  filter: Filter;
  m: M;

  constructor(
    @Self() public allors: ContextService,

    public factoryService: ObjectService,
    public refreshService: RefreshService,
    public overviewService: OverviewService,
    public deleteService: DeleteService,
    public navigation: NavigationService,
    public mediaService: MediaService,
    titleService: Title
  ) {
    super();

    this.allors.context.name = this.constructor.name;
    titleService.setTitle(this.title);

    this.m = this.allors.context.configuration.metaPopulation as M;

    this.delete = deleteService.delete(allors.context);
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

    this.filter = angularFilterFromDefinition(m.WorkEffort);

    const predicate: And = { kind: 'And', operands: [this.filter.definition.predicate] };

    this.subscription = combineLatest([this.refreshService.refresh$, this.filter.fields$, this.table.sort$, this.table.pager$])
      .pipe(
        scan(([previousRefresh, previousFilterFields], [refresh, filterFields, sort, pageEvent]) => {
          pageEvent =
            previousRefresh !== refresh || filterFields !== previousFilterFields
              ? {
                  ...pageEvent,
                  pageIndex: 0,
                }
              : pageEvent;

          if (pageEvent.pageIndex === 0) {
            this.table.pageIndex = 0;
          }

          return [refresh, filterFields, sort, pageEvent];
        }),
        switchMap(([, filterFields, sort, pageEvent]) => {
          const pulls = [
            pull.WorkEffort({
              predicate,
              sorting: sort ? angularSorter(m.WorkEffort)?.create(sort) : null,
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
        })
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
              equipment: v.WorkEffortFixedAssetAssignmentsWhereAssignment ? v.WorkEffortFixedAssetAssignmentsWhereAssignment?.map((w) => w.FixedAsset.DisplayName).join(', ') : '',
              lastModifiedDate: formatDistance(new Date(v.LastModifiedDate), new Date()),
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
