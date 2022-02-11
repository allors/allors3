import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  InternalOrganisation,
  Locale,
  ProductCategory,
  Scope,
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
export class ProductCategoryFormComponent extends AllorsFormComponent<ProductCategory> {
  public m: M;
  public locales: Locale[];
  public categories: ProductCategory[];
  public scopes: Scope[];
  public internalOrganisation: InternalOrganisation;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }
  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      this.fetcher.locales,
      this.fetcher.internalOrganisation,
      p.Scope({}),
      p.ProductCategory({
        sorting: [{ roleType: m.ProductCategory.Name }],
      })
    );

    if (this.editRequest) {
      pulls.push(
        p.ProductCategory({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            CategoryImage: {},
            Children: {},
            LocalisedNames: {
              Locale: {},
            },
            LocalisedDescriptions: {
              Locale: {},
            },
          },
        })
      );
    }

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.onPostPullInitialize(pullResult);

    this.internalOrganisation =
      this.fetcher.getInternalOrganisation(pullResult);
    this.categories = pullResult.collection(this.m.ProductCategory);
    this.scopes = pullResult.collection(this.m.Scope);
    this.locales = this.fetcher.getAdditionalLocales(pullResult);
  }
}
