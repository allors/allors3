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
  Disbursement,
  InternalOrganisation,
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
  implements CreateOrEditPullHandler, EditIncludeHandler, PostCreatePullHandler
{
  readonly m: M;

  disbursement: Disbursement;
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
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest([this.refreshService.refresh$])
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id == null;

          const pulls = [];

          if (!isCreate) {
            pulls.push(
              pull.Disbursement({
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
          this.title = 'Add Disbursement';
          this.paymentApplication =
            this.allors.context.create<PaymentApplication>(
              m.PaymentApplication
            );
          this.paymentApplication.Invoice = this.invoice;

          this.disbursement = this.allors.context.create<Disbursement>(
            m.Disbursement
          );
          this.disbursement.addPaymentApplication(this.paymentApplication);
        } else {
          this.disbursement = loaded.object<Disbursement>(m.Disbursement);
          this.paymentApplication = this.disbursement.PaymentApplications[0];

          if (this.disbursement.canWriteAmount) {
            this.title = 'Edit Disbursement';
          } else {
            this.title = 'View Disbursement';
          }
        }
      });
  }

  public override save(): void {
    this.paymentApplication.AmountApplied = this.disbursement.Amount;
    super.save();
  }
}
