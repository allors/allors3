import { Component, OnDestroy, OnInit, Self, Optional, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { isBefore, isAfter } from 'date-fns';

import { M } from '@allors/workspace/meta/default';
import { Organisation, Part, ProductIdentificationType, Facility, InventoryItemKind, ProductType, Brand, Model, PartNumber, UnitOfMeasure, Settings, PartCategory, NonUnifiedPart, Locale } from '@allors/workspace/domain/default';
import { ObjectData, RefreshService, SaveService, TestScope, SearchFactory } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';

import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { Filters } from '../../../filters/filters';

@Component({
  templateUrl: './nonunifiedpart-create.component.html',
  providers: [ContextService],
})
export class NonUnifiedPartCreateComponent extends TestScope implements OnInit, OnDestroy {
  readonly m: M;

  public title = 'Add Part';

  part: Part;
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

  private subscription: Subscription;

  constructor(
    @Self() public allors: ContextService,
    @Optional() @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<NonUnifiedPartCreateComponent>,

    private refreshService: RefreshService,
    private saveService: SaveService,
    private snackBar: MatSnackBar,
    private fetcher: FetcherService
  ) {
    super();

    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest(this.refreshService.refresh$)
      .pipe(
        switchMap(() => {
          const pulls = [
            this.fetcher.locales,
            this.fetcher.Settings,
            this.fetcher.warehouses,
            pull.UnitOfMeasure({}),
            pull.InventoryItemKind({}),
            pull.ProductIdentificationType({}),
            pull.Ownership({ sorting: [{ roleType: m.Ownership.Name }] }),
            pull.PartCategory({
              sorting: [{ roleType: m.PartCategory.Name }],
            }),
            pull.ProductType({ sorting: [{ roleType: m.ProductType.Name }] }),
            pull.Brand({
              include: {
                Models: x,
              },
              sorting: [{ roleType: m.Brand.Name }],
            }),
          ];

          this.manufacturersFilter = Filters.manufacturersFilter(m);

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        this.inventoryItemKinds = loaded.collection<InventoryItemKind>(m.InventoryItemKind);
        this.productTypes = loaded.collection<ProductType>(m.ProductType);
        this.brands = loaded.collection<Brand>(m.Brand);
        this.locales = this.fetcher.getAdditionalLocales(loaded);
        this.facilities = this.fetcher.getWarehouses(loaded);
        this.categories = loaded.collection<PartCategory>(m.PartCategory);
        this.settings = this.fetcher.getSettings(loaded);

        this.unitsOfMeasure = loaded.collection<UnitOfMeasure>(m.UnitOfMeasure);
        const piece = this.unitsOfMeasure?.find((v) => v.UniqueId === 'f4bbdb52-3441-4768-92d4-729c6c5d6f1b');

        this.goodIdentificationTypes = loaded.collection<ProductIdentificationType>(m.ProductIdentificationType);
        const partNumberType = this.goodIdentificationTypes?.find((v) => v.UniqueId === '5735191a-cdc4-4563-96ef-dddc7b969ca6');

        this.part = this.allors.context.create<NonUnifiedPart>(m.NonUnifiedPart);
        this.part.DefaultFacility = this.settings.DefaultFacility;
        this.part.UnitOfMeasure = piece;

        if (!this.settings.UsePartNumberCounter) {
          this.partNumber = this.allors.context.create<PartNumber>(m.PartNumber);
          this.partNumber.ProductIdentificationType = partNumberType;

          this.part.addProductIdentification(this.partNumber);
        }
      });
  }

  public brandAdded(brand: Brand): void {
    this.brands.push(brand);
    this.selectedBrand = brand;
    this.models = [];
    this.selectedModel = undefined;
  }

  public modelAdded(model: Model): void {
    this.selectedBrand.addModel(model);
    this.models = this.selectedBrand.Models.sort((a, b) => (a.Name > b.Name ? 1 : b.Name > a.Name ? -1 : 0));
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
      this.models = this.selectedBrand.Models ? this.selectedBrand.Models.sort((a, b) => (a.Name > b.Name ? 1 : b.Name > a.Name ? -1 : 0)) : [];
    });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.onSave();

    this.allors.context.push().subscribe(() => {
      this.dialogRef.close(this.part);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }

  public update(): void {
    this.onSave();

    this.allors.context.push().subscribe(() => {
      this.snackBar.open('Successfully saved.', 'close', { duration: 5000 });
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }

  private onSave() {
    this.selectedCategories.forEach((category: PartCategory) => {
      category.addPart(this.part);
    });

    this.part.Brand = this.selectedBrand;
    this.part.Model = this.selectedModel;
  }
}