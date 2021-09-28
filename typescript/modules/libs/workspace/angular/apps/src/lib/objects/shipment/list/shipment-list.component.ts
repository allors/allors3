import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';
import { formatDistance } from 'date-fns';

import { M } from '@allors/workspace/meta/default';
import { Shipment } from '@allors/workspace/domain/default';
import { Action, DeleteService, Filter, MediaService, MethodService, NavigationService, RefreshService, Table, TableRow, TestScope, OverviewService, Sorter } from '@allors/workspace/angular/base';
import { SessionService } from '@allors/workspace/angular/core';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { PrintService } from '../../../actions/print/print.service';

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
  providers: [SessionService],
})
export class ShipmentListComponent extends TestScope implements OnInit, OnDestroy {
  public title = 'Shipments';

  m: M;

  table: Table<Row>;

  delete: Action;

  private subscription: Subscription;
  filter: Filter;

  constructor(
    @Self() public allors: SessionService,

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
    super();

    titleService.setTitle(this.title);

    this.delete = deleteService.delete(allors);
    this.delete.result.subscribe(() => {
      this.table.selection.clear();
    });

    this.m = this.allors.workspace.configuration.metaPopulation as M;

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
    const m = this.allors.workspace.configuration.metaPopulation as M;
    const { pullBuilder: pull } = m;
    const x = {};
    this.filter = m.Shipment.filter = m.Shipment.filter ?? new Filter(m.Shipment.filterDefinition);

    const fromInternalOrganisationPredicate = new Equals({ propertyType: m.Shipment.ShipFromParty });
    const toInternalOrganisationPredicate = new Equals({ propertyType: m.Shipment.ShipToParty });

    const predicate = new And([new Or([fromInternalOrganisationPredicate, toInternalOrganisationPredicate]), this.filter.definition.predicate]);

    const sorter = new Sorter({});

    this.subscription = combineLatest(this.refreshService.refresh$, this.filter.fields$, this.table.sort$, this.table.pager$, this.internalOrganisationId.observable$)
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
          fromInternalOrganisationPredicate.object = internalOrganisationId;
          toInternalOrganisationPredicate.object = internalOrganisationId;

          const pulls = [
            pull.Shipment({
              predicate,
              sorting: sort ? m.Shipment.sorter.create(sort) : null,
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

          return this.allors.client.pullReactive(this.allors.session, pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.session.reset();
        const objects = loaded.collection<Shipment>(m.Shipment);
        this.table.total = loaded.value('Shipments_total') as number;
        this.table.data = objects.map((v) => {
          return {
            object: v,
            number: `${v.ShipmentNumber}`,
            from: v.ShipFromParty.displayName,
            to: v.ShipToParty.displayName,
            state: `${v.ShipmentState && v.ShipmentState.Name}`,
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
