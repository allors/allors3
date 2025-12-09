import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import { PurchaseOrderApprovalLevel1 } from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
  Action,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { PrintService } from '../../../actions/print/print.service';

@Component({
  templateUrl: './purchaseorderapprovallevel1-form.component.html',
  providers: [ContextService],
})
export class PurchaseOrderApprovalLevel1FormComponent extends AllorsFormComponent<PurchaseOrderApprovalLevel1> {
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
      this.m.PurchaseOrderApprovalLevel1.PurchaseOrder
    );
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    if (this.editRequest) {
      pulls.push(
        p.PurchaseOrderApprovalLevel1({
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

  approve(): void {
    this.saveAndInvoke(() => this.allors.context.invoke(this.object.Approve));
  }

  reject(): void {
    this.saveAndInvoke(() => this.allors.context.invoke(this.object.Reject));
  }
}
