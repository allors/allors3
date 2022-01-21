import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';
import { format, formatDistance } from 'date-fns';

import { M } from '@allors/default/workspace/meta';
import {
  Person,
  Organisation,
  InternalOrganisation,
  Quote,
} from '@allors/default/workspace/domain';
import {
  Action,
  DeleteService,
  Filter,
  MediaService,
  NavigationService,
  RefreshService,
  Table,
  TableRow,
  UserId,
  OverviewService,
  angularFilterFromDefinition,
  angularSorter,
  FilterField,
} from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { PrintService } from '../../../actions/print/print.service';
import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { And, Equals } from '@allors/system/workspace/domain';
import { PageEvent } from '@angular/material/paginator';
import { Sort } from '@angular/material/sort';

interface Row extends TableRow {
  object: Quote;
  number: string;
  to: string;
  state: string;
  responseRequired: string;
  description: string;
  lastModifiedDate: string;
}

@Component({
  templateUrl: './productquote-list.component.html',
  providers: [ContextService],
})
export class ProductQuoteListComponent implements OnInit, OnDestroy {
  public title = 'Quotes';

  table: Table<Row>;

  delete: Action;
  print: Action;

  user: Person;
  internalOrganisation: InternalOrganisation;
  canCreate: boolean;

  private subscription: Subscription;
  filter: Filter;
  m: M;

  constructor(
    @Self() public allors: ContextService,
    public refreshService: RefreshService,
    public overviewService: OverviewService,
    public deleteService: DeleteService,
    public printService: PrintService,
    public navigation: NavigationService,
    public mediaService: MediaService,
    private internalOrganisationId: InternalOrganisationId,
    private userId: UserId,
    private fetcher: FetcherService,
    titleService: Title
  ) {
    this.allors.context.name = this.constructor.name;
    titleService.setTitle(this.title);

    this.m = this.allors.context.configuration.metaPopulation as M;

    this.delete = deleteService.delete(allors.context);
    this.delete.result.subscribe(() => {
      this.table.selection.clear();
    });

    this.print = printService.print();

    this.table = new Table({
      selection: true,
      columns: [
        { name: 'number', sort: true },
        { name: 'to' },
        { name: 'state' },
        { name: 'description', sort: true },
        { name: 'responseRequired', sort: true },
        { name: 'lastModifiedDate', sort: true },
      ],
      actions: [overviewService.overview(), this.print, this.delete],
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

    this.filter = angularFilterFromDefinition(m.ProductQuote);

    const internalOrganisationPredicate: Equals = {
      kind: 'Equals',
      propertyType: m.Quote.Issuer,
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
              pull.Quote({
                predicate,
                sorting: sort
                  ? angularSorter(m.ProductQuote)?.create(sort)
                  : null,
                include: {
                  PrintDocument: {
                    Media: x,
                  },
                  Receiver: x,
                  QuoteState: x,
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

        this.canCreate = this.internalOrganisation.canExecuteCreateQuote;

        const quotes = loaded.collection<Quote>(m.Quote);
        this.table.total = (loaded.value('Quotes_total') ?? 0) as number;
        this.table.data = quotes
          ?.filter((v) => v.canReadQuoteNumber)
          ?.map((v) => {
            return {
              object: v,
              number: `${v.QuoteNumber}`,
              to: v.Receiver && v.Receiver.DisplayName,
              state: `${v.QuoteState && v.QuoteState.Name}`,
              description: `${v.Description || ''}`,
              responseRequired:
                v.RequiredResponseDate &&
                format(new Date(v.RequiredResponseDate), 'dd-MM-yyyy'),
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
