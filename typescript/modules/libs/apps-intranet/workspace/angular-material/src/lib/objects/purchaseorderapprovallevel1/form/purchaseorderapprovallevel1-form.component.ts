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
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { PrintService } from '../../../actions/print/print.service';

@Component({
  templateUrl: './purchaseorderapprovallevel1-form.component.html',
  providers: [ContextService],
})
export class PurchaseOrderApprovalLevel1FormComponent
  implements OnInit, OnDestroy
{
  title: string;
  subTitle: string;

  readonly m: M;

  private subscription: Subscription;

  purchaseOrderApproval: PurchaseOrderApprovalLevel1;

  print: Action;

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<PurchaseOrderApprovalLevel1FormComponent>,

    public printService: PrintService,
    public refreshService: RefreshService,
    private errorService: ErrorService
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;

    this.print = printService.print(
      this.m.PurchaseOrderApprovalLevel1.PurchaseOrder
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
            pull.PurchaseOrderApprovalLevel1({
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
        this.purchaseOrderApproval = loaded.object<PurchaseOrderApprovalLevel1>(
          m.PurchaseOrderApprovalLevel1
        );

        this.title = this.purchaseOrderApproval.Title;
      });
  }

  ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  approve(): void {
    this.saveAndInvoke(() =>
      this.allors.context.invoke(this.purchaseOrderApproval.Approve)
    );
  }

  reject(): void {
    this.saveAndInvoke(() =>
      this.allors.context.invoke(this.purchaseOrderApproval.Reject)
    );
  }

  saveAndInvoke(methodCall: () => Observable<IResult>): void {
    const m = this.m;
    const { pullBuilder: pull } = m;

    this.allors.context
      .push()
      .pipe(
        switchMap(() => {
          return this.allors.context.pull([
            pull.PurchaseOrderApprovalLevel1({ objectId: this.data.id }),
          ]);
        }),
        switchMap(() => {
          this.allors.context.reset();
          return methodCall();
        })
      )
      .subscribe(() => {
        this.dialogRef.close(this.purchaseOrderApproval);
        this.refreshService.refresh();
      }, this.errorService.errorHandler);
  }
}
