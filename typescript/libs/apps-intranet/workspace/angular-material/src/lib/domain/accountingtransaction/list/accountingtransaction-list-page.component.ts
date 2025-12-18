import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';
import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { PageEvent } from '@angular/material/paginator';
import { Title } from '@angular/platform-browser';
import { Sort } from '@angular/material/sort';

import { M } from '@allors/default/workspace/meta';
import { And, Equals } from '@allors/system/workspace/domain';
import {
  AccountingTransaction,
  InternalOrganisation,
} from '@allors/default/workspace/domain';
import {
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
  OverviewActionService,
  SorterService,
} from '@allors/base/workspace/angular-material/application';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { format } from 'date-fns';

interface Row extends TableRow {
  object: AccountingTransaction;
  number: string;
  type: string;
  from: string;
  to: string;
  entryDate: string;
  invoice: string;
  shipment: string;
  workEffort: string;
  amount: string;
  exported: string;
}

@Component({
  templateUrl: './accountingtransaction-list-page.component.html',
  providers: [ContextService],
})
export class AccountingTransactionListPageComponent
  implements OnInit, OnDestroy
{
  public title = 'Accounting Transactions';

  table: Table<Row>;

  internalOrganisation: InternalOrganisation;

  private subscription: Subscription;
  filter: Filter;
  m: M;

  constructor(
    @Self() public allors: ContextService,

    public refreshService: RefreshService,
    public overviewService: OverviewActionService,
    public navigation: NavigationService,
    public mediaService: MediaService,
    public filterService: FilterService,
    public sorterService: SorterService,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId,
    titleService: Title
  ) {
    this.allors.context.name = this.constructor.name;
    titleService.setTitle(this.title);

    this.m = this.allors.context.configuration.metaPopulation as M;

    this.table = new Table({
      selection: true,
      columns: [
        { name: 'number', sort: true },
        { name: 'type', sort: true },
        { name: 'from', sort: true },
        { name: 'to', sort: true },
        { name: 'entryDate', sort: true },
        { name: 'invoice', sort: true },
        { name: 'shipment', sort: true },
        { name: 'workEffort', sort: true },
        { name: 'amount', sort: true },
        { name: 'exported', sort: true },
      ],
      actions: [overviewService.overview()],
      defaultAction: overviewService.overview(),
      pageSize: 50,
    });
  }

  ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;

    this.filter = this.filterService.filter(m.AccountingTransaction);

    const internalOrganisationPredicate: Equals = {
      kind: 'Equals',
      propertyType: m.AccountingTransaction.InternalOrganisation,
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
              pull.AccountingTransaction({
                predicate: predicate,
                sorting: sort
                  ? this.sorterService
                      .sorter(m.AccountingTransaction)
                      ?.create(sort)
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

        const objects = loaded.collection<AccountingTransaction>(
          this.m.AccountingTransaction
        );
        this.table.data = objects?.map((v) => {
          return {
            object: v,
            number: `${v.TransactionNumber}`,
            type: `${v.AccountingTransactionTypeName}`,
            from: `${v.FromPartyDisplayName}`,
            to: `${v.ToPartyDisplayName}`,
            entryDate: `${
              v.EntryDate && format(new Date(v.EntryDate), 'dd-MM-yyyy')
            }`,
            invoice: v.InvoiceNumber != null ? `${v.InvoiceNumber}` : '',
            shipment: v.ShipmentNumber != null ? `${v.ShipmentNumber}` : '',
            workEffort:
              v.WorkEffortNumber != null ? `${v.WorkEffortNumber}` : '',
            amount: `${v.DerivedTotalAmount}`,
            exported: v.Exported ? 'Yes' : 'No',
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
