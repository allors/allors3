import { Component, OnInit, Self, OnDestroy } from '@angular/core';
import { Subscription, combineLatest, BehaviorSubject } from 'rxjs';
import { switchMap, filter } from 'rxjs/operators';

import { NavigationService, PanelService, RefreshService, SaveService, SearchFactory, TestScope } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';
import { Brand, Model, NonUnifiedGood, Organisation, Ownership, ProductCategory, ProductDimension, ProductFeatureApplicability, ProductIdentificationType, ProductNumber, ProductType, Locale } from '@allors/workspace/domain/default';
import { M } from '@allors/workspace/meta/default';
import { FetcherService } from '../../../../services/fetcher/fetcher-service';
import { Filters } from '../../../../filters/filters';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'nonunifiedgood-overview-detail',
  templateUrl: './nonunifiedgood-overview-detail.component.html',
  providers: [PanelService, ContextService],
})
export class NonUnifiedGoodOverviewDetailComponent extends TestScope implements OnInit, OnDestroy {
  readonly m: M;

  good: NonUnifiedGood;

  locales: Locale[];
  categories: ProductCategory[];
  productTypes: ProductType[];
  manufacturers: Organisation[];
  brands: Brand[];
  selectedBrand: Brand;
  models: Model[];
  selectedModel: Model;
  ownerships: Ownership[];
  organisations: Organisation[];
  addBrand = false;
  addModel = false;
  goodIdentificationTypes: ProductIdentificationType[];
  productNumber: ProductNumber;
  originalCategories: ProductCategory[] = [];
  selectedCategories: ProductCategory[] = [];
  productFeatureApplicabilities: ProductFeatureApplicability[];
  productDimensions: ProductDimension[];

  private subscription: Subscription;
  private refresh$: BehaviorSubject<Date>;

  nonUnifiedPartsFilter: SearchFactory;

  constructor(
    @Self() public allors: ContextService,
    @Self() public panel: PanelService,
    public refreshService: RefreshService,
    public navigationService: NavigationService,
    private saveService: SaveService,
    private fetcher: FetcherService
  ) {
    super();

    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;
    this.refresh$ = new BehaviorSubject(new Date());

    panel.name = 'detail';
    panel.title = 'Good Details';
    panel.icon = 'person';
    panel.expandable = true;

    // Collapsed
    const pullName = `${this.panel.name}_${this.m.Good.tag}`;

    panel.onPull = (pulls) => {
      this.good = undefined;

      if (this.panel.isCollapsed) {
        const m = this.m;
        const { pullBuilder: pull } = m;
        const x = {};
        const id = this.panel.manager.id;

        pulls.push(
          pull.NonUnifiedGood({
            name: pullName,
            objectId: id,
            include: {
              ProductIdentifications: {
                ProductIdentificationType: x,
              },
              Part: {
                Brand: x,
                Model: x,
              },
            },
          }),
          pull.ProductCategory({ sorting: [{ roleType: m.ProductCategory.Name }] })
        );
      }
    };

    panel.onPulled = (loaded) => {
      if (this.panel.isCollapsed) {
        this.good = loaded.object<NonUnifiedGood>(pullName);
      }
    };
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    // Maximized
    this.subscription = combineLatest([this.refresh$, this.panel.manager.on$])
      .pipe(
        filter(() => {
          return this.panel.isExpanded;
        }),
        switchMap(() => {
          this.good = undefined;

          const id = this.panel.manager.id;

          const pulls = [
            this.fetcher.locales,
            pull.ProductIdentificationType({}),
            pull.ProductCategory({
              include: { Products: x },
              sorting: [{ roleType: m.ProductCategory.Name }],
            }),
            pull.NonUnifiedGood({
              objectId: id,
              include: {
                Part: {
                  Brand: x,
                  Model: x,
                },
                PrimaryPhoto: x,
                ProductIdentifications: x,
                Photos: x,
                PublicElectronicDocuments: x,
                PrivateElectronicDocuments: x,
                PublicLocalisedElectronicDocuments: x,
                PrivateLocalisedElectronicDocuments: x,
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
            pull.NonUnifiedGood({
              name: 'OriginalCategories',
              objectId: id,
              select: { ProductCategoriesWhereProduct: x },
            }),
            pull.NonUnifiedGood({
              objectId: id,
              select: {
                ProductFeatureApplicabilitiesWhereAvailableFor: {
                  include: {
                    ProductFeature: {
                      ProductDimension_Dimension: {
                        UnitOfMeasure: x,
                      },
                    },
                  },
                },
              },
            }),
          ];

          this.nonUnifiedPartsFilter = Filters.nonUnifiedPartsFilter(m);

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        this.good = loaded.object<NonUnifiedGood>(m.NonUnifiedGood);
        this.originalCategories = loaded.collection<ProductCategory>('OriginalCategories');
        this.selectedCategories = this.originalCategories;

        this.categories = loaded.collection<ProductCategory>(m.ProductCategory);
        this.goodIdentificationTypes = loaded.collection<ProductIdentificationType>(m.ProductIdentificationType);
        this.locales = this.fetcher.getAdditionalLocales(loaded);
        this.productFeatureApplicabilities = loaded.collection<ProductFeatureApplicability>(m.NonUnifiedGood.ProductFeatureApplicabilitiesWhereAvailableFor);
        this.productDimensions = this.productFeatureApplicabilities?.map((v) => v.ProductFeature).filter((v) => v.strategy.cls === this.m.ProductDimension) as ProductDimension[];

        const goodNumberType = this.goodIdentificationTypes?.find((v) => v.UniqueId === 'b640630d-a556-4526-a2e5-60a84ab0db3f');

        this.productNumber = this.good.ProductIdentifications?.find((v) => v.ProductIdentificationType === goodNumberType);
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
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

    this.allors.context.push().subscribe(() => {
      this.refreshService.refresh();
      this.panel.toggle();
    }, this.saveService.errorHandler);
  }
}
