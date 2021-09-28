import { Component, OnDestroy, Self, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { scan, switchMap } from 'rxjs/operators';

import { Action, DeleteService, Filter, OverviewService, RefreshService, Table, TableRow, TestScope } from '@allors/workspace/angular/base';
import { Organisation } from '@allors/workspace/domain/default';
import { SessionService } from '@allors/workspace/angular/core';
import { M } from '@allors/workspace/meta/default';

interface Row extends TableRow {
  object: Organisation;
  name: string | null;
  owner: string | null;
}

@Component({
  templateUrl: './organisations.component.html',
  providers: [SessionService],
})
export class OrganisationsComponent extends TestScope implements OnInit, OnDestroy {
  title = 'Organisations';

  table: Table<Row>;

  overview: Action;
  delete: Action;

  filter: Filter;

  private subscription: Subscription;

  constructor(@Self() public allors: SessionService, public refreshService: RefreshService, public deleteService: DeleteService, public overviewService: OverviewService, private titleService: Title) {
    super();

    this.titleService.setTitle(this.title);

    this.overview = overviewService.overview();
    this.delete = deleteService.delete(allors.client, allors.session);
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
    const m = this.allors.workspace.configuration.metaPopulation as M;
    const { pullBuilder: p } = m;

    const angularMeta = this.allors.workspace.services.angularMetaService;
    const angularOrganisation = angularMeta.for(m.Organisation);
    this.filter = angularOrganisation.filter ??= new Filter(angularOrganisation.filterDefinition);

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
              sorting: sort ? angularOrganisation.sorter.create(sort) : null,
              include: {
                Owner: {},
                Employees: {},
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
