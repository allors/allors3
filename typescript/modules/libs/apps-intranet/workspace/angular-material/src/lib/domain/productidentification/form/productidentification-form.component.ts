import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult, IObject } from '@allors/system/workspace/domain';
import {
  Good,
  Part,
  ProductIdentification,
  ProductIdentificationType,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { RoleType } from '@allors/system/workspace/meta';

@Component({
  templateUrl: './productidentification-form.component.html',
  providers: [ContextService],
})
export class ProductIdentificationFormComponent extends AllorsFormComponent<ProductIdentification> {
  public m: M;

  public container: IObject;
  public productIdentificationTypes: ProductIdentificationType[];

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
      p.ProductIdentificationType({
        predicate: {
          kind: 'Equals',
          propertyType: m.ProductIdentificationType.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.ProductIdentificationType.Name }],
      })
    );

    if (this.editRequest) {
      pulls.push(
        p.ProductIdentification({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            ProductIdentificationType: {},
          },
        })
      );
    }

    const initializer = this.createRequest?.initializer;
    if (initializer) {
      pulls.push(
        p.Good({ objectId: initializer.id }),
        p.Part({ objectId: initializer.id })
      );
    }
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.productIdentificationTypes =
      pullResult.collection<ProductIdentificationType>(
        this.m.ProductIdentificationType
      );

    if (this.createRequest) {
      this.container =
        pullResult.object<Good>(this.m.Good) ||
        pullResult.object<Part>(this.m.Part);

      this.container.strategy.addCompositesRole(
        this.createRequest?.initializer.propertyType as RoleType,
        this.object
      );
    }
  }
}
