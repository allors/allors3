import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';
import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Sort } from '@angular/material/sort';
import { PageEvent } from '@angular/material/paginator';
import {
  angularSorter,
  DeleteService,
  EditService,
  OverviewService,
  Table,
  TableRow,
} from '@allors/workspace/angular-material/base';
import { M } from '@allors/workspace/meta/default';
import { Country } from '@allors/workspace/domain/default';
import { ContextService } from '@allors/workspace/angular/core';
import {
  Action,
  AllorsListComponent,
  angularFilterFromDefinition,
  Filter,
  FilterField,
  MediaService,
  NavigationService,
  RefreshService,
} from '@allors/workspace/angular/base';

interface Row extends TableRow {
  object: Country;
  isoCode: string;
  name: string;
}

@Component({
  templateUrl: './country-list.component.html',
  providers: [ContextService],
})
export class CountryListComponent
  extends AllorsListComponent
  implements OnInit, OnDestroy
{
  public title = 'Countries';

  table: Table<Row>;

  edit: Action;
  delete: Action;

  private subscription: Subscription;
  filter: Filter;
  m: M;

  constructor(
    @Self() allors: ContextService,
    titleService: Title,
    public refreshService: RefreshService,
    public overviewService: OverviewService,
    public editService: EditService,
    public deleteService: DeleteService,
    public navigation: NavigationService,
    public mediaService: MediaService
  ) {
    super(allors, titleService);
    this.objectType = this.m.Country;

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
        { name: 'isoCode', sort: true },
        { name: 'name', sort: true },
      ],
      actions: [this.edit, this.delete],
      defaultAction: this.edit,
      pageSize: 50,
      initialSort: 'isoCode',
    });
  }

  ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.filter = angularFilterFromDefinition(m.Country);

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
              pull.Country({
                predicate: this.filter.definition.predicate,
                sorting: sort ? angularSorter(m.Country)?.create(sort) : null,
                include: {
                  LocalisedNames: x,
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

        const objects = loaded.collection<Country>(m.Country);
        this.table.total = (loaded.value('Countrys_total') ?? 0) as number;
        this.table.data = objects?.map((v) => {
          return {
            object: v,
            isoCode: v.IsoCode,
            name: v.Name,
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
