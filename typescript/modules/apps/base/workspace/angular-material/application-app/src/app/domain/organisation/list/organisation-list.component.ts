import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';
import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Sort } from '@angular/material/sort';
import { PageEvent } from '@angular/material/paginator';
import { M } from '@allors/default/workspace/meta';
import { Organisation } from '@allors/default/workspace/domain';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import {
  angularFilterFromDefinition,
  Filter,
  FilterField,
  MediaService,
  RefreshService,
} from '@allors/base/workspace/angular/foundation';
import {
  Action,
  AllorsPageListComponent,
  CreateService,
  NavigationService,
} from '@allors/base/workspace/angular/application';
import {
  angularSorter,
  CreateDialogData,
  DeleteService,
  MethodService,
  OverviewService,
  Table,
  TableRow,
} from '@allors/base/workspace/angular-material/application';

interface Row extends TableRow {
  object: Organisation;
  name: string;
  country: string;
  owner: string;
}

@Component({
  templateUrl: './organisation-list.component.html',
  providers: [ContextService],
})
export class OrganisationListComponent
  extends AllorsPageListComponent
  implements OnInit, OnDestroy
{
  public override title = 'Organisations';

  table: Table<Row>;

  delete: Action;

  private subscription: Subscription;
  filter: Filter;
  override m: M;

  createData: CreateDialogData;

  constructor(
    @Self() allors: ContextService,
    titleService: Title,
    public createService: CreateService,
    public refreshService: RefreshService,
    public overviewService: OverviewService,
    public deleteService: DeleteService,
    public methodService: MethodService,
    public navigation: NavigationService,
    public mediaService: MediaService
  ) {
    super(allors, titleService);
    this.objectType = this.m.Organisation;
    this.createData = {
      kind: 'CreateDialogData',
      objectType: this.m.Organisation,
    };

    this.delete = deleteService.delete(allors.context);
    this.delete.result.subscribe(() => {
      this.table.selection.clear();
    });

    this.table = new Table({
      selection: true,
      columns: [{ name: 'name', sort: true }, 'country', 'owner'],
      actions: [overviewService.overview(), this.delete],
      defaultAction: overviewService.overview(),
      pageSize: 50,
      initialSort: 'name',
    });
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;

    this.filter = angularFilterFromDefinition(m.Organisation);

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
              pull.Organisation({
                predicate: this.filter.definition.predicate,
                sorting: sort
                  ? angularSorter(m.Organisation)?.create(sort)
                  : null,
                include: {
                  Owner: {},
                  Country: {},
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

        const organisations = loaded.collection<Organisation>(m.Organisation);
        this.table.total = (loaded.value('Organisations_total') ?? 0) as number;
        this.table.data = organisations?.map((v) => {
          return {
            object: v,
            name: v.Name,
            country: v.Country?.Name ?? null,
            owner: v.Owner?.UserName ?? null,
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
