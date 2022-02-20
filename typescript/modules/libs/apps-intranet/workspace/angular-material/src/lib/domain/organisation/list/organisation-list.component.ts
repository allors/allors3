import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';
import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { PageEvent } from '@angular/material/paginator';
import { Title } from '@angular/platform-browser';
import { Sort } from '@angular/material/sort';

import { M } from '@allors/default/workspace/meta';
import { And } from '@allors/system/workspace/domain';
import {
  InternalOrganisation,
  Organisation,
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
} from '@allors/base/workspace/angular/foundation';
import {
  NavigationService,
  ScopedService,
} from '@allors/base/workspace/angular/application';
import {
  DeleteService,
  EditRoleService,
  MethodService,
  OverviewService,
  SorterService,
} from '@allors/base/workspace/angular-material/application';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { FetcherService } from '../../../..';
import { formatDistance } from 'date-fns';

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
  providers: [ContextService],
})
export class OrganisationListComponent implements OnInit, OnDestroy {
  public title = 'Organisations';

  table: Table<Row>;

  delete: Action;

  private subscription: Subscription;
  internalOrganisation: InternalOrganisation;
  filter: Filter;
  m: M;

  constructor(
    @Self() public allors: ContextService,

    public scopedService: ScopedService,
    public refreshService: RefreshService,
    public overviewService: OverviewService,
    public deleteService: DeleteService,
    public methodService: MethodService,
    public navigation: NavigationService,
    public mediaService: MediaService,
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

    this.table = new Table({
      selection: true,
      columns: [
        { name: 'name', sort: true },
        'street',
        'locality',
        'country',
        'phone',
        'isCustomer',
        'isSupplier',
        { name: 'lastModifiedDate', sort: true },
      ],
      actions: [overviewService.overview(), this.delete],
      defaultAction: overviewService.overview(),
      pageSize: 50,
    });
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.filter = this.filterService.filter(m.Organisation);

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
              this.fetcher.internalOrganisation,
              pull.Organisation({
                predicate: this.filter.definition.predicate,
                sorting: sort
                  ? this.sorterService.sorter(m.Organisation)?.create(sort)
                  : null,
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

            return this.allors.context.pull(pulls);
          }
        )
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        this.internalOrganisation =
          this.fetcher.getInternalOrganisation(loaded);
        const organisations = loaded.collection<Organisation>(m.Organisation);

        this.table.total = (loaded.value('Organisations_total') ?? 0) as number;
        this.table.data = organisations?.map((v) => {
          return {
            object: v,
            name: v.DisplayName,
            street: v.DisplayAddress,
            locality: v.DisplayAddress2,
            country: v.DisplayAddress3,
            phone: v.DisplayPhone,
            isCustomer:
              v.CustomerRelationshipsWhereCustomer.length > 0 ? 'Yes' : 'No',
            isSupplier:
              v.SupplierRelationshipsWhereSupplier.length > 0 ? 'Yes' : 'No',
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
