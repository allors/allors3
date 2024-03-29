import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';
import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { PageEvent } from '@angular/material/paginator';
import { Title } from '@angular/platform-browser';
import { Sort } from '@angular/material/sort';

import { M } from '@allors/default/workspace/meta';
import {
  Part,
  ProductIdentificationType,
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
import { formatDistance } from 'date-fns';
interface Row extends TableRow {
  object: Part;
  name: string;
  partNo: string;
  type: string;
  qoh: string;
  brand: string;
  model: string;
  kind: string;
  lastModifiedDate: string;
}

@Component({
  templateUrl: './part-list-page.component.html',
  providers: [ContextService],
})
export class PartListPageComponent implements OnInit, OnDestroy {
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
        { name: 'partNo', sort: true },
        { name: 'type', sort: true },
        { name: 'qoh' },
        { name: 'brand', sort: true },
        { name: 'model', sort: true },
        { name: 'kind', sort: true },
        { name: 'lastModifiedDate', sort: true },
      ],
      actions: [overviewService.overview(), this.delete],
      defaultAction: overviewService.overview(),
      pageSize: 50,
    });
  }

  ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.filter = this.filterService.filter(m.Part);

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
              pull.Part({
                predicate: this.filter.definition.predicate,
                sorting: sort
                  ? this.sorterService.sorter(m.Part)?.create(sort)
                  : null,
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
          }
        )
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        const parts = loaded.collection<Part>(m.Part);
        this.goodIdentificationTypes =
          loaded.collection<ProductIdentificationType>(
            m.ProductIdentificationType
          );
        const partNumberType = this.goodIdentificationTypes?.find(
          (v) => v.UniqueId === '5735191a-cdc4-4563-96ef-dddc7b969ca6'
        );

        const partNumberByPart = parts?.reduce((map, object) => {
          map[object.id] = object.ProductIdentifications?.filter(
            (v) => v.ProductIdentificationType === partNumberType
          )?.map((w) => w.Identification);
          return map;
        }, {});

        this.table.data = parts?.map((v) => {
          return {
            object: v,
            name: v.Name,
            partNo: partNumberByPart[v.id][0],
            qoh: v.QuantityOnHand,
            type: v.ProductType ? v.ProductType.Name : '',
            brand: v.Brand ? v.Brand.Name : '',
            model: v.Model ? v.Model.Name : '',
            kind: v.InventoryItemKind.Name,
            lastModifiedDate: formatDistance(
              new Date(v.LastModifiedDate),
              new Date()
            ),
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
