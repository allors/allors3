import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Carrier } from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { IPullResult, Pull } from '@allors/system/workspace/domain';

@Component({
  templateUrl: './carrier-form.component.html',
  providers: [ContextService],
})
export class CarrierFormComponent extends AllorsFormComponent<Carrier> {
  public m: M;

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
        p.Carrier({
          name: '_object',
          objectId: this.editRequest.objectId,
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
  }
}
