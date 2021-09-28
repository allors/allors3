import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { SerialisedItemCharacteristicType } from '@allors/workspace/domain/default';
import { Action, DeleteService, EditService, Filter, MediaService, NavigationService, RefreshService, Table, TableRow, TestScope, OverviewService } from '@allors/workspace/angular/base';
import { SessionService } from '@allors/workspace/angular/core';

interface Row extends TableRow {
  object: SerialisedItemCharacteristicType;
  name: string;
  uom: string;
  active: boolean;
}

@Component({
  templateUrl: './serialiseditemcharacteristic-list.component.html',
  providers: [SessionService],
})
export class SerialisedItemCharacteristicListComponent extends TestScope implements OnInit, OnDestroy {
  public title = 'Product Characteristics';

  table: Table<Row>;

  edit: Action;
  delete: Action;

  private subscription: Subscription;
  filter: Filter;

  constructor(
    @Self() public allors: SessionService,

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

    this.edit = editService.edit();
    this.edit.result.subscribe(() => {
      this.table.selection.clear();
    });

    this.delete = deleteService.delete(allors);
    this.delete.result.subscribe(() => {
      this.table.selection.clear();
    });

    this.table = new Table({
      selection: true,
      columns: [
        { name: 'name', sort: true },
        { name: 'uom', sort: true },
        { name: 'active', sort: true },
      ],
      actions: [this.edit, this.delete],
      defaultAction: this.edit,
    });
  }

  ngOnInit(): void {
    const m = this.allors.workspace.configuration.metaPopulation as M;
    const { pullBuilder: pull } = m;
    const x = {};
    this.filter = m.SerialisedItemCharacteristic.filter = m.SerialisedItemCharacteristic.filter ?? new Filter(m.SerialisedItemCharacteristic.filterDefinition);

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
            pull.SerialisedItemCharacteristicType({
              predicate: this.filter.definition.predicate,
              sorting: sort ? m.SerialisedItemCharacteristic.sorter.create(sort) : null,
              include: {
                UnitOfMeasure: x,
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

        const objects = loaded.collection<SerialisedItemCharacteristicType>(m.SerialisedItemCharacteristicType);
        this.table.total = loaded.value('SerialisedItemCharacteristicTypes_total') as number;
        this.table.data = objects.map((v) => {
          return {
            object: v,
            name: `${v.Name}`,
            uom: v.UnitOfMeasure ? v.UnitOfMeasure.Name : '',
            active: v.IsActive,
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
