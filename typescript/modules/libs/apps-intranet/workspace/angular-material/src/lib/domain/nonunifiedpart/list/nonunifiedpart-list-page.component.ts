import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';
import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { PageEvent } from '@angular/material/paginator';
import { Title } from '@angular/platform-browser';
import { Sort } from '@angular/material/sort';

import { M } from '@allors/default/workspace/meta';
import {
  Facility,
  InternalOrganisation,
  NonSerialisedInventoryItem,
  NonUnifiedPart,
  NonUnifiedPartBarcodePrint,
  Part,
  Person,
  ProductIdentificationType,
} from '@allors/default/workspace/domain';
import {
  Action,
  ErrorService,
  Filter,
  FilterField,
  FilterService,
  MediaService,
  RefreshService,
  SingletonId,
  Table,
  TableRow,
  UserId,
} from '@allors/base/workspace/angular/foundation';
import { NavigationService } from '@allors/base/workspace/angular/application';
import {
  DeleteService,
  OverviewService,
  SorterService,
} from '@allors/base/workspace/angular-material/application';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { PrintService } from '../../../actions/print/print.service';
import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { formatDistance } from 'date-fns';

interface Row extends TableRow {
  object: Part;
  name: string;
  partNo: string;
  categories: string;
  qoh: string;
  localQoh: string;
  brand: string;
  model: string;
  kind: string;
  lastModifiedDate: string;
}

@Component({
  templateUrl: './nonunifiedpart-list-page.component.html',
  providers: [ContextService],
})
export class NonUnifiedPartListPageComponent implements OnInit, OnDestroy {
  public title = 'Parts';

  table: Table<Row>;

  edit: Action;
  delete: Action;
  print: Action;

  private subscription: Subscription;
  goodIdentificationTypes: ProductIdentificationType[];
  parts: NonUnifiedPart[];
  nonUnifiedPartBarcodePrint: NonUnifiedPartBarcodePrint;
  facilities: Facility[];
  user: Person;
  internalOrganisation: InternalOrganisation;
  filter: Filter;
  m: M;

