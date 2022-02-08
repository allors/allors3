import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import {
  EditIncludeHandler,
  Node,
  CreateOrEditPullHandler,
  Pull,
  IPullResult,
  PostCreatePullHandler,
} from '@allors/system/workspace/domain';
import {
  BasePrice,
  InternalOrganisation,
  ProductCategory,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './productcategory-form.component.html',
  providers: [ContextService],
})
export class ProductCategoryFormComponent
  extends AllorsFormComponent<ProductCategory>
  implements CreateOrEditPullHandler, EditIncludeHandler, PostCreatePullHandler
{
  public m: M;
  public title: string;

  public category: ProductCategory;
  public locales: Locale[];
  public categories: ProductCategory[];
  public scopes: Scope[];
  public internalOrganisation: InternalOrganisation;

  private subscription: Subscription;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super(allors, errorService, form);
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest(
      this.refreshService.refresh$,
      this.internalOrganisationId.observable$
    )
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id == null;

          const pulls = [
            this.fetcher.locales,
            this.fetcher.internalOrganisation,
            pull.Scope({}),
            pull.ProductCategory({
              sorting: [{ roleType: m.ProductCategory.Name }],
            }),
          ];

          if (!isCreate) {
            pulls.push(
              pull.ProductCategory({
                objectId: this.data.id,
                include: {
                  CategoryImage: x,
                  Children: x,
                  LocalisedNames: {
                    Locale: x,
                  },
                  LocalisedDescriptions: {
                    Locale: x,
                  },
                },
              })
            );
          }

          return this.allors.context
            .pull(pulls)
            .pipe(map((loaded) => ({ loaded, isCreate })));
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.context.reset();

        this.internalOrganisation =
          this.fetcher.getInternalOrganisation(loaded);
        this.category = loaded.object(m.ProductCategory);
        this.categories = loaded.collection(m.ProductCategory);
        this.scopes = loaded.collection(m.Scope);
        this.locales = this.fetcher.getAdditionalLocales(loaded);

        if (isCreate) {
          this.title = 'Add Category';
          this.category = this.allors.context.create<ProductCategory>(
            m.ProductCategory
          );
          this.category.InternalOrganisation = this.internalOrganisation;
        } else {
          if (this.category.canWriteCatScope) {
            this.title = 'Edit Category';
          } else {
            this.title = 'View Category';
          }
        }
      });
  }
}
