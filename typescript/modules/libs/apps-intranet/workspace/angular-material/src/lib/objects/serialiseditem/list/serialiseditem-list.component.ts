import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';

import { M } from '@allors/default/workspace/meta';
import { SerialisedItem } from '@allors/default/workspace/domain';
import {
  Action,
  DeleteService,
  Filter,
  MediaService,
  NavigationService,
  ObjectService,
  RefreshService,
  Table,
  TableRow,
  OverviewService,
  angularFilterFromDefinition,
  angularSorter,
  FilterField,
} from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';
import { Sort } from '@angular/material/sort';
import { PageEvent } from '@angular/material/paginator';

interface Row extends TableRow {
  object: SerialisedItem;
  id: string;
  name: string;
  categories: string;
  availability: string;
  ownership: string;
  suppliedBy: string;
  ownedBy: string;
  rentedBy: string;
}

@Component({
  templateUrl: './serialiseditem-list.component.html',
  providers: [ContextService],
})
export class SerialisedItemListComponent implements OnInit, OnDestroy {
  public title = 'Serialised Assets';

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
      columns: [
        { name: 'id', sort: true },
        { name: 'name', sort: true },
        { name: 'categories', sort: true },
        { name: 'availability', sort: true },
        { name: 'ownership', sort: true },
        { name: 'suppliedBy', sort: true },
        { name: 'ownedBy', sort: true },
        { name: 'rentedBy', sort: true },
      ],
      actions: [overviewService.overview(), this.delete],
      defaultAction: overviewService.overview(),
      pageSize: 50,
      initialSort: 'id',
    });
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.filter = angularFilterFromDefinition(m.SerialisedItem);

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
              pull.SerialisedItem({
                predicate: this.filter.definition.predicate,
                sorting: sort
                  ? angularSorter(m.SerialisedItem)?.create(sort)
                  : null,
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

        const objects = loaded.collection<SerialisedItem>(m.SerialisedItem);

        this.table.total = (loaded.value('SerialisedItems_total') ??
          0) as number;
        this.table.data = objects?.map((v) => {
          return {
            object: v,
            id: v.ItemNumber,
            name: v.Name,
            categories: v.ProductCategoriesDisplayName,
            availability: v.SerialisedItemAvailabilityName,
            ownership: v.OwnershipName,
            suppliedBy: v.SuppliedByPartyName,
            ownedBy: v.OwnedByPartyName,
            rentedBy: v.RentedByPartyName,
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