  constructor(
    @Self() public allors: ContextService,
    public refreshService: RefreshService,
    public overviewService: OverviewService,
    public deleteService: DeleteService,
    public navigation: NavigationService,
    public mediaService: MediaService,
    public printService: PrintService,
    private errorService: ErrorService,
    private singletonId: SingletonId,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId,
    private userId: UserId,
    public filterService: FilterService,
    public sorterService: SorterService,
    titleService: Title
  ) {
    titleService.setTitle(this.title);

    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;

    this.print = printService.print();

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
        { name: 'categories', sort: true },
        { name: 'qoh' },
        { name: 'localQoh' },
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

    this.filter = this.filterService.filter(m.NonUnifiedPart);

    this.subscription = combineLatest(
      this.refreshService.refresh$,
      this.filter.fields$,
      this.table.sort$,
      this.table.pager$,
      this.internalOrganisationId.observable$
    )
      .pipe(
        scan(
          (
            [previousRefresh, previousFilterFields],
            [refresh, filterFields, sort, pageEvent, internalOrganisationId]
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

            return [
              refresh,
              filterFields,
              sort,
              pageEvent,
              internalOrganisationId,
            ];
          }
        ),
        switchMap(
          ([, filterFields, sort, pageEvent, internalOrganisationId]: [
            Date,
            FilterField[],
            Sort,
            PageEvent,
            number
          ]) => {
            const pulls = [
              this.fetcher.internalOrganisation,
              pull.InternalOrganisation({
                objectId: internalOrganisationId,
                include: { FacilitiesWhereOwner: x },
              }),
              pull.NonUnifiedPart({
                predicate: this.filter.definition.predicate,
                sorting: sort
                  ? this.sorterService.sorter(m.NonUnifiedPart)?.create(sort)
                  : null,
                include: {
                  PrimaryPhoto: x,
                  InventoryItemsWherePart: {
                    Facility: x,
                  },
                  ProductIdentifications: {
                    ProductIdentificationType: x,
                  },
                },
                arguments: this.filter.parameters(filterFields),
                skip: pageEvent.pageIndex * pageEvent.pageSize,
                take: pageEvent.pageSize,
              }),
              pull.Singleton({
                objectId: this.singletonId.value,
                select: {
                  NonUnifiedPartBarcodePrint: {
                    include: {
                      PrintDocument: {
                        Media: x,
                      },
                    },
                  },
                },
              }),
              pull.ProductIdentificationType({}),
              pull.BasePrice({}),
              pull.Person({
                objectId: this.userId.value,
                include: { Locale: x },
              }),
            ];

            return this.allors.context.pull(pulls);
          }
        )
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        this.user = loaded.object<Person>(m.Person);
        this.internalOrganisation =
          this.fetcher.getInternalOrganisation(loaded);
        this.facilities = loaded.collection<Facility>(m.Facility);
        this.nonUnifiedPartBarcodePrint =
          loaded.object<NonUnifiedPartBarcodePrint>(
            this.m.Singleton.NonUnifiedPartBarcodePrint
          );

        this.parts = loaded.collection<NonUnifiedPart>(m.NonUnifiedPart);

        const inStockSearch = this.filter.fields?.find(
          (v) => v.definition.name === 'In Stock'
        );
        let facilitySearchId = inStockSearch?.value;
        if (inStockSearch != null) {
          this.parts = this.parts?.filter((v) => {
            return (
              v.InventoryItemsWherePart?.filter(
                (i: NonSerialisedInventoryItem) =>
                  i.Facility.id === inStockSearch.value &&
                  Number(i.QuantityOnHand) > 0
              ).length > 0
            );
          });
        }

        const outOStockSearch = this.filter.fields?.find(
          (v) => v.definition.name === 'Out Of Stock'
        );
        if (facilitySearchId == null) {
          facilitySearchId = outOStockSearch?.value;
        }

        if (outOStockSearch != null) {
          this.parts = this.parts?.filter((v) => {
            return (
              v.InventoryItemsWherePart?.filter(
                (i: NonSerialisedInventoryItem) =>
                  i.Facility.id === outOStockSearch.value &&
                  Number(i.QuantityOnHand) === 0
              ).length > 0
            );
          });
        }

        this.goodIdentificationTypes =
          loaded.collection<ProductIdentificationType>(
            m.ProductIdentificationType
          );

        this.table.total = (loaded.value('NonUnifiedParts_total') ??
          0) as number;

        this.table.data = this.parts?.map((v) => {
          return {
            object: v,
            name: v.Name,
            partNo: v.ProductNumber,
            qoh: v.QuantityOnHand,
            localQoh:
              facilitySearchId &&
              (v.InventoryItemsWherePart as NonSerialisedInventoryItem[])?.find(
                (i) => i.Facility.id === facilitySearchId
              ).QuantityOnHand,
            categories: v.PartCategoriesDisplayName,
            brand: v.BrandName,
            model: v.ModelName,
            kind: v.InventoryItemKindName,
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

  public printBarcode(parts: any): void {
    this.nonUnifiedPartBarcodePrint.Parts = parts;
    this.nonUnifiedPartBarcodePrint.Facility =
      this.internalOrganisation.FacilitiesWhereOwner[0];
    this.nonUnifiedPartBarcodePrint.Locale = this.user.Locale;

    this.allors.context.push().subscribe(() => {
      const m = this.m;
      const { pullBuilder: pull } = m;
      const x = {};

      const pulls = [
        pull.Singleton({
          objectId: this.singletonId.value,
          select: {
            NonUnifiedPartBarcodePrint: {
              include: {
                PrintDocument: {
                  Media: x,
                },
              },
            },
          },
        }),
      ];

      this.allors.context.pull(pulls).subscribe((loaded) => {
        this.allors.context.reset();

        this.nonUnifiedPartBarcodePrint =
          loaded.object<NonUnifiedPartBarcodePrint>(
            this.m.Singleton.NonUnifiedPartBarcodePrint
          );

        this.print.execute(this.nonUnifiedPartBarcodePrint);
        this.refreshService.refresh();
      });
    }, this.errorService.errorHandler);
  }
}
