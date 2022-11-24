import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult, IObject } from '@allors/system/workspace/domain';
import {
  Invoice,
  Order,
  OrderAdjustment,
  Quote,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { AssociationType, RoleType } from '@allors/system/workspace/meta';

@Component({
  templateUrl: './orderadjustment-form.component.html',
  providers: [ContextService],
})
export class OrderAdjustmentFormComponent extends AllorsFormComponent<OrderAdjustment> {
  public m: M;

  public container: IObject;

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

    if (this.editRequest) {
      pulls.push(
        p.OrderAdjustment({
          name: '_object',
          objectId: this.editRequest.objectId,
        })
      );
    }

    const initializer = this.createRequest?.initializer;
    if (initializer) {
      pulls.push(
        p.Quote({ objectId: initializer.id }),
        p.Order({ objectId: initializer.id }),
        p.Invoice({ objectId: initializer.id })
      );
    }
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.container =
      pullResult.object<Quote>(this.m.Quote) ??
      pullResult.object<Order>(this.m.Order) ??
      pullResult.object<Invoice>(this.m.Invoice);

    if (this.createRequest) {
      const associationType = this.createRequest?.initializer
        .propertyType as AssociationType;
      const roleType = associationType.roleType;
      this.container.strategy.addCompositesRole(roleType, this.object);
    }
  }
}
