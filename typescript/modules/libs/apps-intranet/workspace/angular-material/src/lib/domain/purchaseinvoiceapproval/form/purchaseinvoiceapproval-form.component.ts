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
  PurchaseInvoiceApproval,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { PrintService } from '../../../actions/print/print.service';

@Component({
  templateUrl: './purchaseinvoiceapproval-form.component.html',
  providers: [ContextService],
})
export class PurchaseInvoiceApprovalFormComponent
  extends AllorsFormComponent<PurchaseInvoiceApproval>
  implements CreateOrEditPullHandler, EditIncludeHandler, PostCreatePullHandler
{
  title: string;
  subTitle: string;

  readonly m: M;

  private subscription: Subscription;

  purchaseInvoiceApproval: PurchaseInvoiceApproval;

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
      this.m.PurchaseInvoiceApproval.PurchaseInvoice
    );
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest(this.refreshService.refresh$)
      .pipe(
        switchMap(() => {
          const pulls = [
            pull.PurchaseInvoiceApproval({
              objectId: this.data.id,
              include: {
                PurchaseInvoice: {
                  PrintDocument: x,
                },
              },
            }),
          ];

          return this.allors.context.pull(pulls).pipe(map((loaded) => loaded));
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();
        this.purchaseInvoiceApproval = loaded.object<PurchaseInvoiceApproval>(
          m.PurchaseInvoiceApproval
        );

        this.title = this.purchaseInvoiceApproval.Title;
      });
  }

  approve(): void {
    this.saveAndInvoke(() =>
      this.allors.context.invoke(this.purchaseInvoiceApproval.Approve)
    );
  }

  reject(): void {
    this.saveAndInvoke(() =>
      this.allors.context.invoke(this.purchaseInvoiceApproval.Reject)
    );
  }

  // TODO: KOEN
  saveAndInvoke(methodCall: () => Observable<IResult>): void {
    const m = this.m;
    const { pullBuilder: pull } = m;

    this.allors.context
      .push()
      .pipe(
        switchMap(() => {
          return this.allors.context.pull([
            pull.PurchaseInvoiceApproval({ objectId: this.data.id }),
          ]);
        }),
        switchMap(() => {
          this.allors.context.reset();
          return methodCall();
        })
      )
      .subscribe(() => {
        this.dialogRef.close(this.purchaseInvoiceApproval);
        this.refreshService.refresh();
      }, this.errorService.errorHandler);
  }
}
