import { Component, OnDestroy, Self, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { scan, switchMap } from 'rxjs/operators';

import { Action, angularFilter, angularFilterFromDefinition, angularSorter, DeleteService, Filter, OverviewService, RefreshService, Table, TableRow, TestScope } from '@allors/workspace/angular/base';
import { Organisation } from '@allors/workspace/domain/default';
import { ContextService } from '@allors/workspace/angular/core';
import { M } from '@allors/workspace/meta/default';

interface Row extends TableRow {
  object: Organisation;
  name: string | null;
  owner: string | null;
}

@Component({
  templateUrl: './organisations.component.html',
  providers: [ContextService],
})
export class OrganisationsComponent extends TestScope implements OnInit, OnDestroy {
  title = 'Organisations';

  table: Table<Row>;

  overview: Action;
  delete: Action;

  filter: Filter;

  private subscription: Subscription;
  m: M;

  constructor(@Self() public allors: ContextService, public refreshService: RefreshService, public deleteService: DeleteService, public overviewService: OverviewService, private titleService: Title) {
    super();

    this.allors.context.name = this.constructor.name;
    this.titleService.setTitle(this.title);

    this.m = this.allors.context.configuration.metaPopulation as M;

    this.overview = overviewService.overview();
    this.delete = deleteService.delete(allors.context);
    this.delete.result.subscribe(() => {
      this.table.selection.clear();
    });

    this.table = new Table({
      selection: true,
      columns: [{ name: 'name', sort: true }, 'owner'],
      actions: [this.overview, this.delete],
      defaultAction: this.overview,
    });
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: p } = m;

    this.filter = angularFilterFromDefinition(m.Organisation);

    this.subscription = combineLatest([this.refreshService.refresh$, this.filter.fields$, this.table.sort$, this.table.pager$])
      .pipe(
        scan(([previousRefresh, previousFilterFields], [refresh, filterFields, sort, pageEvent]) => [
          refresh,
          filterFields,
          sort,
          previousRefresh !== refresh || filterFields !== previousFilterFields ? Object.assign({ pageIndex: 0 }, pageEvent) : pageEvent,
        ]),
        switchMap(([, filterFields, sort, pageEvent]) => {
          const pulls = [
            p.Organisation({
              predicate: this.filter.definition.predicate,
              sorting: sort ? angularSorter(m.Organisation)?.create(sort) : null,
              include: {
                Owner: {},
                Employees: {},
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
        const organisations = loaded.collection<Organisation>(m.Organisation);
        this.table.total = loaded.value('Organisations_total') as number;
        this.table.data = organisations.map((v) => {
          return {
            object: v,
            name: v.Name,
            owner: v.Owner?.UserName ?? null,
          };
        });
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
