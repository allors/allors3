import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { PageEvent } from '@angular/material/paginator';
import { Sort } from '@angular/material/sort';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';
import { formatDistance } from 'date-fns';

import { M } from '@allors/workspace/meta/default';
import { WorkRequirement } from '@allors/workspace/domain/default';
import {
  Action,
  DeleteService,
  Filter,
  MediaService,
  NavigationService,
  ObjectService,
  RefreshService,
  Table,
  TableRow,
  angularSorter,
  FilterField,
  EditService,
  angularFilterFromDefinition,
  OverviewService,
} from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';

import { And, Equals } from '@allors/workspace/domain/system';
import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

interface Row extends TableRow {
  object: WorkRequirement;
  number: string;
  state: string;
  priority: string;
  originator: string;
  equipment: string;
  location: string;
  lastModifiedDate: string;
}

@Component({
  templateUrl: './workrequirement-list.component.html',
  providers: [ContextService],
})
export class WorkRequirementListComponent implements OnInit, OnDestroy {
  public title = 'Service Requests';

  table: Table<Row>;

  edit: Action;
  delete: Action;

  private subscription: Subscription;
  filter: Filter;
  m: M;

  constructor(
    @Self() public allors: ContextService,

    public factoryService: ObjectService,
    public refreshService: RefreshService,
    public overviewService: OverviewService,
    public editService: EditService,
    public deleteService: DeleteService,
    public navigation: NavigationService,
    private internalOrganisationId: InternalOrganisationId,
    public mediaService: MediaService,
    titleService: Title
  ) {
    this.allors.context.name = this.constructor.name;
    titleService.setTitle(this.title);

    this.m = this.allors.context.configuration.metaPopulation as M;

    this.edit = editService.edit();
    this.delete = deleteService.delete(allors.context);
    this.delete.result.subscribe(() => {
      this.table.selection.clear();
    });

    this.table = new Table({
      selection: true,
      columns: [
        { name: 'number', sort: true },
        { name: 'state', sort: true },
        { name: 'priority', sort: true },
        { name: 'originator', sort: true },
        { name: 'equipment', sort: true },
        { name: 'location', sort: true },
        { name: 'lastModifiedDate', sort: true },
      ],
      actions: [overviewService.overview(), this.edit, this.delete],
      defaultAction: overviewService.overview(),
      pageSize: 50,
      initialSort: 'number',
      initialSortDirection: 'desc',
    });
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.filter = angularFilterFromDefinition(m.WorkRequirement);

    const internalOrganisationPredicate: Equals = { kind: 'Equals', propertyType: m.WorkRequirement.ServicedBy };
    const predicate: And = { kind: 'And', operands: [internalOrganisationPredicate, this.filter.definition.predicate] };

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
        switchMap(([, filterFields, sort, pageEvent, internalOrganisationId]: [Date, FilterField[], Sort, PageEvent, number]) => {
          internalOrganisationPredicate.value = internalOrganisationId;

          const pulls = [
            pull.WorkRequirement({
              predicate,
              sorting: sort ? angularSorter(m.WorkRequirement)?.create(sort) : null,
              arguments: this.filter.parameters(filterFields),
              skip: pageEvent.pageIndex * pageEvent.pageSize,
              take: pageEvent.pageSize,
            }),
          ];

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();
        const objects = loaded.collection<WorkRequirement>(m.WorkRequirement);
        this.table.total = loaded.value('Requirements_total') as number;
        this.table.data = objects?.map((v) => {
          return {
            object: v,
            number: v.RequirementNumber,
            state: v.RequirementStateName,
            priority: v.PriorityName,
            originator: v.OriginatorName,
            equipment: v.FixedAssetName,
            location: v.Location,
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
