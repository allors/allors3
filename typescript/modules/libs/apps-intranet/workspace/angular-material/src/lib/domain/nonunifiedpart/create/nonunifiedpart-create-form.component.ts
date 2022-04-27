import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  Brand,
  Facility,
  Locale,
  InternalOrganisation,
  InventoryItemKind,
  NonUnifiedPart,
  ProductType,
  Model,
  Organisation,
  ProductIdentificationType,
  PartNumber,
  UnitOfMeasure,
  Settings,
  PartCategory,
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
import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './nonunifiedpart-create-form.component.html',
  providers: [ContextService],
})
export class NonUnifiedPartCreateFormComponent extends AllorsFormComponent<NonUnifiedPart> {
  readonly m: M;
  facility: Facility;
  locales: Locale[];
  inventoryItemKinds: InventoryItemKind[];
  productTypes: ProductType[];
  brands: Brand[];
  selectedBrand: Brand;
  models: Model[];
  selectedModel: Model;
  organisations: Organisation[];
  addBrand = false;
  addModel = false;
  goodIdentificationTypes: ProductIdentificationType[];
  partNumber: PartNumber;
  facilities: Facility[];
  unitsOfMeasure: UnitOfMeasure[];
  settings: Settings;
  categories: PartCategory[];
  selectedCategories: PartCategory[] = [];
  manufacturersFilter: SearchFactory;
  internalOrganisation: any;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    public internalOrganisationId: InternalOrganisationId,
    private snackBar: MatSnackBar,
    private fetcher: FetcherService,
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
      p.UnitOfMeasure({}),
      p.InventoryItemKind({}),
      p.ProductIdentificationType({}),
      p.Ownership({ sorting: [{ roleType: m.Ownership.Name }] }),
      p.PartCategory({
        sorting: [{ roleType: m.PartCategory.Name }],
      }),
      p.ProductType({ sorting: [{ roleType: m.ProductType.Name }] }),
      p.Brand({
        include: {
          Models: {},
        },
        sorting: [{ roleType: m.Brand.Name }],
      }),
      p.InternalOrganisation({
        objectId: this.internalOrganisationId.value,
        include: { FacilitiesWhereOwner: {} },
      }),
    );

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.context.create(this.createRequest.objectType);

    this.onPostPullInitialize(pullResult);

    this.internalOrganisation = pullResult.object<InternalOrganisation>(
      this.m.InternalOrganisation
    );
    this.inventoryItemKinds = pullResult.collection<InventoryItemKind>(
      this.m.InventoryItemKind
    );
    this.productTypes = pullResult.collection<ProductType>(this.m.ProductType);
    this.brands = pullResult.collection<Brand>(this.m.Brand);
    this.locales = this.fetcher.getAdditionalLocales(pullResult);
    this.facilities = this.fetcher.getWarehouses(pullResult);
    this.categories = pullResult.collection<PartCategory>(this.m.PartCategory);
    this.settings = this.fetcher.getSettings(pullResult);

    this.unitsOfMeasure = pullResult.collection<UnitOfMeasure>(
      this.m.UnitOfMeasure
    );
    const piece = this.unitsOfMeasure?.find(
      (v) => v.UniqueId === 'f4bbdb52-3441-4768-92d4-729c6c5d6f1b'
    );

    this.goodIdentificationTypes =
      pullResult.collection<ProductIdentificationType>(
        this.m.ProductIdentificationType
      );
    const partNumberType = this.goodIdentificationTypes?.find(
      (v) => v.UniqueId === '5735191a-cdc4-4563-96ef-dddc7b969ca6'
    );

    this.object.DefaultFacility = this.internalOrganisation.FacilitiesWhereOwner[0];
    this.object.UnitOfMeasure = piece;

    if (!this.settings.UsePartNumberCounter) {
      this.partNumber = this.allors.context.create<PartNumber>(
        this.m.PartNumber
      );
      this.partNumber.ProductIdentificationType = partNumberType;

      this.object.addProductIdentification(this.partNumber);
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
      this.models = this.selectedBrand.Models
        ? this.selectedBrand.Models.sort((a, b) =>
            a.Name > b.Name ? 1 : b.Name > a.Name ? -1 : 0
          )
        : [];
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
    });

    this.object.Brand = this.selectedBrand;
    this.object.Model = this.selectedModel;
  }
}
