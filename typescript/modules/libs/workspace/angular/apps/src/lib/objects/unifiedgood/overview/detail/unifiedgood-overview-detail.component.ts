import { Component, OnInit, Self, OnDestroy } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Subscription, combineLatest, BehaviorSubject } from 'rxjs';
import { switchMap, filter } from 'rxjs/operators';
import { isBefore, isAfter } from 'date-fns';

import { M } from '@allors/workspace/meta/default';
import {
  Locale,
  Organisation,
  PriceComponent,
  SupplierOffering,
  ProductIdentificationType,
  Facility,
  InventoryItemKind,
  ProductType,
  Brand,
  Model,
  UnitOfMeasure,
  Settings,
  SupplierRelationship,
  ProductCategory,
  ProductNumber,
  UnifiedGood,
} from '@allors/workspace/domain/default';
import { NavigationService, PanelService, RefreshService, SaveService, TestScope } from '@allors/workspace/angular/base';
import { SessionService } from '@allors/workspace/angular/core';

import { FetcherService } from '../../../../services/fetcher/fetcher-service';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'unifiedgood-overview-detail',
  templateUrl: './unifiedgood-overview-detail.component.html',
  providers: [PanelService, SessionService],
})
export class UnifiedGoodOverviewDetailComponent extends TestScope implements OnInit, OnDestroy {
  readonly m: M;

  good: UnifiedGood;

  facility: Facility;
  facilities: Facility[];
  locales: Locale[];
  inventoryItemKinds: InventoryItemKind[];
  productTypes: ProductType[];
  categories: ProductCategory[];
  manufacturers: Organisation[];
  suppliers: Organisation[];
  currentSuppliers: Set<Organisation>;
  activeSuppliers: Organisation[];
  selectedSuppliers: Organisation[];
  supplierOfferings: SupplierOffering[];
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

  private subscription: Subscription;
  private refresh$: BehaviorSubject<Date>;

  constructor(
    @Self() public allors: SessionService,
    @Self() public panel: PanelService,
    public refreshService: RefreshService,
    public navigationService: NavigationService,
    private saveService: SaveService,
    private fetcher: FetcherService,
    private snackBar: MatSnackBar
  ) {
    super();

    this.m = this.allors.workspace.configuration.metaPopulation as M;
    this.refresh$ = new BehaviorSubject(new Date());

    panel.name = 'detail';
    panel.title = 'Good Details';
    panel.icon = 'person';
    panel.expandable = true;

    // Collapsed
    const pullName = `${this.panel.name}_${this.m.UnifiedGood.tag}`;

    panel.onPull = (pulls) => {
      this.good = undefined;

      if (this.panel.isCollapsed) {
        const m = this.m; const { pullBuilder: pull } = m;
        const id = this.panel.manager.id;

        pulls.push(
          pull.UnifiedGood({
            name: pullName,
            objectId: id,
          })
        );
      }
    };

    panel.onPulled = (loaded) => {
      if (this.panel.isCollapsed) {
        this.good = loaded.object<UnifiedGood>(pullName);
      }
    };
  }

