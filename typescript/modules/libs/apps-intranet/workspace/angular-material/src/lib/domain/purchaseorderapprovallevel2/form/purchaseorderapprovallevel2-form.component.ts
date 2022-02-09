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
  PurchaseOrderApprovalLevel2,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { PrintService } from '../../../actions/print/print.service';

@Component({
  templateUrl: './purchaseorderapprovallevel2-form.component.html',
  providers: [ContextService],
})
export class PurchaseOrderApprovalLevel2FormComponent
  extends AllorsFormComponent<PurchaseOrderApprovalLevel2>
  implements CreateOrEditPullHandler, EditIncludeHandler, PostCreatePullHandler
{
  title: string;
  subTitle: string;

  readonly m: M;

  private subscription: Subscription;

  purchaseOrderApproval: PurchaseOrderApprovalLevel2;

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

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest(this.refreshService.refresh$)
      .pipe(
        switchMap(() => {
          const pulls = [
            pull.PurchaseOrderApprovalLevel2({
              objectId: this.data.id,
              include: {
                PurchaseOrder: {
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
        this.purchaseOrderApproval = loaded.object<PurchaseOrderApprovalLevel2>(
          m.PurchaseOrderApprovalLevel2
        );

        this.title = this.purchaseOrderApproval.Title;
      });
  }

  //TODO: KOEN
  // approve(): void {
  //   this.saveAndInvoke(() =>
  //     this.allors.context.invoke(this.purchaseOrderApproval.Approve)
  //   );
  // }

  //TODO: KOEN
  // reject(): void {
  //   this.saveAndInvoke(() =>
  //     this.allors.context.invoke(this.purchaseOrderApproval.Reject)
  //   );
  // }
}