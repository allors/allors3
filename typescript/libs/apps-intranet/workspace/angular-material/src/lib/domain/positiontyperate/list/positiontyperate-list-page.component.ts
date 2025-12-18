import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';
import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { PageEvent } from '@angular/material/paginator';
import { Title } from '@angular/platform-browser';
import { Sort } from '@angular/material/sort';

import { M } from '@allors/default/workspace/meta';
import {
  PositionType,
  PositionTypeRate,
} from '@allors/default/workspace/domain';
import {
  Action,
  Filter,
  FilterField,
  FilterService,
  MediaService,
  RefreshService,
  Table,
  TableRow,
} from '@allors/base/workspace/angular/foundation';
import { NavigationService } from '@allors/base/workspace/angular/application';
import {
  DeleteActionService,
  EditActionService,
  OverviewActionService,
  SorterService,
} from '@allors/base/workspace/angular-material/application';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { format } from 'date-fns';

interface Row extends TableRow {
  object: PositionTypeRate;
  positionType: string;
  rateType: string;
  from: string;
  through: string;
  rate: string;
  frequency: string;
}

@Component({
  templateUrl: './positiontyperate-list-page.component.html',
  providers: [ContextService],
})
export class PositionTypeRateListPageComponent implements OnInit, OnDestroy {
  public title = 'Position Type Rates';

  table: Table<Row>;

  edit: Action;
  delete: Action;

  private subscription: Subscription;
  positionTypes: PositionType[];
  filter: Filter;
  m: M;

  constructor(
    @Self() public allors: ContextService,
    public refreshService: RefreshService,
    public overviewService: OverviewActionService,
    public editRoleService: EditActionService,
    public deleteService: DeleteActionService,
    public navigation: NavigationService,
    public mediaService: MediaService,
    public filterService: FilterService,
    public sorterService: SorterService,
    titleService: Title
  ) {
    this.allors.context.name = this.constructor.name;
    titleService.setTitle(this.title);

    this.m = this.allors.context.configuration.metaPopulation as M;

    this.edit = editRoleService.edit();
    this.edit.result.subscribe(() => {
      this.table.selection.clear();
    });

    this.delete = deleteService.delete();
    this.delete.result.subscribe(() => {
      this.table.selection.clear();
    });

    const sort = true;
    this.table = new Table({
      selection: true,
      columns: [
        { name: 'positionType' },
        { name: 'rateType' },
        { name: 'from', sort },
        { name: 'through', sort },
        { name: 'rate', sort },
        { name: 'frequency' },
      ],
      actions: [this.edit, this.delete],
      defaultAction: this.edit,
      pageSize: 50,
    });
  }

  ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.filter = this.filterService.filter(m.PositionTypeRate);

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
              pull.PositionTypeRate({
                predicate: this.filter.definition.predicate,
                sorting: sort
                  ? this.sorterService.sorter(m.PositionTypeRate)?.create(sort)
                  : null,
                include: {
                  Frequency: x,
                  RateType: x,
                },
                arguments: this.filter.parameters(filterFields),
                skip: pageEvent.pageIndex * pageEvent.pageSize,
                take: pageEvent.pageSize,
              }),
              pull.PositionType({
                include: {
                  PositionTypeRate: x,
                },
              }),
            ];

            return this.allors.context.pull(pulls);
          }
        )
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        this.positionTypes = loaded.collection<PositionType>(m.PositionType);
        const objects = loaded.collection<PositionTypeRate>(m.PositionTypeRate);

        this.table.data = objects?.map((v) => {
          return {
            object: v,
            positionType: this.positionTypes
              ?.filter((p) => p.PositionTypeRate === v)
              ?.map((p) => p.Title)
              .join(', '),
            rateType: v.RateType?.Name,
            from: format(new Date(v.FromDate), 'dd-MM-yyyy'),
            through:
              v.ThroughDate != null
                ? format(new Date(v.ThroughDate), 'dd-MM-yyyy')
                : '',
            rate: v.Rate,
            frequency: v.Frequency?.Name,
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
