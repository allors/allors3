import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  Invoice,
  PaymentApplication,
  Receipt,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

@Component({
  templateUrl: './receipt-form.component.html',
  providers: [ContextService],
})
export class ReceiptFormComponent extends AllorsFormComponent<Receipt> {
  readonly m: M;

  invoice: Invoice;

  paymentApplication: PaymentApplication;

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
        p.Receipt({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            PaymentApplications: {},
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

    if (this.createRequest) {
      this.paymentApplication = this.allors.context.create<PaymentApplication>(
        this.m.PaymentApplication
      );

      this.object.addPaymentApplication(this.paymentApplication);
    } else {
      this.paymentApplication = this.object.PaymentApplications[0];
    }

    this.onPostPullInitialize(pullResult);
  }

  public override save(): void {
    this.paymentApplication.AmountApplied = this.object.Amount;
    super.save();
  }
}
