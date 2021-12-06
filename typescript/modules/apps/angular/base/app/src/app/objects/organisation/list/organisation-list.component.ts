import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { Organisation } from '@allors/workspace/domain/default';
import {
  Action,
  angularFilterFromDefinition,
  angularSorter,
  DeleteService,
  Filter,
  FilterField,
  MediaService,
  MethodService,
  NavigationService,
  ObjectService,
  OverviewService,
  RefreshService,
  Table,
  TableRow,
} from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';

import { Sort } from '@angular/material/sort';
import { PageEvent } from '@angular/material/paginator';

interface Row extends TableRow {
  object: Organisation;
  name: string;
  country: string;
  owner: string;
}

@Component({
  templateUrl: './organisation-list.component.html',
  providers: [ContextService],
})
export class OrganisationListComponent implements OnInit, OnDestroy {
  public title = 'Organisations';

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
    public methodService: MethodService,
    public navigation: NavigationService,
    public mediaService: MediaService,
    titleService: Title
  ) {
    this.allors.context.name = this.constructor.name;
    titleService.setTitle(this.title);

    this.m = this.allors.context.configuration.metaPopulation as M;

    this.delete = deleteService.delete(allors.context);
    this.delete.result.subscribe(() => {
      this.table.selection.clear();
    });

    this.table = new Table({
      selection: true,
      columns: [{ name: 'name', sort: true }, 'country', 'owner'],
      actions: [overviewService.overview(), this.delete],
      defaultAction: overviewService.overview(),
      pageSize: 50,
    });
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;

    this.filter = angularFilterFromDefinition(m.Organisation);

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
        switchMap(([, filterFields, sort, pageEvent]: [Date, FilterField[], Sort, PageEvent]) => {
          const pulls = [
            pull.Organisation({
              predicate: this.filter.definition.predicate,
              sorting: sort ? angularSorter(m.Organisation)?.create(sort) : null,
              include: {
                Owner: {},
                Country: {},
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

        const organisations = loaded.collection<Organisation>(m.Organisation);
        this.table.total = (loaded.value('Organisations_total') ?? 0) as number;
        this.table.data = organisations?.map((v) => {
          return {
            object: v,
            name: v.Name,
            country: v.Country?.Name ?? null,
            owner: v.Owner?.UserName ?? null,
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
