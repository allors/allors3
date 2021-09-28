import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';
import { format } from 'date-fns';

import { M } from '@allors/workspace/meta/default';
import { PositionTypeRate, PositionType } from '@allors/workspace/domain/default';
import { Action, DeleteService, EditService, Filter, MediaService, NavigationService, OverviewService, RefreshService, Table, TestScope } from '@allors/workspace/angular/base';
import { SessionService } from '@allors/workspace/angular/core';


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
  templateUrl: './positiontyperate-list.component.html',
  providers: [SessionService],
})
export class PositionTypeRatesOverviewComponent extends TestScope implements OnInit, OnDestroy {
  public title = 'Position Type Rates';

  table: Table<Row>;

  edit: Action;
  delete: Action;

  private subscription: Subscription;
  positionTypes: PositionType[];
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
    const m = this.allors.workspace.configuration.metaPopulation as M; const { pullBuilder: pull } = m; const x = {};
    this.filter = m.PositionTypeRate.filter = m.PositionTypeRate.filter ?? new Filter(m.PositionTypeRate.filterDefinition);

    this.subscription = combineLatest([this.refreshService.refresh$, this.filter.fields$, this.table.sort$, this.table.pager$])
      .pipe(
        scan(
          ([previousRefresh, previousFilterFields], [refresh, filterFields, sort, pageEvent]) => {
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
            pull.PositionTypeRate({
              predicate: this.filter.definition.predicate,
              sort: sort ? m.PositionTypeRate.sorter.create(sort) : null,
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

          return this.allors.client.pullReactive(this.allors.session, pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.session.reset();

        this.positionTypes = loaded.collection<PositionType>(m.PositionType);
        const objects = loaded.collection<PositionTypeRate>(m.PositionTypeRate);

        this.table.total = loaded.value('PositionTypeRates_total') as number;
        this.table.data = objects.map((v) => {
          return {
            object: v,
            positionType: this.positionTypes
              .filter((p) => p.PositionTypeRate === v)
              .map((p) => p.Title)
              .join(', '),
            rateType: v.RateType.Name,
            from: format(new Date(v.FromDate), 'dd-MM-yyyy'),
            through: v.ThroughDate !== null ? format(new Date(v.ThroughDate), 'dd-MM-yyyy') : '',
            rate: v.Rate,
            frequency: v.Frequency.Name,
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
