import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';
import { formatDistance } from 'date-fns';

import { M } from '@allors/workspace/meta/default';
import { Organisation } from '@allors/workspace/domain/default';
import { Action, DeleteService, Filter, MediaService, MethodService, NavigationService, ObjectService, OverviewService, RefreshService, Table, TableRow, TestScope } from '@allors/workspace/angular/base';
import { SessionService } from '@allors/workspace/angular/core';

import { FetcherService } from '../../../services/fetcher/fetcher-service';

interface Row extends TableRow {
  object: Organisation;
  name: string;
  street: string;
  locality: string;
  country: string;
  phone: string;
  isCustomer: string;
  isSupplier: string;
  lastModifiedDate: string;
}

@Component({
  templateUrl: './organisation-list.component.html',
  providers: [SessionService],
})
export class OrganisationListComponent extends TestScope implements OnInit, OnDestroy {
  public title = 'Organisations';

  table: Table<Row>;

  delete: Action;

  private subscription: Subscription;
  internalOrganisation: Organisation;
  filter: Filter;
  m: M;

  constructor(
    @Self() public allors: SessionService,

    public factoryService: ObjectService,
    public refreshService: RefreshService,
    public overviewService: OverviewService,
    public deleteService: DeleteService,
    public methodService: MethodService,
    public navigation: NavigationService,
    public mediaService: MediaService,
    private fetcher: FetcherService,
    titleService: Title
  ) {
    super();

    titleService.setTitle(this.title);

    this.m = this.allors.workspace.configuration.metaPopulation as M;
    
    this.delete = deleteService.delete(allors.client, allors.session);
    this.delete.result.subscribe(() => {
      this.table.selection.clear();
    });

    this.table = new Table({
      selection: true,
      columns: [{ name: 'name', sort: true }, 'street', 'locality', 'country', 'phone', 'isCustomer', 'isSupplier', { name: 'lastModifiedDate', sort: true }],
      actions: [overviewService.overview(), this.delete],
      defaultAction: overviewService.overview(),
      pageSize: 50,
    });
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.filter = m.Organisation._.filter ??= new Filter(m.Organisation._.filterDefinition);

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
            this.fetcher.internalOrganisation,
            pull.Organisation({
              predicate: this.filter.definition.predicate,
              sorting: sort ? m.Organisation._.sorter?.create(sort) : null,
              include: {
                CustomerRelationshipsWhereCustomer: x,
                SupplierRelationshipsWhereSupplier: x,
                PartyContactMechanisms: {
                  ContactMechanism: {
                    PostalAddress_Country: x,
                  },
                },
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
        const organisations = loaded.collection<Organisation>(m.Organisation);

        this.table.total = loaded.value('Organisations_total') as number;
        this.table.data = organisations.map((v) => {
          return {
            object: v,
            name: v.DisplayName,
            street: v.DisplayAddress,
            locality: v.DisplayAddress2,
            country: v.DisplayAddress3,
            phone: v.DisplayPhone,
            isCustomer: v.CustomerRelationshipsWhereCustomer.length > 0 ? 'Yes' : 'No',
            isSupplier: v.SupplierRelationshipsWhereSupplier.length > 0 ? 'Yes' : 'No',
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
