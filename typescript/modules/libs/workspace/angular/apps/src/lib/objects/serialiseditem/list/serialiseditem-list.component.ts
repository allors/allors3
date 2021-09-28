import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { SerialisedItem } from '@allors/workspace/domain/default';
import { Action, DeleteService, Filter, MediaService, NavigationService, ObjectService, RefreshService, Table, TableRow, TestScope, OverviewService } from '@allors/workspace/angular/base';
import { SessionService } from '@allors/workspace/angular/core';

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
  providers: [SessionService],
})
export class SerialisedItemListComponent extends TestScope implements OnInit, OnDestroy {
  public title = 'Serialised Assets';

  table: Table<Row>;

  delete: Action;

  private subscription: Subscription;
  filter: Filter;

  constructor(
    @Self() public allors: SessionService,

    public factoryService: ObjectService,
    public refreshService: RefreshService,
    public overviewService: OverviewService,
    public deleteService: DeleteService,
    public navigation: NavigationService,
    public mediaService: MediaService,
    titleService: Title
  ) {
    super();

    titleService.setTitle(this.title);

    this.delete = deleteService.delete(allors);
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
    const m = this.allors.workspace.configuration.metaPopulation as M;
    const { pullBuilder: pull } = m;
    const x = {};
    this.filter = m.SerialisedItem.filter = m.SerialisedItem.filter ?? new Filter(m.SerialisedItem.filterDefinition);

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
            pull.SerialisedItem({
              predicate: this.filter.definition.predicate,
              sorting: sort ? m.SerialisedItem.sorter.create(sort) : null,
              include: {
                SerialisedItemState: x,
                SerialisedItemAvailability: x,
                Ownership: x,
                SuppliedBy: x,
                OwnedBy: x,
                RentedBy: x,
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

        const objects = loaded.collection<SerialisedItem>(m.SerialisedItem);

        this.table.total = loaded.value('SerialisedItems_total') as number;
        this.table.data = objects.map((v) => {
          return {
            object: v,
            id: v.ItemNumber,
            name: v.Name,
            categories: v.DisplayProductCategories,
            availability: v.SerialisedItemAvailabilityName,
            ownership: v.OwnershipByOwnershipName,
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
