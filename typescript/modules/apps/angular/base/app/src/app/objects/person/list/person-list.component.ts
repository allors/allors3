import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { Person } from '@allors/workspace/domain/default';
import { Action, angularFilterFromDefinition, angularSorter, DeleteService, Filter, FilterField, MediaService, NavigationService, ObjectService, OverviewService, RefreshService, Table, TableRow } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';
import { Sort } from '@angular/material/sort';
import { PageEvent } from '@angular/material/paginator';

interface Row extends TableRow {
  object: Person;
  firstName: string;
  lastName: string;
  email: string;
}

@Component({
  templateUrl: './person-list.component.html',
  providers: [{ provide: 'dependencies', useValue: 'person-list' }, ContextService],
})
export class PersonListComponent implements OnInit, OnDestroy {
  public title = 'People';

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
    this.allors.context.name = this.constructor.name;
    titleService.setTitle(this.title);

    this.m = this.allors.context.configuration.metaPopulation as M;

    this.delete = deleteService.delete(allors.context);
    this.delete.result.subscribe(() => {
      this.table.selection.clear();
    });

    this.table = new Table({
      selection: true,
      columns: [{ name: 'firstName', sort: true }, { name: 'firstName' }, { name: 'email' }],
      actions: [overviewService.overview(), this.delete],
      defaultAction: overviewService.overview(),
      pageSize: 50,
      initialSort: 'firstName',
    });
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;

    this.filter = angularFilterFromDefinition(m.Person);

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
            pull.Person({
              predicate: this.filter.definition.predicate,
              sorting: sort ? angularSorter(m.Person)?.create(sort) : null,
              include: {
                Pictures: {},
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

        const people = loaded.collection<Person>(m.Person);
        this.table.total = (loaded.value('People_total') ?? 0) as number;
        this.table.data = people?.map((v) => {
          return {
            object: v,
            firstName: v.FirstName,
            lastName: v.LastName,
            email: v.UserEmail,
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
