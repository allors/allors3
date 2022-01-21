import {
  Component,
  OnDestroy,
  OnInit,
  Self,
  Optional,
  Inject,
} from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { M } from '@allors/default/workspace/meta';
import {
  NonUnifiedGood,
  Organisation,
  ProductIdentificationType,
  ProductType,
  Settings,
  Good,
  ProductCategory,
  Ownership,
  ProductNumber,
  Locale,
} from '@allors/default/workspace/domain';
import {
  NavigationService,
  ObjectData,
  RefreshService,
  SaveService,
  SearchFactory,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { Filters } from '../../../filters/filters';

@Component({
  templateUrl: './nonunifiedgood-create.component.html',
  providers: [ContextService],
})
export class NonUnifiedGoodCreateComponent implements OnInit, OnDestroy {
  readonly m: M;
  good: Good;

  public title = 'Add Good';

  locales: Locale[];
  categories: ProductCategory[];
  productTypes: ProductType[];
  manufacturers: Organisation[];
  ownerships: Ownership[];
  organisations: Organisation[];
  goodIdentificationTypes: ProductIdentificationType[];
  productNumber: ProductNumber;
  selectedCategories: ProductCategory[] = [];
  settings: Settings;
  goodNumberType: ProductIdentificationType;

  private subscription: Subscription;

  nonUnifiedPartsFilter: SearchFactory;

  constructor(
    @Self() public allors: ContextService,
    @Optional() @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<NonUnifiedGoodCreateComponent>,

    private refreshService: RefreshService,
    public navigationService: NavigationService,
    private saveService: SaveService,
    private fetcher: FetcherService
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;

    this.subscription = combineLatest(this.refreshService.refresh$)
      .pipe(
        switchMap(() => {
          const pulls = [
            this.fetcher.locales,
            this.fetcher.Settings,
            pull.ProductIdentificationType({}),
            pull.ProductCategory({
              sorting: [{ roleType: m.ProductCategory.Name }],
            }),
          ];

          this.nonUnifiedPartsFilter = Filters.nonUnifiedPartsFilter(m);

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        this.categories = loaded.collection<ProductCategory>(m.ProductCategory);
        this.goodIdentificationTypes =
          loaded.collection<ProductIdentificationType>(
            m.ProductIdentificationType
          );
        this.locales = this.fetcher.getAdditionalLocales(loaded);
        this.settings = this.fetcher.getSettings(loaded);

        this.goodNumberType = this.goodIdentificationTypes?.find(
          (v) => v.UniqueId === 'b640630d-a556-4526-a2e5-60a84ab0db3f'
        );

        this.good = this.allors.context.create<NonUnifiedGood>(
          m.NonUnifiedGood
        );

        if (!this.settings.UseProductNumberCounter) {
          this.productNumber = this.allors.context.create<ProductNumber>(
            m.ProductNumber
          );
          this.productNumber.ProductIdentificationType = this.goodNumberType;

          this.good.addProductIdentification(this.productNumber);
        }
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
    });

    this.allors.context.push().subscribe(() => {
      this.dialogRef.close(this.good);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }
}
