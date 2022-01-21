import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';
import { formatDistance } from 'date-fns';

import { M } from '@allors/default/workspace/meta';
import { Shipment } from '@allors/default/workspace/domain';
import {
  Action,
  DeleteService,
  Filter,
  MediaService,
  MethodService,
  NavigationService,
  RefreshService,
  Table,
  TableRow,
  OverviewService,
  Sorter,
  angularFilterFromDefinition,
  angularSorter,
  FilterField,
} from '@allors/workspace/angular/base';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { PrintService } from '../../../actions/print/print.service';
import { And, Equals } from '@allors/system/workspace/domain';
import { Sort } from '@angular/material/sort';
import { PageEvent } from '@angular/material/paginator';

interface Row extends TableRow {
  object: Shipment;
  number: string;
  from: string;
  to: string;
  state: string;
  lastModifiedDate: string;
}

@Component({
  templateUrl: './shipment-list.component.html',
  providers: [ContextService],
})
export class ShipmentListComponent implements OnInit, OnDestroy {
  public title = 'Shipments';

  m: M;

  table: Table<Row>;

  delete: Action;

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
    titleService: Title
  ) {
    titleService.setTitle(this.title);

    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;

    this.delete = deleteService.delete(allors.context);
    this.delete.result.subscribe(() => {
      this.table.selection.clear();
    });

    this.m = this.allors.context.configuration.metaPopulation as M;

    const sort = true;
    this.table = new Table({
      selection: true,
      columns: [
        { name: 'number', sort },
        { name: 'from', sort },
        { name: 'to', sort },
        { name: 'state', sort },
        { name: 'lastModifiedDate', sort: true },
      ],
      actions: [overviewService.overview(), this.delete],
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

    this.filter = angularFilterFromDefinition(m.Shipment);

    const fromInternalOrganisationPredicate: Equals = {
      kind: 'Equals',
      propertyType: m.Shipment.ShipFromParty,
    };
    const toInternalOrganisationPredicate: Equals = {
      kind: 'Equals',
      propertyType: m.Shipment.ShipToParty,
    };

    const predicate: And = {
      kind: 'And',
      operands: [
        {
          kind: 'Or',
          operands: [
            fromInternalOrganisationPredicate,
            toInternalOrganisationPredicate,
          ],
        },
        this.filter.definition.predicate,
      ],
    };

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
            fromInternalOrganisationPredicate.value = internalOrganisationId;
            toInternalOrganisationPredicate.value = internalOrganisationId;

            const pulls = [
              pull.Shipment({
                predicate,
                sorting: sort ? angularSorter(m.Shipment)?.create(sort) : null,
                include: {
                  ShipToParty: x,
                  ShipFromParty: x,
                  ShipmentState: x,
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
        const objects = loaded.collection<Shipment>(m.Shipment);
        this.table.total = (loaded.value('Shipments_total') ?? 0) as number;
        this.table.data = objects?.map((v) => {
          return {
            object: v,
            number: `${v.ShipmentNumber}`,
            from: v.ShipFromParty.DisplayName,
            to: v.ShipToParty.DisplayName,
            state: `${v.ShipmentState && v.ShipmentState.Name}`,
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
