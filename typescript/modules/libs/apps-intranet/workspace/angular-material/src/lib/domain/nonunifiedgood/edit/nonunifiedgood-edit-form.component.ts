import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  Locale,
  NonUnifiedGood,
  ProductCategory,
  ProductDimension,
  ProductFeatureApplicability,
  ProductIdentificationType,
  ProductNumber,
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
  selector: 'nonunifiedgood-edit-form',
  templateUrl: './nonunifiedgood-edit-form.component.html',
  providers: [ContextService],
})
export class NonUnifiedGoodEditFormComponent extends AllorsFormComponent<NonUnifiedGood> {
  readonly m: M;
  locales: Locale[];
  categories: ProductCategory[];
  goodIdentificationTypes: ProductIdentificationType[];
  productNumber: ProductNumber;
  originalCategories: ProductCategory[] = [];
  selectedCategories: ProductCategory[] = [];
  productFeatureApplicabilities: ProductFeatureApplicability[];
  productDimensions: ProductDimension[];

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
      p.ProductIdentificationType({}),
      p.ProductCategory({
        include: { Products: {} },
        sorting: [{ roleType: m.ProductCategory.Name }],
      }),
      p.NonUnifiedGood({
        name: '_object',
        objectId: this.editRequest.objectId,
        include: {
          Part: {
            Brand: {},
            Model: {},
          },
          PrimaryPhoto: {},
          ProductIdentifications: {},
          Photos: {},
          PublicElectronicDocuments: {},
          PrivateElectronicDocuments: {},
          PublicLocalisedElectronicDocuments: {},
          PrivateLocalisedElectronicDocuments: {},
          LocalisedNames: {
            Locale: {},
          },
          LocalisedDescriptions: {
            Locale: {},
          },
          LocalisedComments: {
            Locale: {},
          },
          LocalisedKeywords: {
            Locale: {},
          },
        },
      }),
      p.NonUnifiedGood({
        name: 'OriginalCategories',
        objectId: this.editRequest.objectId,
        select: { ProductCategoriesWhereProduct: {} },
      }),
      p.NonUnifiedGood({
        objectId: this.editRequest.objectId,
        select: {
          ProductFeatureApplicabilitiesWhereAvailableFor: {
            include: {
              ProductFeature: {
                ProductDimension_Dimension: {
                  UnitOfMeasure: {},
                },
              },
            },
          },
        },
      })
    );

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.onPostPullInitialize(pullResult);

    this.originalCategories =
      pullResult.collection<ProductCategory>('OriginalCategories');
    this.selectedCategories = this.originalCategories;

    this.categories = pullResult.collection<ProductCategory>(
      this.m.ProductCategory
    );

    this.goodIdentificationTypes =
      pullResult.collection<ProductIdentificationType>(
        this.m.ProductIdentificationType
      );

    this.locales = this.fetcher.getAdditionalLocales(pullResult);

    this.productFeatureApplicabilities =
      pullResult.collection<ProductFeatureApplicability>(
        this.m.NonUnifiedGood.ProductFeatureApplicabilitiesWhereAvailableFor
      );

    this.productDimensions = this.productFeatureApplicabilities
      ?.map((v) => v.ProductFeature)
      .filter(
        (v) => v.strategy.cls === this.m.ProductDimension
      ) as ProductDimension[];

    const goodNumberType = this.goodIdentificationTypes?.find(
      (v) => v.UniqueId === 'b640630d-a556-4526-a2e5-60a84ab0db3f'
    );

    this.productNumber = this.object.ProductIdentifications?.find(
      (v) => v.ProductIdentificationType === goodNumberType
    );
  }

  public override save(): void {
    this.selectedCategories.forEach((category: ProductCategory) => {
      category.addProduct(this.object);

      const index = this.originalCategories.indexOf(category);
      if (index > -1) {
        this.originalCategories.splice(index, 1);
      }
    });

    this.originalCategories.forEach((category: ProductCategory) => {
      category.removeProduct(this.object);
    });

    super.save();
  }
}
