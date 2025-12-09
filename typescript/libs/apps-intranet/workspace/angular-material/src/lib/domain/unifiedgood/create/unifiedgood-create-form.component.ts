import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  InventoryItemKind,
  ProductIdentificationType,
  ProductNumber,
  ProductType,
  Settings,
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
export class UnifiedGoodCreateFormComponent extends AllorsFormComponent<UnifiedGood> {
  readonly m: M;

  productTypes: ProductType[];
  inventoryItemKinds: InventoryItemKind[];
  goodIdentificationTypes: ProductIdentificationType[];
  productNumber: ProductNumber;
  settings: Settings;
  goodNumberType: ProductIdentificationType;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      this.fetcher.Settings,
      p.InventoryItemKind({}),
      p.ProductType({ sorting: [{ roleType: m.ProductType.Name }] }),
      p.ProductIdentificationType({})
    );

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.context.create(this.createRequest.objectType);

    this.onPostPullInitialize(pullResult);

    this.inventoryItemKinds = pullResult.collection<InventoryItemKind>(
      this.m.InventoryItemKind
    );
    this.productTypes = pullResult.collection<ProductType>(this.m.ProductType);
    this.goodIdentificationTypes =
      pullResult.collection<ProductIdentificationType>(
        this.m.ProductIdentificationType
      );
    this.settings = this.fetcher.getSettings(pullResult);

    this.goodNumberType = this.goodIdentificationTypes?.find(
      (v) => v.UniqueId === 'b640630d-a556-4526-a2e5-60a84ab0db3f'
    );

    if (!this.settings.UseProductNumberCounter) {
      this.productNumber = this.allors.context.create<ProductNumber>(
        this.m.ProductNumber
      );
      this.productNumber.ProductIdentificationType = this.goodNumberType;

      this.object.addProductIdentification(this.productNumber);
    }
  }
}
