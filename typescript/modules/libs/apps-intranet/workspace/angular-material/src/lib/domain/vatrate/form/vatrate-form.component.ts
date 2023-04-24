import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import { VatRate, VatRegime } from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

@Component({
  templateUrl: './vatrate-form.component.html',
  providers: [ContextService],
})
export class VatRateFormComponent extends AllorsFormComponent<VatRate> {
  readonly m: M;
  vatRegime: VatRegime;

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
        p.VatRate({
          name: '_object',
          objectId: this.editRequest.objectId,
        })
      );
    }

    const initializer = this.createRequest?.initializer;
    if (initializer) {
      pulls.push(
        p.VatRegime({
          objectId: initializer.id,
          include: {
            VatRates: {},
          },
        })
      );
    }
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.vatRegime = pullResult.object<VatRegime>(this.m.VatRegime);

    if (this.createRequest) {
      this.vatRegime.addVatRate(this.object);
    }
  }
}
