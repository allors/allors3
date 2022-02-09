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
  UnifiedGood,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { FetcherService } from '../../../services/fetcher/fetcher-service';

@Component({
  templateUrl: './unifiedgood-create-form.component.html',
  providers: [ContextService],
})
export class UnifiedGoodCreateFormComponent
  extends AllorsFormComponent<UnifiedGood>
  implements CreateOrEditPullHandler, EditIncludeHandler, PostCreatePullHandler
{
  readonly m: M;
  good: Good;

  public title = 'Add Unified Good';

  productTypes: ProductType[];
  inventoryItemKinds: InventoryItemKind[];
  goodIdentificationTypes: ProductIdentificationType[];
  productNumber: ProductNumber;
  settings: Settings;
  goodNumberType: ProductIdentificationType;

  private subscription: Subscription;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;

    this.subscription = combineLatest(this.refreshService.refresh$)
      .pipe(
        switchMap(() => {
          const pulls = [
            this.fetcher.Settings,
            pull.InventoryItemKind({}),
            pull.ProductType({ sorting: [{ roleType: m.ProductType.Name }] }),
            pull.ProductIdentificationType({}),
          ];

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        this.inventoryItemKinds = loaded.collection<InventoryItemKind>(
          m.InventoryItemKind
        );
        this.productTypes = loaded.collection<ProductType>(m.ProductType);
        this.goodIdentificationTypes =
          loaded.collection<ProductIdentificationType>(
            m.ProductIdentificationType
          );
        this.settings = this.fetcher.getSettings(loaded);

        this.goodNumberType = this.goodIdentificationTypes?.find(
          (v) => v.UniqueId === 'b640630d-a556-4526-a2e5-60a84ab0db3f'
        );

        this.good = this.allors.context.create<UnifiedGood>(m.UnifiedGood);

        if (!this.settings.UseProductNumberCounter) {
          this.productNumber = this.allors.context.create<ProductNumber>(
            m.ProductNumber
          );
          this.productNumber.ProductIdentificationType = this.goodNumberType;

          this.good.addProductIdentification(this.productNumber);
        }
      });
  }
}
