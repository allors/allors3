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
  SalesOrder,
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
  UserId,
} from '@allors/base/workspace/angular/foundation';
import { NavigationService } from '@allors/base/workspace/angular/application';
import {
  DeleteService,
  MethodService,
  OverviewService,
  SorterService,
} from '@allors/base/workspace/angular-material/application';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { PrintService } from '../../../actions/print/print.service';
import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { formatDistance } from 'date-fns';

interface Row extends TableRow {
  object: SalesOrder;
  number: string;
  shipToCustomer: string;
  state: string;
  customerReference: string;
  lastModifiedDate: string;
}

@Component({
  templateUrl: './salesorder-list-page.component.html',
  providers: [ContextService],
})
export class SalesOrderListPageComponent implements OnInit, OnDestroy {
  public title = 'Sales Orders';

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
    public overviewService: OverviewService,
    public printService: PrintService,
    public methodService: MethodService,
    public deleteService: DeleteService,
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

    this.delete = deleteService.delete();
    this.delete.result.subscribe(() => {
      this.table.selection.clear();
    });

    this.print = printService.print();
    this.ship = methodService.create(allors.context, this.m.SalesOrder.Ship, {
      name: 'Ship',
    });
    this.invoice = methodService.create(
      allors.context,
      this.m.SalesOrder.Invoice,
      { name: 'Invoice' }
    );

    this.table = new Table({
      selection: true,
      columns: [
        { name: 'number', sort: true },
        { name: 'shipToCustomer' },
        { name: 'state' },
        { name: 'invoice' },
        { name: 'customerReference', sort: true },
        { name: 'lastModifiedDate', sort: true },
      ],
      actions: [
        overviewService.overview(),
        this.print,
        this.delete,
        this.ship,
        this.invoice,
      ],
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

    this.filter = this.filterService.filter(m.SalesOrder);

    const internalOrganisationPredicate: Equals = {
      kind: 'Equals',
      propertyType: m.SalesOrder.TakenBy,
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
              pull.SalesOrder({
                predicate,
                sorting: sort
                  ? this.sorterService.sorter(m.SalesOrder)?.create(sort)
                  : null,
                include: {
                  PrintDocument: {
                    Media: x,
                  },
                  ShipToCustomer: x,
                  SalesOrderState: x,
                  SalesInvoicesWhereSalesOrder: x,
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

        this.canCreate = this.internalOrganisation.canExecuteCreateSalesOrder;

        const requests = loaded.collection<SalesOrder>(m.SalesOrder);
        this.table.total = (loaded.value('SalesOrders_total') ?? 0) as number;
        this.table.data = requests
          ?.filter((v) => v.canReadOrderNumber)
          ?.map((v) => {
            return {
              object: v,
              number: `${v.OrderNumber}`,
              shipToCustomer: v.ShipToCustomer && v.ShipToCustomer.DisplayName,
              state: `${v.SalesOrderState && v.SalesOrderState.Name}`,
              invoice: v.SalesInvoicesWhereSalesOrder?.map(
                (w) => w.InvoiceNumber
              ).join(', '),
              customerReference: `${v.Description || ''}`,
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
