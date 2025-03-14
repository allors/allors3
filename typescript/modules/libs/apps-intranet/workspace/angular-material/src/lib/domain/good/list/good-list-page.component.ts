import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';
import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { PageEvent } from '@angular/material/paginator';
import { Title } from '@angular/platform-browser';
import { Sort } from '@angular/material/sort';

import { M } from '@allors/default/workspace/meta';
import {
  Good,
  NonUnifiedGood,
  ProductCategory,
  UnifiedGood,
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
  OverviewActionService,
  SorterService,
} from '@allors/base/workspace/angular-material/application';
import { ContextService } from '@allors/base/workspace/angular/foundation';

interface Row extends TableRow {
  object: Good;
  name: string;
  id: string;
  categories: string;
  qoh: string;
}

@Component({
  templateUrl: './good-list-page.component.html',
  providers: [ContextService],
})
export class GoodListPageComponent implements OnInit, OnDestroy {
  public title = 'Goods';

  table: Table<Row>;

  delete: Action;

  private subscription: Subscription;
  filter: Filter;
  m: M;

  constructor(
    @Self() public allors: ContextService,
    public refreshService: RefreshService,
    public overviewService: OverviewActionService,
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

    this.delete = deleteService.delete();
    this.delete.result.subscribe(() => {
      this.table.selection.clear();
    });

    this.table = new Table({
      selection: true,
      columns: [
        { name: 'name', sort: true },
        { name: 'id' },
        { name: 'categories' },
        { name: 'qoh' },
      ],
      actions: [overviewService.overview(), this.delete],
      defaultAction: overviewService.overview(),
      pageSize: 50,
    });
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.filter = this.filterService.filter(m.Good);

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
            return [
              refresh,
              filterFields,
              sort,
              previousRefresh !== refresh ||
              filterFields !== previousFilterFields
                ? Object.assign({ pageIndex: 0 }, pageEvent)
                : pageEvent,
            ];
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
              pull.Good({
                predicate: this.filter.definition.predicate,
                sorting: sort
                  ? this.sorterService.sorter(m.Good)?.create(sort)
                  : null,
                include: {
                  NonUnifiedGood_Part: x,
                  ProductIdentifications: {
                    ProductIdentificationType: x,
                  },
                },
                arguments: this.filter.parameters(filterFields),
                skip: pageEvent.pageIndex * pageEvent.pageSize,
                take: pageEvent.pageSize,
              }),
              pull.Good({
                predicate: this.filter.definition.predicate,
                sorting: sort
                  ? this.sorterService.sorter(m.Good)?.create(sort)
                  : null,
                select: {
                  ProductCategoriesWhereProduct: {
                    include: {
                      Products: x,
                      PrimaryAncestors: x,
                    },
                  },
                },
              }),
            ];

            return this.allors.context.pull(pulls);
          }
        )
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        const goods = loaded.collection<Good>(m.Good);
        const productCategories = loaded.collection<ProductCategory>(
          m.Good.ProductCategoriesWhereProduct
        );

        this.table.data = goods?.map((v) => {
          return {
            object: v,
            name: v.Name,
            id: v.ProductIdentifications?.find(
              (p) =>
                p.ProductIdentificationType.UniqueId ===
                'b640630d-a556-4526-a2e5-60a84ab0db3f'
            )?.Identification,
            categories: productCategories
              ?.filter((w) => w.Products.includes(v))
              ?.map((w) => w.DisplayName)
              .join(', '),
            // qoh: v.Part && v.Part.QuantityOnHand
            qoh:
              ((v as NonUnifiedGood).Part &&
                (v as NonUnifiedGood).Part.QuantityOnHand) ||
              (v as UnifiedGood).QuantityOnHand,
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
