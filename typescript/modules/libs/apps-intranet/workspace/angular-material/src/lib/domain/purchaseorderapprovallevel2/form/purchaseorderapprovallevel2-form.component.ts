import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import { PurchaseOrderApprovalLevel2 } from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { PrintService } from '../../../actions/print/print.service';
import { Action } from '@allors/base/workspace/angular/application';

@Component({
  templateUrl: './purchaseorderapprovallevel2-form.component.html',
  providers: [ContextService],
})
export class PurchaseOrderApprovalLevel2FormComponent extends AllorsFormComponent<PurchaseOrderApprovalLevel2> {
  readonly m: M;

  print: Action;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    public printService: PrintService
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;

    this.print = printService.print(
      this.m.PurchaseOrderApprovalLevel2.PurchaseOrder
    );
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    if (this.editRequest) {
      pulls.push(
        p.PurchaseOrderApprovalLevel2({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            PurchaseOrder: {
              PrintDocument: {},
            },
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
  }
}
