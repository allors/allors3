import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { PositionType } from '@allors/workspace/domain/default';
import { Action, DeleteService, EditService, Filter, MediaService, NavigationService, OverviewService, RefreshService, Table, TableRow, TestScope } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';

interface Row extends TableRow {
  object: PositionType;
  title: string;
  description: string;
}

@Component({
  templateUrl: './positiontype-list.component.html',
  providers: [ContextService],
})
export class PositionTypesOverviewComponent extends TestScope implements OnInit, OnDestroy {
  public title = 'Position Types';

  table: Table<Row>;

  edit: Action;
  delete: Action;

  private subscription: Subscription;
  filter: Filter;
  m: M;

  constructor(
    @Self() public allors: ContextService,
    public refreshService: RefreshService,
    public overviewService: OverviewService,
    public editService: EditService,
    public deleteService: DeleteService,
    public navigation: NavigationService,
    public mediaService: MediaService,
    titleService: Title
  ) {
    super();

    titleService.setTitle(this.title);

    this.m = this.allors.context.configuration.metaPopulation as M;

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
      columns: [{ name: 'title', sort: true }, { name: 'description' }],
      actions: [this.edit, this.delete],
      defaultAction: this.edit,
      pageSize: 50,
    });
  }

  ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.filter = m.PositionType._.filter ??= new Filter(m.PositionType._.filterDefinition);

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
            pull.PositionType({
              predicate: this.filter.definition.predicate,
              sorting: sort ? m.PositionType._.sorter?.create(sort) : null,
              include: {
                PositionTypeRate: x,
              },
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

        const objects = loaded.collection<PositionType>(m.PositionType);
        this.table.total = loaded.value('PositionTypes_total') as number;
        this.table.data = objects?.map((v) => {
          return {
            object: v,
            title: v.Title,
            description: v.Description,
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
