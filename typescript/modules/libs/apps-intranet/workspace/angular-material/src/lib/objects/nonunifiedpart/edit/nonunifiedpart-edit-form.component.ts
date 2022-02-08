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
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { FetcherService } from '../../../../services/fetcher/fetcher-service';
import { Filters } from '../../../../filters/filters';

@Component({
  selector: 'nonunifiedpart-edit-form',
  templateUrl: './nonunifiedpart-edit-form.component.html',
  providers: [OldPanelService, ContextService],
})
export class NonUnifiedPartEditFormComponent implements OnInit, OnDestroy {
  readonly m: M;

  part: Part;

  facility: Facility;
  facilities: Facility[];
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
  unitsOfMeasure: UnitOfMeasure[];
  currentSellingPrice: PriceComponent;
  internalOrganisation: Organisation;
  settings: Settings;
  categories: PartCategory[];
  originalCategories: PartCategory[] = [];
  selectedCategories: PartCategory[] = [];
  manufacturersFilter: SearchFactory;

  private subscription: Subscription;

  constructor(
    @Self() public allors: ContextService,
    @Self() public panel: OldPanelService,
    public refreshService: RefreshService,
    public navigationService: NavigationService,
    private errorService: ErrorService,
    private snackBar: MatSnackBar,
    private fetcher: FetcherService
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;

    panel.name = 'detail';
    panel.title = 'Part Details';
    panel.icon = 'business';
    panel.expandable = true;

    // Collapsed
    const pullName = `${this.panel.name}_${this.m.Part.tag}`;

    panel.onPull = (pulls) => {
      this.part = undefined;
      if (this.panel.isCollapsed) {
        const m = this.m;
        const { pullBuilder: pull } = m;
        const id = this.panel.manager.id;

        pulls.push(
          pull.Part({
            name: pullName,
            objectId: id,
          })
        );
      }
    };

    panel.onPulled = (loaded) => {
      if (this.panel.isCollapsed) {
        this.part = loaded.object<Part>(pullName);
      }
    };
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    // Maximized
    this.subscription = combineLatest(
      this.refreshService.refresh$,
      this.panel.manager.on$
    )
      .pipe(
        filter(() => {
          return this.panel.isExpanded;
        }),
        switchMap(() => {
          this.part = undefined;

          const id = this.panel.manager.id;

          const pulls = [
            this.fetcher.locales,
            this.fetcher.Settings,
            this.fetcher.warehouses,
            pull.Part({
              objectId: id,
              include: {
                PrimaryPhoto: x,
                Photos: x,
                Documents: x,
                PublicElectronicDocuments: x,
                PrivateElectronicDocuments: x,
                PublicLocalisedElectronicDocuments: x,
                PrivateLocalisedElectronicDocuments: x,
                ManufacturedBy: x,
                SuppliedBy: x,
                DefaultFacility: x,
                PartWeightedAverage: x,
                SerialisedItemCharacteristics: {
                  LocalisedValues: x,
                  SerialisedItemCharacteristicType: {
                    UnitOfMeasure: x,
                    LocalisedNames: x,
                  },
                },
                Brand: {
                  Models: x,
                },
                ProductIdentifications: {
                  ProductIdentificationType: x,
                },
                LocalisedNames: {
                  Locale: x,
                },
                LocalisedComments: {
                  Locale: x,
                },
                LocalisedKeywords: {
                  Locale: x,
                },
              },
            }),
            pull.UnitOfMeasure({}),
            pull.InventoryItemKind({}),
            pull.ProductIdentificationType({}),
            pull.Ownership({ sorting: [{ roleType: m.Ownership.Name }] }),
            pull.ProductType({ sorting: [{ roleType: m.ProductType.Name }] }),
            pull.PartCategory({
              sorting: [{ roleType: m.PartCategory.Name }],
            }),
            pull.Brand({
              include: {
                Models: x,
              },
              sorting: [{ roleType: m.Brand.Name }],
            }),
            pull.NonUnifiedPart({
              name: 'OriginalCategories',
              objectId: id,
              select: { PartCategoriesWherePart: x },
            }),
          ];

          this.manufacturersFilter = Filters.manufacturersFilter(m);

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        this.part = loaded.object<Part>(m.Part);
        this.originalCategories =
          loaded.collection<PartCategory>('OriginalCategories') ?? [];
        this.selectedCategories = this.originalCategories;

        this.inventoryItemKinds = loaded.collection<InventoryItemKind>(
          m.InventoryItemKind
        );
        this.productTypes = loaded.collection<ProductType>(m.ProductType);
        this.brands = loaded.collection<Brand>(m.Brand);
        this.locales = this.fetcher.getAdditionalLocales(loaded);
        this.facilities = this.fetcher.getWarehouses(loaded);
        this.unitsOfMeasure = loaded.collection<UnitOfMeasure>(m.UnitOfMeasure);
        this.categories = loaded.collection<PartCategory>(m.PartCategory);
        this.settings = this.fetcher.getSettings(loaded);

        this.goodIdentificationTypes =
          loaded.collection<ProductIdentificationType>(
            m.ProductIdentificationType
          );
        const partNumberType = this.goodIdentificationTypes?.find(
          (v) => v.UniqueId === '5735191a-cdc4-4563-96ef-dddc7b969ca6'
        );

        this.partNumber = this.part.ProductIdentifications?.find(
          (v) => v.ProductIdentificationType === partNumberType
        );

        this.selectedBrand = this.part.Brand;
        this.selectedModel = this.part.Model;

        if (this.selectedBrand) {
          this.brandSelected(this.selectedBrand);
        }

        this.categorySelected(this.selectedCategories);
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
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

  public save(): void {
    this.onSave();

    this.allors.context.push().subscribe(() => {
      this.panel.toggle();
    }, this.errorService.errorHandler);
  }

  public update(): void {
    this.onSave();

    this.allors.context.push().subscribe(() => {
      this.snackBar.open('Successfully saved.', 'close', { duration: 5000 });
      this.refreshService.refresh();
    }, this.errorService.errorHandler);
  }

  private onSave() {
    this.selectedCategories.forEach((category: PartCategory) => {
      category.addPart(this.part);

      const index = this.originalCategories.indexOf(category);
      if (index > -1) {
        this.originalCategories.splice(index, 1);
      }
    });

    this.originalCategories.forEach((category: PartCategory) => {
      category.removePart(this.part);
    });

    this.part.Brand = this.selectedBrand;
    this.part.Model = this.selectedModel;
  }
}
