import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';

import { Action, angularFilterFromDefinition, angularSorter, DeleteService, EditService, Filter, FilterField, MediaService, NavigationService, OverviewService, RefreshService, Table, TableRow } from '@allors/workspace/angular/base';
import { Carrier } from '@allors/workspace/domain/default';
import { ContextService, WorkspaceService } from '@allors/workspace/angular/core';
import { M } from '@allors/workspace/meta/default';
import { Sort } from '@angular/material/sort';
import { PageEvent } from '@angular/material/paginator';

interface Row extends TableRow {
  object: Carrier;
  name: string;
}

@Component({
  templateUrl: './carrier-list.component.html',
  providers: [ContextService],
})
export class CarrierListComponent implements OnInit, OnDestroy {
  public title = 'Carriers';

  table: Table<Row>;

  edit: Action;
  delete: Action;

  private subscription: Subscription;
  filter: Filter;
  m: M;

  constructor(
    @Self() public allors: ContextService,
    public workspaceService: WorkspaceService,
    public refreshService: RefreshService,
    public overviewService: OverviewService,
    public editService: EditService,
    public deleteService: DeleteService,
    public navigation: NavigationService,
    public mediaService: MediaService,
    titleService: Title
  ) {
    this.allors.context.name = this.constructor.name;
    titleService.setTitle(this.title);

    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;

    this.edit = editService.edit();
    this.edit.result.subscribe(() => {
      this.table.selection.clear();
    });

    this.delete = deleteService.delete(allors.context);
    this.delete.result.subscribe(() => {
      this.table.selection.clear();
    });

    this.table = new Table({
      selection: true,
      columns: [{ name: 'name', sort: true }],
      actions: [this.edit, this.delete],
      defaultAction: this.edit,
      pageSize: 50,
    });
  }

  ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;

    this.filter = angularFilterFromDefinition(m.Carrier);

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
        switchMap(([, filterFields, sort, pageEvent]: [Date, FilterField[], Sort, PageEvent]) => {
          const pulls = [
            pull.Carrier({
              predicate: this.filter.definition.predicate,
              sorting: sort ? angularSorter(m.Carrier)?.create(sort) : null,
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

        const objects = loaded.collection<Carrier>(m.Carrier);
        this.table.total = (loaded.value('Carriers_total') ?? 0) as number;
        this.table.data = objects?.map((v) => {
          return {
            object: v,
            name: `${v.Name}`,
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
