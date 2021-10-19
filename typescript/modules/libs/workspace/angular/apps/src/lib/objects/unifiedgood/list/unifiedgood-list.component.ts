import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';
import { formatDistance } from 'date-fns';

import { M } from '@allors/workspace/meta/default';
import { ProductCategory, UnifiedGood } from '@allors/workspace/domain/default';
import { Action, DeleteService, Filter, MediaService, NavigationService, ObjectService, RefreshService, Table, TableRow, TestScope, OverviewService } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';

interface Row extends TableRow {
  object: UnifiedGood;
  name: string;
  id: string;
  categories: string;
  qoh: string;
  photos: string;
  lastModifiedDate: string;
}

@Component({
  templateUrl: './unifiedgood-list.component.html',
  providers: [ContextService],
})
export class UnifiedGoodListComponent extends TestScope implements OnInit, OnDestroy {
  public title = 'Unified Goods';

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
    super();

    titleService.setTitle(this.title);

    this.m = this.allors.context.configuration.metaPopulation as M;

    this.delete = deleteService.delete(allors.context);
    this.delete.result.subscribe(() => {
      this.table.selection.clear();
    });

    this.table = new Table({
      selection: true,
      columns: [{ name: 'name', sort: true }, { name: 'id', sort: true }, { name: 'photos' }, { name: 'categories' }, { name: 'qoh' }, { name: 'lastModifiedDate', sort: true }],
      actions: [overviewService.overview(), this.delete],
      defaultAction: overviewService.overview(),
      pageSize: 50,
    });
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;

    this.filter = m.UnifiedGood._.filter ??= new Filter(m.UnifiedGood._.filterDefinition);

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
            pull.UnifiedGood({
              predicate: this.filter.definition.predicate,
              sorting: sort ? m.UnifiedGood._.sorter?.create(sort) : null,
              include: {
                Photos: {},
                PrimaryPhoto: {},
                ProductIdentifications: {
                  ProductIdentificationType: {},
                },
              },
              arguments: this.filter.parameters(filterFields),
              skip: pageEvent.pageIndex * pageEvent.pageSize,
              take: pageEvent.pageSize,
            }),
            pull.UnifiedGood({
              predicate: this.filter.definition.predicate,
              sorting: sort ? m.UnifiedGood._.sorter?.create(sort) : null,
              select: {
                ProductCategoriesWhereProduct: {
                  include: {
                    Products: {},
                    PrimaryAncestors: {},
                  },
                },
              },
            }),
          ];

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        const goods = loaded.collection<UnifiedGood>(m.UnifiedGood);
        const productCategories = loaded.collection<ProductCategory>(m.UnifiedGood.ProductCategoriesWhereProduct);

        this.table.total = loaded.value('UnifiedGoods_total') as number;
        this.table.data = goods.map((v) => {
          return {
            object: v,
            name: v.Name,
            id: v.ProductNumber,
            categories: productCategories
              .filter((w) => w.Products.includes(v))
              .map((w) => w.DisplayName)
              .join(', '),
            qoh: v.QuantityOnHand,
            photos: v.Photos.length.toString(),
            lastModifiedDate: formatDistance(new Date(v.LastModifiedDate), new Date()),
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