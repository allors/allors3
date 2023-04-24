import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import { IrpfRate, IrpfRegime } from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

@Component({
  templateUrl: './irpfrate-form.component.html',
  providers: [ContextService],
})
export class IrpfRateFormComponent extends AllorsFormComponent<IrpfRate> {
  readonly m: M;
  irpfRegime: IrpfRegime;

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
        p.IrpfRate({
          name: '_object',
          objectId: this.editRequest.objectId,
        })
      );
    }

    const initializer = this.createRequest?.initializer;
    if (initializer) {
      pulls.push(
        p.IrpfRegime({
          objectId: initializer.id,
          include: {
            IrpfRates: {},
          },
        })
      );
    }
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.irpfRegime = pullResult.object<IrpfRegime>(this.m.IrpfRegime);

    if (this.createRequest) {
      this.irpfRegime.addIrpfRate(this.object);
    }
  }
}
