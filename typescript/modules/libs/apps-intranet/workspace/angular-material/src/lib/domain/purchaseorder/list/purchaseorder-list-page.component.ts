import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';
import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { PageEvent } from '@angular/material/paginator';
import { Title } from '@angular/platform-browser';
import { Sort } from '@angular/material/sort';

import { M } from '@allors/default/workspace/meta';
import { And, Equals } from '@allors/system/workspace/domain';
import {
  InternalOrganisation,
  Person,
  PurchaseOrder,
} from '@allors/default/workspace/domain';
import {
  Action,
  Filter,
  FilterDefinition,
  FilterField,
  FilterService,
  MediaService,
  RefreshService,
  Table,
  TableRow,
  UserId,
} from '@allors/base/workspace/angular/foundation';
import { NavigationService } from '@allors/base/workspace/angular/application';
import {
  DeleteActionService,
  EditActionService,
  MethodActionService,
  OverviewActionService,
  SorterService,
} from '@allors/base/workspace/angular-material/application';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { PrintService } from '../../../actions/print/print.service';
import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { formatDistance } from 'date-fns';

interface Row extends TableRow {
  object: PurchaseOrder;
  number: string;
  supplier: string;
  state: string;
  shipmentState: string;
  customerReference: string;
  invoice: string;
  currency: string;
  totalExVat: string;
  totalIncVat: string;
  lastModifiedDate: string;
}

@Component({
  templateUrl: './purchaseorder-list-page.component.html',
  providers: [ContextService],
})
export class PurchaseOrderListPageComponent implements OnInit, OnDestroy {
  public title = 'Purchase Orders';

  m: M;

  table: Table<Row>;

  delete: Action;
  print: Action;
  ship: Action;
  invoice: Action;

  user: Person;
  internalOrganisation: InternalOrganisation;
  canCreate: boolean;

  private subscription: Subscription;
  filter: Filter;

  constructor(
    @Self() public allors: ContextService,
    public refreshService: RefreshService,
    public overviewService: OverviewActionService,
    public printService: PrintService,
    public methodService: MethodActionService,
    public deleteService: DeleteActionService,
    public navigation: NavigationService,
    public mediaService: MediaService,
    private internalOrganisationId: InternalOrganisationId,
    private userId: UserId,
    private fetcher: FetcherService,
    public filterService: FilterService,
    public sorterService: SorterService,
    titleService: Title
  ) {
    this.allors.context.name = this.constructor.name;
    titleService.setTitle(this.title);

    this.m = this.allors.context.configuration.metaPopulation as M;

    this.print = printService.print();
    this.delete = deleteService.delete();
    this.delete.result.subscribe(() => {
      this.table.selection.clear();
    });

    this.m = this.allors.context.configuration.metaPopulation as M;

    this.table = new Table({
      selection: true,
      columns: [
        { name: 'number', sort: true },
        { name: 'supplier' },
        { name: 'state' },
        { name: 'shipmentState' },
        { name: 'customerReference', sort: true },
        { name: 'invoice' },
        { name: 'currency' },
        { name: 'totalExVat', sort: true },
        { name: 'totalIncVat', sort: true },
        { name: 'lastModifiedDate', sort: true },
      ],
      actions: [overviewService.overview(), this.delete, this.print],
      defaultAction: overviewService.overview(),
      pageSize: 50,
      initialSort: 'number',
      initialSortDirection: 'desc',
    });
  }

  ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.filter = this.filterService.filter(m.PurchaseOrder);

    const internalOrganisationPredicate: Equals = {
      kind: 'Equals',
      propertyType: m.PurchaseOrder.OrderedBy,
    };

    const predicate: And = {
      kind: 'And',
      operands: [
        internalOrganisationPredicate,
        this.filter.definition.predicate,
      ],
    };

    this.subscription = combineLatest([
      this.refreshService.refresh$,
      this.filter.fields$,
      this.table.sort$,
      this.table.pager$,
      this.internalOrganisationId.observable$,
    ])
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
            internalOrganisationPredicate.value = internalOrganisationId;

            const pulls = [
              this.fetcher.internalOrganisation,
              pull.Person({
                objectId: this.userId.value,
              }),
              pull.PurchaseOrder({
                predicate,
                sorting: sort
                  ? this.sorterService.sorter(m.PurchaseOrder)?.create(sort)
                  : null,
                include: {
                  PrintDocument: {
                    Media: x,
                  },
                  TakenViaSupplier: x,
                  PurchaseOrderState: x,
                  PurchaseOrderShipmentState: x,
                  PurchaseInvoicesWherePurchaseOrder: x,
                  DerivedCurrency: x,
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

        this.internalOrganisation =
          this.fetcher.getInternalOrganisation(loaded);
        this.user = loaded.object<Person>(m.Person);

        this.canCreate =
          this.internalOrganisation.canExecuteCreatePurchaseOrder;

        const orders = loaded.collection<PurchaseOrder>(m.PurchaseOrder);
        this.table.data = orders
          ?.filter((v) => v.canReadOrderNumber)
          ?.map((v) => {
            return {
              object: v,
              number: `${v.OrderNumber}`,
              supplier: v.TakenViaSupplier && v.TakenViaSupplier.DisplayName,
              state: `${v.PurchaseOrderState && v.PurchaseOrderState.Name}`,
              shipmentState: `${
                v.PurchaseOrderShipmentState &&
                v.PurchaseOrderShipmentState.Name
              }`,
              customerReference: `${v.Description || ''}`,
              invoice: v.PurchaseInvoicesWherePurchaseOrder?.map(
                (w) => w.InvoiceNumber
              ).join(', '),
              currency: `${v.DerivedCurrency && v.DerivedCurrency.IsoCode}`,
              totalExVat: v.TotalExVat,
              totalIncVat: v.TotalIncVat,
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
