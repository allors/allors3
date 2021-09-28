import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';
import { format } from 'date-fns';

import { M } from '@allors/workspace/meta/default';
import { ExchangeRate } from '@allors/workspace/domain/default';
import { Action, DeleteService, EditService, Filter, MediaService, NavigationService, RefreshService, Table, TableRow, TestScope, OverviewService } from '@allors/workspace/angular/base';
import { SessionService } from '@allors/workspace/angular/core';

interface Row extends TableRow {
  object: ExchangeRate;
  validFrom: string;
  from: string;
  to: string;
  rate: string;
}

@Component({
  templateUrl: './exchangerate-list.component.html',
  providers: [SessionService],
})
export class ExchangeRateListComponent extends TestScope implements OnInit, OnDestroy {
  public title = 'Exchange Rates';

  table: Table<Row>;

  edit: Action;
  delete: Action;

  private subscription: Subscription;
  filter: Filter;

  constructor(
    @Self() public allors: SessionService,

    public refreshService: RefreshService,
    public overviewService: OverviewService,
    public editService: EditService,
    public deleteService: DeleteService,
    public navigation: NavigationService,
    public mediaService: MediaService,
    titleService: Title
  ) {
    super();

    titleService.setTitle(this.title);

    this.edit = editService.edit();
    this.edit.result.subscribe(() => {
      this.table.selection.clear();
    });

    this.delete = deleteService.delete(allors);
    this.delete.result.subscribe(() => {
      this.table.selection.clear();
    });

    this.table = new Table({
      selection: true,
      columns: [{ name: 'validFrom', sort: true }, { name: 'from', sort: true }, { name: 'to', sort: true }, { name: 'rate' }],
      actions: [this.edit, this.delete],
      defaultAction: this.edit,
      pageSize: 50,
      initialSort: 'validFrom',
      initialSortDirection: 'desc',
    });
  }

  ngOnInit(): void {
    const m = this.allors.workspace.configuration.metaPopulation as M;
    const { pullBuilder: pull } = m;
    const x = {};
    this.filter = m.ExchangeRate.filter = m.ExchangeRate.filter ?? new Filter(m.ExchangeRate.filterDefinition);

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
            pull.ExchangeRate({
              predicate: this.filter.definition.predicate,
              sort: sort ? m.ExchangeRate.sorter.create(sort) : null,
              include: {
                FromCurrency: x,
                ToCurrency: x,
              },
              arguments: this.filter.parameters(filterFields),
              skip: pageEvent.pageIndex * pageEvent.pageSize,
              take: pageEvent.pageSize,
            }),
          ];

          return this.allors.client.pullReactive(this.allors.session, pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.session.reset();

        const objects = loaded.collection<ExchangeRate>(m.ExchangeRate);
        this.table.total = loaded.value('ExchangeRates_total') as number;
        this.table.data = objects.map((v) => {
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
