import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { ProductIdentificationType, Part } from '@allors/workspace/domain/default';
import { Action, DeleteService, Filter, MediaService, NavigationService, ObjectService, OverviewService, RefreshService, Table, TableRow, TestScope } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';

interface Row extends TableRow {
  object: Part;
  name: string;
  partNo: string;
  type: string;
  qoh: string;
  brand: string;
  model: string;
  kind: string;
}

@Component({
  templateUrl: './part-list.component.html',
  providers: [ContextService],
})
export class PartListComponent extends TestScope implements OnInit, OnDestroy {
  public title = 'Parts';

  table: Table<Row>;

  edit: Action;
  delete: Action;

  private subscription: Subscription;
  goodIdentificationTypes: ProductIdentificationType[];
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
      columns: [{ name: 'name', sort: true }, { name: 'partNo' }, { name: 'type' }, { name: 'qoh' }, { name: 'brand' }, { name: 'model' }, { name: 'kind' }],
      actions: [overviewService.overview(), this.delete],
      defaultAction: overviewService.overview(),
      pageSize: 50,
    });
  }

  ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.filter = m.Part._.filter ??= new Filter(m.Part._.filterDefinition);

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
            pull.Part({
              predicate: this.filter.definition.predicate,
              sorting: sort ? m.Part._.sorter?.create(sort) : null,
              include: {
                Brand: x,
                Model: x,
                ProductType: x,
                PrimaryPhoto: x,
                InventoryItemKind: x,
                ProductIdentifications: {
                  ProductIdentificationType: x,
                },
              },
              arguments: this.filter.parameters(filterFields),
              skip: pageEvent.pageIndex * pageEvent.pageSize,
              take: pageEvent.pageSize,
            }),
            pull.ProductIdentificationType({}),
            pull.BasePrice({}),
          ];

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        const parts = loaded.collection<Part>(m.Part);
        this.goodIdentificationTypes = loaded.collection<ProductIdentificationType>(m.ProductIdentificationType);
        const partNumberType = this.goodIdentificationTypes.find((v) => v.UniqueId === '5735191a-cdc4-4563-96ef-dddc7b969ca6');

        const partNumberByPart = parts.reduce((map, obj) => {
          map[obj.id] = obj.ProductIdentifications.filter((v) => v.ProductIdentificationType === partNumberType).map((w) => w.Identification);
          return map;
        }, {});

        this.table.total = loaded.value('NonUnifiedParts_total') as number;

        this.table.data = parts.map((v) => {
          return {
            object: v,
            name: v.Name,
            partNo: partNumberByPart[v.id][0],
            qoh: v.QuantityOnHand,
            type: v.ProductType ? v.ProductType.Name : '',
            brand: v.Brand ? v.Brand.Name : '',
            model: v.Model ? v.Model.Name : '',
            kind: v.InventoryItemKind.Name,
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
