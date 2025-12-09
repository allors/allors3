import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  Brand,
  Facility,
  InventoryItemKind,
  Locale,
  Model,
  NonUnifiedPart,
  PartCategory,
  PartNumber,
  PriceComponent,
  ProductIdentificationType,
  ProductType,
  Settings,
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
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'nonunifiedpart-edit-form',
  templateUrl: './nonunifiedpart-edit-form.component.html',
  providers: [ContextService],
})
export class NonUnifiedPartEditFormComponent extends AllorsFormComponent<NonUnifiedPart> {
  readonly m: M;

  facilities: Facility[];
  locales: Locale[];
  inventoryItemKinds: InventoryItemKind[];
  productTypes: ProductType[];
  brands: Brand[];
  selectedBrand: Brand;
  models: Model[];
  selectedModel: Model;
  addBrand = false;
  addModel = false;
  goodIdentificationTypes: ProductIdentificationType[];
  partNumber: PartNumber;
  unitsOfMeasure: UnitOfMeasure[];
  currentSellingPrice: PriceComponent;
  settings: Settings;
  categories: PartCategory[];
  originalCategories: PartCategory[] = [];
  selectedCategories: PartCategory[] = [];
  manufacturersFilter: SearchFactory;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private snackBar: MatSnackBar,
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
      this.fetcher.warehouses,
      p.Part({
        name: '_object',
        objectId: this.editRequest.objectId,
        include: {
          PrimaryPhoto: {},
          Photos: {},
          Documents: {},
          PublicElectronicDocuments: {},
          PrivateElectronicDocuments: {},
          PublicLocalisedElectronicDocuments: {},
          PrivateLocalisedElectronicDocuments: {},
          ManufacturedBy: {},
          SuppliedBy: {},
          DefaultFacility: {},
          PartWeightedAverage: {},
          SerialisedItemCharacteristics: {
            LocalisedValues: {},
            SerialisedItemCharacteristicType: {
              UnitOfMeasure: {},
              LocalisedNames: {},
            },
          },
          Brand: {
            Models: {},
          },
          ProductIdentifications: {
            ProductIdentificationType: {},
          },
          LocalisedNames: {
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
      p.Ownership({ sorting: [{ roleType: m.Ownership.Name }] }),
      p.ProductType({ sorting: [{ roleType: m.ProductType.Name }] }),
      p.PartCategory({
        sorting: [{ roleType: m.PartCategory.Name }],
      }),
      p.Brand({
        include: {
          Models: {},
        },
        sorting: [{ roleType: m.Brand.Name }],
      }),
      p.NonUnifiedPart({
        name: 'OriginalCategories',
        objectId: this.editRequest.objectId,
        select: { PartCategoriesWherePart: {} },
      })
    );

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    this.object = pullResult.object('_object');

    this.onPostPullInitialize(pullResult);

    this.originalCategories =
      pullResult.collection<PartCategory>('OriginalCategories') ?? [];
    this.selectedCategories = this.originalCategories;

    this.inventoryItemKinds = pullResult.collection<InventoryItemKind>(
      this.m.InventoryItemKind
    );

    this.productTypes = pullResult.collection<ProductType>(this.m.ProductType);
    this.brands = pullResult.collection<Brand>(this.m.Brand);
    this.locales = this.fetcher.getAdditionalLocales(pullResult);
    this.facilities = this.fetcher.getWarehouses(pullResult);
    this.unitsOfMeasure = pullResult.collection<UnitOfMeasure>(
      this.m.UnitOfMeasure
    );
    this.categories = pullResult.collection<PartCategory>(this.m.PartCategory);
    this.settings = this.fetcher.getSettings(pullResult);

    this.goodIdentificationTypes =
      pullResult.collection<ProductIdentificationType>(
        this.m.ProductIdentificationType
      );
    const partNumberType = this.goodIdentificationTypes?.find(
      (v) => v.UniqueId === '5735191a-cdc4-4563-96ef-dddc7b969ca6'
    );

    this.partNumber = this.object.ProductIdentifications?.find(
      (v) => v.ProductIdentificationType === partNumberType
    );

    this.selectedBrand = this.object.Brand;
    this.selectedModel = this.object.Model;

    if (this.selectedBrand) {
      this.brandSelected(this.selectedBrand);
    }

    this.categorySelected(this.selectedCategories);
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
      this.models = this.selectedBrand?.Models.sort((a, b) =>
        a.Name > b.Name ? 1 : b.Name > a.Name ? -1 : 0
      );
    });
  }

  public categorySelected(categories: PartCategory[]): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    let pulls = [];

    categories.forEach((category: PartCategory) => {
      pulls = [
        ...pulls,
        pull.PartCategory({
          object: category,
          include: {
            Parts: x,
          },
        }),
      ];
    });

    this.allors.context.pull(pulls);
  }

  public override save(): void {
    this.onSave();

    super.save();
  }

  private onSave() {
    this.selectedCategories.forEach((category: PartCategory) => {
      category.addPart(this.object);

      const index = this.originalCategories.indexOf(category);
      if (index > -1) {
        this.originalCategories.splice(index, 1);
      }
    });

    this.originalCategories.forEach((category: PartCategory) => {
      category.removePart(this.object);
    });

    this.object.Brand = this.selectedBrand;
    this.object.Model = this.selectedModel;
  }
}
