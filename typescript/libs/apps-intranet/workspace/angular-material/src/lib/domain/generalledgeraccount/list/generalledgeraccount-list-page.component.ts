import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';
import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { PageEvent } from '@angular/material/paginator';
import { Title } from '@angular/platform-browser';
import { Sort } from '@angular/material/sort';

import { M } from '@allors/default/workspace/meta';
import {
  GeneralLedgerAccount,
  OrganisationGlAccount,
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
  EditActionService,
  OverviewActionService,
  SorterService,
} from '@allors/base/workspace/angular-material/application';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { format } from 'date-fns';
import { And, Equals } from '@allors/system/workspace/domain';
import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
interface Row extends TableRow {
  object: GeneralLedgerAccount;
  referenceNumber: string;
  name: string;
}

@Component({
  templateUrl: './generalledgeraccount-list-page.component.html',
  providers: [ContextService],
})
export class GeneralLedgerAccountListPageComponent
  implements OnInit, OnDestroy
{
  public title = 'GL Accounts';

  table: Table<Row>;

  edit: Action;
  delete: Action;

  private subscription: Subscription;
  filter: Filter;
  m: M;

  constructor(
    @Self() public allors: ContextService,
    public refreshService: RefreshService,
    public overviewService: OverviewActionService,
    public editRoleService: EditActionService,
    public deleteService: DeleteActionService,
    public navigation: NavigationService,
    public mediaService: MediaService,
    public filterService: FilterService,
    public sorterService: SorterService,
    titleService: Title,
    private internalOrganisationId: InternalOrganisationId
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;

    titleService.setTitle(this.title);

    this.edit = editRoleService.edit();
    this.edit.result.subscribe(() => {
      this.table.selection.clear();
    });

    this.delete = deleteService.delete();
    this.delete.result.subscribe(() => {
      this.table.selection.clear();
    });

    this.table = new Table({
      selection: true,
      columns: [
        { name: 'referenceNumber', sort: true },
        { name: 'name', sort: true },
      ],
      actions: [this.edit, this.delete],
      defaultAction: this.edit,
      pageSize: 50,
      initialSort: 'referenceNumber',
      initialSortDirection: 'desc',
    });
  }

  ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;

    this.filter = this.filterService.filter(m.GeneralLedgerAccount);

    const internalOrganisationPredicate: Equals = {
      kind: 'Equals',
      propertyType: m.OrganisationGlAccount.InternalOrganisation,
    };

    const predicate: And = {
      kind: 'And',
      operands: [
        this.filter.definition.predicate,
        {
          kind: 'ContainedIn',
          propertyType:
            m.GeneralLedgerAccount
              .OrganisationGlAccountsWhereGeneralLedgerAccount,
          extent: {
            kind: 'Filter',
            objectType: m.OrganisationGlAccount,
            predicate: internalOrganisationPredicate,
          },
        },
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
              pull.GeneralLedgerAccount({
                predicate,
                sorting: sort
                  ? this.sorterService
                      .sorter(m.GeneralLedgerAccount)
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

        const objects = loaded.collection<GeneralLedgerAccount>(
          m.GeneralLedgerAccount
        );
        this.table.data = objects?.map((v) => {
          return {
            object: v,
            referenceNumber: v.ReferenceNumber,
            name: v.Name,
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
