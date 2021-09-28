import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';
import { format, formatDistance } from 'date-fns';

import { M } from '@allors/workspace/meta/default';
import { Person, Organisation, InternalOrganisation, Quote } from '@allors/workspace/domain/default';
import { Action, DeleteService, Filter, MediaService, NavigationService, RefreshService, Table, TableRow, TestScope, UserId, OverviewService } from '@allors/workspace/angular/base';
import { SessionService } from '@allors/workspace/angular/core';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { PrintService } from '../../../actions/print/print.service';
import { FetcherService } from '../../../services/fetcher/fetcher-service';

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
  providers: [SessionService],
})
export class ProductQuoteListComponent extends TestScope implements OnInit, OnDestroy {
  public title = 'Quotes';

  table: Table<Row>;

  delete: Action;
  print: Action;

  user: Person;
  internalOrganisation: Organisation;
  canCreate: boolean;

  private subscription: Subscription;
  filter: Filter;

  constructor(
    @Self() public allors: SessionService,

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
    super();

    titleService.setTitle(this.title);

    this.delete = deleteService.delete(allors.client, allors.session);
    this.delete.result.subscribe(() => {
      this.table.selection.clear();
    });

    this.print = printService.print();

    this.table = new Table({
      selection: true,
      columns: [{ name: 'number', sort: true }, { name: 'to' }, { name: 'state' }, { name: 'description', sort: true }, { name: 'responseRequired', sort: true }, { name: 'lastModifiedDate', sort: true }],
      actions: [overviewService.overview(), this.print, this.delete],
      defaultAction: overviewService.overview(),
      pageSize: 50,
      initialSort: 'number',
      initialSortDirection: 'desc',
    });
  }

  ngOnInit(): void {
    const m = this.allors.workspace.configuration.metaPopulation as M;
    const { pullBuilder: pull } = m;
    const x = {};
    this.filter = m.ProductQuote.filter = m.ProductQuote.filter ?? new Filter(m.ProductQuote.filterDefinition);

    const internalOrganisationPredicate = new Equals({ propertyType: m.Quote.Issuer });
    const predicate = new And([internalOrganisationPredicate, this.filter.definition.predicate]);

    this.subscription = combineLatest([this.refreshService.refresh$, this.filter.fields$, this.table.sort$, this.table.pager$, this.internalOrganisationId.observable$])
      .pipe(
        scan(([previousRefresh, previousFilterFields], [refresh, filterFields, sort, pageEvent, internalOrganisationId]) => {
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

          return [refresh, filterFields, sort, pageEvent, internalOrganisationId];
        }),
        switchMap(([, filterFields, sort, pageEvent, internalOrganisationId]) => {
          internalOrganisationPredicate.object = internalOrganisationId;

          const pulls = [
            this.fetcher.internalOrganisation,
            pull.Person({
              object: this.userId.value,
            }),
            pull.Quote({
              predicate,
              sorting: sort ? m.ProductQuote.sorter.create(sort) : null,
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

          return this.allors.client.pullReactive(this.allors.session, pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.session.reset();

        this.internalOrganisation = loaded.object<Organisation>(m.InternalOrganisation);
        this.user = loaded.object<Person>(m.Person);

        this.canCreate = this.internalOrganisation.canExecuteCreateQuote;

        const quotes = loaded.collection<Quote>(m.Quote);
        this.table.total = loaded.value('Quotes_total') as number;
        this.table.data = quotes
          .filter((v) => v.canReadQuoteNumber)
          .map((v) => {
            return {
              object: v,
              number: `${v.QuoteNumber}`,
              to: v.Receiver && v.Receiver.displayName,
              state: `${v.QuoteState && v.QuoteState.Name}`,
              description: `${v.Description || ''}`,
              responseRequired: v.RequiredResponseDate && format(new Date(v.RequiredResponseDate), 'dd-MM-yyyy'),
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
