import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  ProductType,
  SerialisedItemCharacteristicType,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

@Component({
  templateUrl: './producttype-form.component.html',
  providers: [ContextService],
})
export class ProductTypeFormComponent extends AllorsFormComponent<ProductType> {
  public m: M;

  public characteristics: SerialisedItemCharacteristicType[];

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      p.SerialisedItemCharacteristicType({
        sorting: [{ roleType: m.SerialisedItemCharacteristicType.Name }],
      })
    );

    if (this.editRequest) {
      pulls.push(
        p.ProductType({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            SerialisedItemCharacteristicTypes: {},
          },
        })
      );
    }

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.onPostPullInitialize(pullResult);

    this.characteristics =
      pullResult.collection<SerialisedItemCharacteristicType>(
        this.m.SerialisedItemCharacteristicType
      );
  }
}
