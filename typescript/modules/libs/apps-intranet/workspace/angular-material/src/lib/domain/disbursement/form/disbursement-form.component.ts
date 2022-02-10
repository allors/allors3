import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import {
  EditIncludeHandler,
  IPullResult,
  Node,
  PostCreatePullHandler,
  PostEditPullHandler,
} from '@allors/system/workspace/domain';
import {
  Disbursement,
  Invoice,
  PaymentApplication,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

@Component({
  templateUrl: './disbursement-form.component.html',
  providers: [ContextService],
})
export class DisbursementFormComponent
  extends AllorsFormComponent<Disbursement>
  implements EditIncludeHandler, PostCreatePullHandler, PostEditPullHandler
{
  readonly m: M;
  paymentApplication: PaymentApplication;
  invoice: Invoice;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  onEditInclude(): Node[] {
    const { treeBuilder: t } = this.m;

    return t.Disbursement({
      PaymentApplications: {},
    });
  }

  onPostCreatePull(_, loaded: IPullResult): void {
    this.paymentApplication = this.allors.context.create<PaymentApplication>(
      this.m.PaymentApplication
    );

    this.invoice = loaded.object<Invoice>(this.m.Invoice);
    this.paymentApplication.Invoice = this.invoice;

    this.object.addPaymentApplication(this.paymentApplication);
  }

  onPostEditPull(): void {
    this.paymentApplication = this.object.PaymentApplications[0];
  }

  public override save(): void {
    this.paymentApplication.AmountApplied = this.object.Amount;
    super.save();
  }

  // TODO: KOEN
  // Pre
  // if (isCreate && this.data.associationId) {
  //   pulls.push(
  //     pull.Invoice({
  //       object: this.data.associationId,
  //     })
  //   );
  // }
}
