import { Component, OnInit, Self, OnDestroy } from '@angular/core';
import { Subscription, combineLatest, BehaviorSubject } from 'rxjs';
import { switchMap, filter } from 'rxjs/operators';

import { MetaService, RefreshService, NavigationService, PanelService, SessionService } from '@allors/angular/services/core';
import {
  Organisation,
  ProductType,
  ProductCategory,
  Brand,
  Model,
  ProductIdentificationType,
  ProductNumber,
  NonUnifiedGood,
  Ownership,
  ProductFeatureApplicability,
  ProductDimension,
  Locale,
} from '@allors/domain/generated';
import { SaveService } from '@allors/angular/material/services/core';
import { Meta } from '@allors/meta/generated';
import { FetcherService, Filters } from '@allors/angular/base';
import { PullRequest } from '@allors/protocol/system';
import { Sort } from '@allors/data/system';
import { TestScope, SearchFactory } from '@allors/angular/core';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'nonunifiedgood-overview-detail',
  templateUrl: './nonunifiedgood-overview-detail.component.html',
  providers: [PanelService, SessionService],
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
    @Self() public allors: SessionService,
    @Self() public panel: PanelService,
    
    public refreshService: RefreshService,
    public navigationService: NavigationService,
    private saveService: SaveService,
    private fetcher: FetcherService
  ) {
    super();

    this.m = this.allors.workspace.configuration.metaPopulation as M;
    this.refresh$ = new BehaviorSubject(new Date());

    panel.name = 'detail';
    panel.title = 'Good Details';
    panel.icon = 'person';
    panel.expandable = true;

    // Collapsed
    const pullName = `${this.panel.name}_${this.m.Good.name}`;

    panel.onPull = (pulls) => {
      this.good = undefined;

      if (this.panel.isCollapsed) {
        const { pull, x, m } = this.metaService;
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
        this.good = loaded.objects[pullName] as NonUnifiedGood;
      }
    };
  }

  public ngOnInit(): void {
    // Maximized
    this.subscription = combineLatest([this.refresh$, this.panel.manager.on$])
      .pipe(
        filter(() => {
          return this.panel.isExpanded;
        }),
        switchMap(() => {
          this.good = undefined;

          const m = this.allors.workspace.configuration.metaPopulation as M; const { pullBuilder: pull } = m; const x = {};
          const id = this.panel.manager.id;

          const pulls = [
            this.fetcher.locales,
            this.fetcher.internalOrganisation,
            pull.ProductIdentificationType(),
            pull.ProductCategory({ sorting: [{ roleType: m.ProductCategory.Name }] }),
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

          return this.allors.client.pullReactive(this.allors.session, pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.session.reset();

        this.good = loaded.object<NonUnifiedGood>(m.NonUnifiedGood);
        this.originalCategories = loaded.collection<ProductCategory>(m.ProductCategory);
        this.selectedCategories = this.originalCategories;

        this.categories = loaded.collection<ProductCategory>(m.ProductCategory);
        this.goodIdentificationTypes = loaded.collection<ProductIdentificationType>(m.ProductIdentificationType);
        this.locales = loaded.collection<Locale>(m.Locale);
        this.productFeatureApplicabilities = loaded.collection<ProductFeatureApplicability>(m.ProductFeatureApplicability);
        this.productDimensions = this.productFeatureApplicabilities
          .map((v) => v.ProductFeature)
          .filter((v) => v.objectType.name === this.m.ProductDimension.name) as ProductDimension[];

        const goodNumberType = this.goodIdentificationTypes.find((v) => v.UniqueId === 'b640630d-a556-4526-a2e5-60a84ab0db3f');

        this.productNumber = this.good.ProductIdentifications.find((v) => v.ProductIdentificationType === goodNumberType);
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.selectedCategories.forEach((category: ProductCategory) => {
      category.AddProduct(this.good);

      const index = this.originalCategories.indexOf(category);
      if (index > -1) {
        this.originalCategories.splice(index, 1);
      }
    });

    this.originalCategories.forEach((category: ProductCategory) => {
      category.RemoveProduct(this.good);
    });

    this.allors.client.pushReactive(this.allors.session).subscribe(() => {
      this.refreshService.refresh();
      this.panel.toggle();
    }, this.saveService.errorHandler);
  }

  public setDirty(): void {
    this.allors.session.hasChanges = true;
  }
}