  public ngOnInit(): void {
    const m = this.m;

    // Maximized
    this.subscription = combineLatest(this.refresh$, this.panel.manager.on$)
      .pipe(
        filter(() => {
          return this.panel.isExpanded;
        }),
        switchMap(() => {
          this.good = undefined;

          const m = this.m;
          const { pullBuilder: pull } = m;
          const x = {};
          const id = this.panel.manager.id;

          const pulls = [
            this.fetcher.locales,
            this.fetcher.Settings,
            pull.UnifiedGood({
              objectId: id,
              include: {
                PrimaryPhoto: x,
                Photos: x,
                PublicElectronicDocuments: x,
                PrivateElectronicDocuments: x,
                PublicLocalisedElectronicDocuments: x,
                PrivateLocalisedElectronicDocuments: x,
                ManufacturedBy: x,
                SuppliedBy: x,
                DefaultFacility: x,
                SerialisedItemCharacteristics: {
                  LocalisedValues: x,
                  SerialisedItemCharacteristicType: {
                    UnitOfMeasure: x,
                    LocalisedNames: x,
                  },
                },
                ProductIdentifications: {
                  ProductIdentificationType: x,
                },
                Brand: {
                  Models: x,
                },
                Model: x,
                LocalisedNames: {
                  Locale: x,
                },
                LocalisedDescriptions: {
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
            pull.UnifiedGood({
              objectId: id,
              select: {
                SupplierOfferingsWherePart: x,
              },
            }),
            pull.UnifiedGood({
              objectId: id,
              select: {
                PriceComponentsWherePart: x,
              },
            }),
            pull.UnitOfMeasure({}),
            pull.InventoryItemKind({}),
            pull.ProductIdentificationType({}),
            pull.Facility({}),
            pull.ProductIdentificationType({}),
            pull.ProductType({ sorting: [{ roleType: m.ProductType.Name }] }),
            pull.ProductCategory({ sorting: [{ roleType: m.ProductCategory.Name }] }),
            pull.SupplierRelationship({
              include: {
                Supplier: x,
              },
            }),
            pull.Brand({
              include: {
                Models: x,
              },
              sorting: [{ roleType: m.Brand.Name }],
            }),
            pull.Organisation({
              predicate: { kind: 'Equals', propertyType: m.Organisation.IsManufacturer, value: true },
            }),
            pull.UnifiedGood({
              name: 'OriginalCategories',
              objectId: id,
              select: {
                ProductCategoriesWhereProduct: {
                  include: {
                    Products: x,
                  },
                },
              },
            }),
          ];

          return this.allors.client.pullReactive(this.allors.session, pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.session.reset();

        this.good = loaded.object<UnifiedGood>(m.UnifiedGood);
        this.originalCategories = loaded.collection<ProductCategory>(m.ProductCategory);
        this.selectedCategories = this.originalCategories;

        this.inventoryItemKinds = loaded.collection<InventoryItemKind>(m.InventoryItemKind);
        this.productTypes = loaded.collection<ProductType>(m.ProductType);
        this.brands = loaded.collection<Brand>(m.Brand);
        this.locales = loaded.collection<Locale>(m.Locale);
        this.facilities = loaded.collection<Facility>(m.Facility);
        this.unitsOfMeasure = loaded.collection<UnitOfMeasure>(m.UnitOfMeasure);
        this.manufacturers = loaded.collection<Organisation>(m.Organisation);
        this.settings = loaded.object<Settings>(m.Settings);
        this.goodIdentificationTypes = loaded.collection<ProductIdentificationType>(m.ProductIdentificationType);
        this.locales = loaded.collection<Locale>(m.Locale);
        this.manufacturers = loaded.collection<Organisation>(m.Organisation);
        this.categories = loaded.collection<ProductCategory>(m.ProductCategory);

        const supplierRelationships = loaded.collection<SupplierRelationship>(m.SupplierRelationship);
        const currentsupplierRelationships = supplierRelationships.filter((v) => isBefore(new Date(v.FromDate), new Date()) && (v.ThroughDate === null || isAfter(new Date(v.ThroughDate), new Date())));
        this.currentSuppliers = new Set(currentsupplierRelationships.map((v) => v.Supplier).sort((a, b) => (a.Name > b.Name ? 1 : b.Name > a.Name ? -1 : 0)));

        const goodNumberType = this.goodIdentificationTypes.find((v) => v.UniqueId === 'b640630d-a556-4526-a2e5-60a84ab0db3f');

        this.productNumber = this.good.ProductIdentifications.find((v) => v.ProductIdentificationType === goodNumberType);

        this.suppliers = this.good.SuppliedBy as Organisation[];
        this.selectedSuppliers = this.suppliers;

        this.selectedBrand = this.good.Brand;
        this.selectedModel = this.good.Model;

        if (this.selectedBrand) {
          this.brandSelected(this.selectedBrand);
        }

        this.supplierOfferings = loaded.collection<SupplierOffering>(m.SupplierOffering);
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

    this.allors.client.pullReactive(this.allors.session, pulls).subscribe(() => {
      this.models = this.selectedBrand.Models.sort((a, b) => (a.Name > b.Name ? 1 : b.Name > a.Name ? -1 : 0));
    });
  }

  public save(): void {
    this.onSave();

    this.allors.client.pushReactive(this.allors.session).subscribe(() => {
      this.refreshService.refresh();
      this.panel.toggle();
    });
  }

  public update(): void {
    

    this.onSave();

    this.allors.client.pushReactive(this.allors.session).subscribe(() => {
      this.snackBar.open('Successfully saved.', 'close', { duration: 5000 });
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }

  private onSave() {
    this.selectedCategories.forEach((category: ProductCategory) => {
      category.addProduct(this.good);

      const index = this.originalCategories.indexOf(category);
      if (index > -1) {
        this.originalCategories.splice(index, 1);
      }
    });

    this.originalCategories.forEach((category: ProductCategory) => {
      category.removeProduct(this.good);
    });

    this.good.Brand = this.selectedBrand;
    this.good.Model = this.selectedModel;

    if (this.suppliers !== undefined) {
      const suppliersToDelete = this.suppliers.filter((v) => v);

      if (this.selectedSuppliers !== undefined) {
        this.selectedSuppliers.forEach((supplier: Organisation) => {
          const index = suppliersToDelete.indexOf(supplier);
          if (index > -1) {
            suppliersToDelete.splice(index, 1);
          }

          const supplierOffering = this.supplierOfferings.find((v) => v.Supplier === supplier && isBefore(new Date(v.FromDate), new Date()) && (v.ThroughDate === null || isAfter(new Date(v.ThroughDate), new Date())));

          if (supplierOffering === undefined) {
            this.supplierOfferings.push(this.newSupplierOffering(supplier));
          } else {
            supplierOffering.ThroughDate = null;
          }
        });
      }

      if (suppliersToDelete !== undefined) {
        suppliersToDelete.forEach((supplier: Organisation) => {
          const supplierOffering = this.supplierOfferings.find((v) => v.Supplier === supplier && isBefore(new Date(v.FromDate), new Date()) && (v.ThroughDate === null || isAfter(new Date(v.ThroughDate), new Date())));

          if (supplierOffering !== undefined) {
            supplierOffering.ThroughDate = new Date();
          }
        });
      }
    }
  }

  private newSupplierOffering(supplier: Organisation): SupplierOffering {
    const supplierOffering = this.allors.session.create<SupplierOffering>(this.m.SupplierOffering);
    supplierOffering.Supplier = supplier;
    supplierOffering.Part = this.good;
    supplierOffering.UnitOfMeasure = this.good.UnitOfMeasure;
    supplierOffering.Currency = this.settings.PreferredCurrency;
    return supplierOffering;
  }
}
