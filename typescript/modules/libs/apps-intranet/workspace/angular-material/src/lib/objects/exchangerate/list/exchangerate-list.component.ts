import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';
import { format } from 'date-fns';

import { M } from '@allors/default/workspace/meta';
import { ExchangeRate } from '@allors/default/workspace/domain';
import {
  Action,
  DeleteService,
  EditService,
  Filter,
  MediaService,
  NavigationService,
  RefreshService,
  Table,
  TableRow,
  OverviewService,
  angularFilterFromDefinition,
  angularSorter,
  FilterField,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { Sort } from '@angular/material/sort';
import { PageEvent } from '@angular/material/paginator';

interface Row extends TableRow {
  object: ExchangeRate;
  validFrom: string;
  from: string;
  to: string;
  rate: string;
}

@Component({
  templateUrl: './exchangerate-list.component.html',
  providers: [ContextService],
})
export class ExchangeRateListComponent implements OnInit, OnDestroy {
  public title = 'Exchange Rates';

  table: Table<Row>;

  edit: Action;
  delete: Action;

  private subscription: Subscription;
  filter: Filter;
  m: M;

  constructor(
    @Self() public allors: ContextService,
    public refreshService: RefreshService,
    public overviewService: OverviewService,
    public editService: EditService,
    public deleteService: DeleteService,
    public navigation: NavigationService,
    public mediaService: MediaService,
    titleService: Title
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;

    titleService.setTitle(this.title);

    this.edit = editService.edit();
    this.edit.result.subscribe(() => {
      this.table.selection.clear();
    });

    this.delete = deleteService.delete(allors.context);
    this.delete.result.subscribe(() => {
      this.table.selection.clear();
    });

    this.table = new Table({
      selection: true,
      columns: [
        { name: 'validFrom', sort: true },
        { name: 'from', sort: true },
        { name: 'to', sort: true },
        { name: 'rate' },
      ],
      actions: [this.edit, this.delete],
      defaultAction: this.edit,
      pageSize: 50,
      initialSort: 'validFrom',
      initialSortDirection: 'desc',
    });
  }

  ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.filter = angularFilterFromDefinition(m.ExchangeRate);

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
              pull.ExchangeRate({
                predicate: this.filter.definition.predicate,
                sorting: sort
                  ? angularSorter(m.ExchangeRate)?.create(sort)
                  : null,
                include: {
                  FromCurrency: x,
                  ToCurrency: x,
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

        const objects = loaded.collection<ExchangeRate>(m.ExchangeRate);
        this.table.total = (loaded.value('ExchangeRates_total') ?? 0) as number;
        this.table.data = objects?.map((v) => {
          return {
            object: v,
            validFrom: format(new Date(v.ValidFrom), 'dd-MM-yyyy'),
            from: v.FromCurrency.Name,
            to: v.ToCurrency.Name,
            rate: v.Rate,
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
