import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';
import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Sort } from '@angular/material/sort';
import { PageEvent } from '@angular/material/paginator';

import { Person } from '@allors/default/workspace/domain';
import {
  Action,
  ContextService,
  Filter,
  FilterField,
  MediaService,
  RefreshService,
  CreateService,
  FilterService,
  TableRow,
  Table,
  MetaService,
} from '@allors/base/workspace/angular/foundation';
import {
  AllorsListPageComponent,
  NavigationService,
} from '@allors/base/workspace/angular/application';
import {
  DeleteActionService,
  OverviewActionService,
  SorterService,
} from '@allors/base/workspace/angular-material/application';

interface Row extends TableRow {
  object: Person;
  firstName: string;
  lastName: string;
  email: string;
}

@Component({
  templateUrl: './person-list-page.component.html',
  providers: [ContextService],
})
export class PersonListPageComponent
  extends AllorsListPageComponent
  implements OnInit, OnDestroy
{
  table: Table<Row>;
  filter: Filter;

  delete: Action;

  private subscription: Subscription;

  constructor(
    @Self() allors: ContextService,
    titleService: Title,
    public createService: CreateService,
    public refreshService: RefreshService,
    public overviewService: OverviewActionService,
    public deleteService: DeleteActionService,
    public navigation: NavigationService,
    public mediaService: MediaService,
    public filterService: FilterService,
    public sorterService: SorterService,
    metaService: MetaService
  ) {
    super(allors, metaService, titleService);
    this.objectType = this.m.Person;

    this.delete = deleteService.delete();
    this.delete.result.subscribe(() => {
      this.table.selection.clear();
    });

    this.table = new Table({
      selection: true,
      columns: [
        { name: 'firstName', sort: true },
        { name: 'lastName' },
        { name: 'email' },
      ],
      actions: [overviewService.overview(), this.delete],
      defaultAction: overviewService.overview(),
      pageSize: 50,
      initialSort: 'firstName',
    });
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;

    this.filter = this.filterService.filter(m.Person);

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
              pull.Person({
                predicate: this.filter.definition.predicate,
                sorting: sort
                  ? this.sorterService.sorter(m.Person)?.create(sort)
                  : null,
                include: {
                  Pictures: {},
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

        const people = loaded.collection<Person>(m.Person);
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
