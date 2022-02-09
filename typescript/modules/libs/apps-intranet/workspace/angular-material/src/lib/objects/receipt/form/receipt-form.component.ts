import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import {
  EditIncludeHandler,
  Node,
  CreateOrEditPullHandler,
  Pull,
  IPullResult,
  PostCreatePullHandler,
} from '@allors/system/workspace/domain';
import {
  BasePrice,
  InternalOrganisation,
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
export class ReceiptFormComponent
  extends AllorsFormComponent<Receipt>
  implements CreateOrEditPullHandler, EditIncludeHandler, PostCreatePullHandler
{
  readonly m: M;

  receipt: Receipt;
  invoice: Invoice;

  title: string;

  private subscription: Subscription;
  paymentApplication: PaymentApplication;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest(this.refreshService.refresh$)
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id == null;

          const pulls = [];

          if (!isCreate) {
            pulls.push(
              pull.Receipt({
                objectId: this.data.id,
                include: {
                  PaymentApplications: x,
                },
              })
            );
          }

          if (isCreate && this.data.associationId) {
            pulls.push(
              pull.Invoice({
                objectId: this.data.associationId,
              })
            );
          }

          return this.allors.context
            .pull(pulls)
            .pipe(map((loaded) => ({ loaded, isCreate })));
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.context.reset();

        this.invoice = loaded.object<Invoice>(m.Invoice);

        if (isCreate) {
          this.title = 'Add Receipt';
          this.paymentApplication =
            this.allors.context.create<PaymentApplication>(
              m.PaymentApplication
            );
          this.paymentApplication.Invoice = this.invoice;

          this.receipt = this.allors.context.create<Receipt>(m.Receipt);
          this.receipt.addPaymentApplication(this.paymentApplication);
        } else {
          this.receipt = loaded.object<Receipt>(m.Receipt);
          this.paymentApplication = this.receipt.PaymentApplications[0];

          if (this.receipt.canWriteAmount) {
            this.title = 'Edit Receipt';
          } else {
            this.title = 'View Receipt';
          }
        }
      });
  }

  public save(): void {
    this.paymentApplication.AmountApplied = this.receipt.Amount;

    super.save();
  }
}
