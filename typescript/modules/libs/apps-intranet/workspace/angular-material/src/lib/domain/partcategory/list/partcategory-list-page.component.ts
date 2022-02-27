import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';
import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { PageEvent } from '@angular/material/paginator';
import { Title } from '@angular/platform-browser';
import { Sort } from '@angular/material/sort';

import { M } from '@allors/default/workspace/meta';
import { And } from '@allors/system/workspace/domain';
import { PartCategory } from '@allors/default/workspace/domain';
import {
  Action,
  Filter,
  FilterDefinition,
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
import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

interface Row extends TableRow {
  object: PartCategory;
  name: string;
  primaryParent: string;
  secondaryParents: string;
  scope: string;
}

@Component({
  templateUrl: './partcategory-list-page.component.html',
  providers: [ContextService],
})
export class PartCategoryListPageComponent implements OnInit, OnDestroy {
  public title = 'Part Categories';

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
    private internalOrganisationId: InternalOrganisationId,
    public filterService: FilterService,
    public sorterService: SorterService,
    titleService: Title
  ) {
    this.allors.context.name = this.constructor.name;
    titleService.setTitle(this.title);

    this.m = this.allors.context.configuration.metaPopulation as M;

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
        { name: 'name', sort: true },
        { name: 'primaryParent', sort: true },
        { name: 'secondaryParents', sort: true },
      ],
      actions: [this.edit, this.delete],
      defaultAction: this.edit,
      pageSize: 50,
    });
  }

  ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const predicate: And = {
      kind: 'And',
      operands: [
        { kind: 'Like', roleType: m.PartCategory.Name, parameter: 'name' },
      ],
    };

    const filterDefinition = new FilterDefinition(predicate);
    this.filter = new Filter(filterDefinition);

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
          ([, filterFields, sort, pageEvent]: [
            Date,
            FilterField[],
            Sort,
            PageEvent
          ]) => {
            const pulls = [
              pull.PartCategory({
                predicate,
                sorting: sort
                  ? this.sorterService.sorter(m.PartCategory)?.create(sort)
                  : null,
                include: {
                  CategoryImage: x,
                  LocalisedNames: x,
                  LocalisedDescriptions: x,
                  PrimaryParent: {
                    PrimaryAncestors: x,
                  },
                  SecondaryParents: {
                    PrimaryAncestors: x,
                  },
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

        const objects = loaded.collection<PartCategory>(m.PartCategory);
        this.table.total = (loaded.value('PartCategories_total') ??
          0) as number;
        this.table.data = objects?.map((v) => {
          return {
            object: v,
            name: v.Name,
            primaryParent: v.PrimaryParent && v.PrimaryParent.DisplayName,
            secondaryParents: v.SecondaryParents?.map(
              (w) => w.DisplayName
            ).join(', '),
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
