import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';

import { M } from '@allors/default/workspace/meta';
import { ProductCategory } from '@allors/default/workspace/domain';
import {
  Action,
  DeleteService,
  EditService,
  Filter,
  MediaService,
  NavigationService,
  RefreshService,
  Table,
  TableRow,
  OverviewService,
  angularFilterFromDefinition,
  angularSorter,
  FilterField,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { And, Equals } from '@allors/system/workspace/domain';
import { Sort } from '@angular/material/sort';
import { PageEvent } from '@angular/material/paginator';

interface Row extends TableRow {
  object: ProductCategory;
  name: string;
  primaryParent: string;
  secondaryParents: string;
  scope: string;
}

@Component({
  templateUrl: './productcategory-list.component.html',
  providers: [ContextService],
})
export class ProductCategoryListComponent implements OnInit, OnDestroy {
  public title = 'Categories';

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
    private internalOrganisationId: InternalOrganisationId,
    titleService: Title
  ) {
    this.allors.context.name = this.constructor.name;
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
      columns: [
        { name: 'name', sort: true },
        { name: 'primaryParent', sort: true },
        { name: 'secondaryParents', sort: true },
        { name: 'scope', sort: true },
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

    this.filter = angularFilterFromDefinition(m.ProductCategory);

    const internalOrganisationPredicate: Equals = {
      kind: 'Equals',
      propertyType: m.ProductCategory.InternalOrganisation,
    };
    const predicate: And = {
      kind: 'And',
      operands: [
        internalOrganisationPredicate,
        this.filter.definition.predicate,
      ],
    };

    this.subscription = combineLatest([
      this.refreshService.refresh$,
      this.filter.fields$,
      this.table.sort$,
      this.table.pager$,
      this.internalOrganisationId.observable$,
    ])
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
            internalOrganisationPredicate.value = internalOrganisationId;

            const pulls = [
              pull.ProductCategory({
                predicate: predicate,
                sorting: sort
                  ? angularSorter(m.ProductCategory)?.create(sort)
                  : null,
                include: {
                  CategoryImage: x,
                  LocalisedNames: x,
                  LocalisedDescriptions: x,
                  CatScope: x,
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

        const objects = loaded.collection<ProductCategory>(m.ProductCategory);
        this.table.total = (loaded.value('ProductCategories_total') ??
          0) as number;
        this.table.data = objects?.map((v) => {
          return {
            object: v,
            name: v.Name,
            primaryParent: v.PrimaryParent && v.PrimaryParent.DisplayName,
            secondaryParents: v.SecondaryParents?.map(
              (w) => w.DisplayName
            ).join(', '),
            scope: v.CatScope.Name,
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