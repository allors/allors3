import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  Locale,
  NonUnifiedGood,
  ProductCategory,
  ProductIdentificationType,
  ProductNumber,
  Settings,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
  SearchFactory,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { Filters } from '../../../filters/filters';

@Component({
  templateUrl: './nonunifiedgood-create-form.component.html',
  providers: [ContextService],
})
export class NonUnifiedGoodCreateFormComponent extends AllorsFormComponent<NonUnifiedGood> {
  readonly m: M;

  locales: Locale[];
  categories: ProductCategory[];
  goodIdentificationTypes: ProductIdentificationType[];
  productNumber: ProductNumber;
  selectedCategories: ProductCategory[] = [];
  settings: Settings;
  goodNumberType: ProductIdentificationType;

  nonUnifiedPartsFilter: SearchFactory;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;

    this.nonUnifiedPartsFilter = Filters.nonUnifiedPartsFilter(this.m);
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      this.fetcher.locales,
      this.fetcher.Settings,
      p.ProductIdentificationType({}),
      p.ProductCategory({
        sorting: [{ roleType: m.ProductCategory.Name }],
      })
    );

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.context.create(this.createRequest.objectType);

    this.onPostPullInitialize(pullResult);

    this.categories = pullResult.collection<ProductCategory>(
      this.m.ProductCategory
    );
    this.goodIdentificationTypes =
      pullResult.collection<ProductIdentificationType>(
        this.m.ProductIdentificationType
      );
    this.locales = this.fetcher.getAdditionalLocales(pullResult);
    this.settings = this.fetcher.getSettings(pullResult);

    this.goodNumberType = this.goodIdentificationTypes?.find(
      (v) => v.UniqueId === 'b640630d-a556-4526-a2e5-60a84ab0db3f'
    );

    if (!this.settings.UseProductNumberCounter) {
      this.productNumber = this.allors.context.create<ProductNumber>(
        this.m.ProductNumber
      );
      this.productNumber.ProductIdentificationType = this.goodNumberType;

      this.object.addProductIdentification(this.productNumber);
    }
  }

  public override save(): void {
    this.selectedCategories.forEach((category: ProductCategory) => {
      category.addProduct(this.object);
    });

    super.save();
  }
}
