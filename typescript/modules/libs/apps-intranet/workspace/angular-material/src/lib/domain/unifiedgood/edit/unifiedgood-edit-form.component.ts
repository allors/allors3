import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  Brand,
  Facility,
  InventoryItemKind,
  Locale,
  Model,
  Organisation,
  PriceComponent,
  ProductCategory,
  ProductIdentificationType,
  ProductNumber,
  ProductType,
  Settings,
  UnifiedGood,
  UnitOfMeasure,
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
  selector: 'unifiedgood-edit-form',
  templateUrl: './unifiedgood-edit-form.component.html',
  providers: [ContextService],
})
export class UnifiedGoodEditFormComponent extends AllorsFormComponent<UnifiedGood> {
  readonly m: M;

  facility: Facility;
  facilities: Facility[];
  locales: Locale[];
  inventoryItemKinds: InventoryItemKind[];
  productTypes: ProductType[];
  categories: ProductCategory[];
  suppliers: Organisation[];
  brands: Brand[];
  selectedBrand: Brand;
  models: Model[];
  selectedModel: Model;
  organisations: Organisation[];
  addBrand = false;
  addModel = false;
  goodIdentificationTypes: ProductIdentificationType[];
  productNumber: ProductNumber;
  originalCategories: ProductCategory[] = [];
  selectedCategories: ProductCategory[] = [];
  unitsOfMeasure: UnitOfMeasure[];
  currentSellingPrice: PriceComponent;
  internalOrganisation: Organisation;
  settings: Settings;
  manufacturersFilter: SearchFactory;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;

    this.manufacturersFilter = Filters.manufacturersFilter(this.m);
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      this.fetcher.locales,
      this.fetcher.Settings,
      p.UnifiedGood({
        objectId: this.editRequest.objectId,
        include: {
          PrimaryPhoto: {},
          Photos: {},
          PublicElectronicDocuments: {},
          PrivateElectronicDocuments: {},
          PublicLocalisedElectronicDocuments: {},
          PrivateLocalisedElectronicDocuments: {},
          ManufacturedBy: {},
          SuppliedBy: {},
          DefaultFacility: {},
          SerialisedItemCharacteristics: {
            LocalisedValues: {},
            SerialisedItemCharacteristicType: {
              UnitOfMeasure: {},
              LocalisedNames: {},
            },
          },
          ProductIdentifications: {
            ProductIdentificationType: {},
          },
          Brand: {
            Models: {},
          },
          Model: {},
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
      p.UnitOfMeasure({}),
      p.InventoryItemKind({}),
      p.ProductIdentificationType({}),
      p.Facility({}),
      p.ProductIdentificationType({}),
      p.ProductType({ sorting: [{ roleType: m.ProductType.Name }] }),
      p.ProductCategory({
        sorting: [{ roleType: m.ProductCategory.Name }],
      }),
      p.Brand({
        include: {
          Models: {},
        },
        sorting: [{ roleType: m.Brand.Name }],
      }),
      p.UnifiedGood({
        name: 'OriginalCategories',
        objectId: this.editRequest.objectId,
        select: {
          ProductCategoriesWhereProduct: {
            include: {
              Products: {},
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
      pullResult.collection<ProductCategory>('OriginalCategories') ?? [];
    this.selectedCategories = this.originalCategories;

    this.inventoryItemKinds = pullResult.collection<InventoryItemKind>(
      this.m.InventoryItemKind
    );
    this.productTypes = pullResult.collection<ProductType>(this.m.ProductType);
    this.brands = pullResult.collection<Brand>(this.m.Brand);
    this.locales = this.fetcher.getAdditionalLocales(pullResult);
    this.facilities = pullResult.collection<Facility>(this.m.Facility);
    this.unitsOfMeasure = pullResult.collection<UnitOfMeasure>(
      this.m.UnitOfMeasure
    );
    this.settings = this.fetcher.getSettings(pullResult);
    this.goodIdentificationTypes =
      pullResult.collection<ProductIdentificationType>(
        this.m.ProductIdentificationType
      );
    this.categories = pullResult.collection<ProductCategory>(
      this.m.ProductCategory
    );

    const goodNumberType = this.goodIdentificationTypes?.find(
      (v) => v.UniqueId === 'b640630d-a556-4526-a2e5-60a84ab0db3f'
    );

    this.productNumber = this.object.ProductIdentifications?.find(
      (v) => v.ProductIdentificationType === goodNumberType
    );

    this.suppliers = this.object.SuppliedBy as Organisation[];

    this.selectedBrand = this.object.Brand;
    this.selectedModel = this.object.Model;

    if (this.selectedBrand) {
      this.brandSelected(this.selectedBrand);
    }
  }

  public brandAdded(brand: Brand): void {
    this.brands.push(brand);
    this.selectedBrand = brand;
    this.models = [];
    this.selectedModel = undefined;
  }

  public modelAdded(model: Model): void {
    this.selectedBrand.addModel(model);
    this.models = this.selectedBrand.Models.sort((a, b) =>
      a.Name > b.Name ? 1 : b.Name > a.Name ? -1 : 0
    );
    this.selectedModel = model;
  }

  public brandSelected(brand: Brand): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const pulls = [
      pull.Brand({
        object: brand,
        include: {
          Models: x,
        },
      }),
    ];

    this.allors.context.pull(pulls).subscribe(() => {
      this.models = this.selectedBrand.Models.sort((a, b) =>
        a.Name > b.Name ? 1 : b.Name > a.Name ? -1 : 0
      );
    });
  }

  public override save(): void {
    this.onSave();

    super.save();
  }

  private onSave() {
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

    this.object.Brand = this.selectedBrand;
    this.object.Model = this.selectedModel;
  }
}
