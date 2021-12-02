import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest, Observable } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { PurchaseOrderApprovalLevel2 } from '@allors/workspace/domain/default';
import { Action, ObjectData, RefreshService, SaveService } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';
import { IObject, IResult } from '@allors/workspace/domain/system';

import { PrintService } from '../../../actions/print/print.service';

@Component({
  templateUrl: './purchaseorderapprovallevel2-edit.component.html',
  providers: [ContextService],
})
export class PurchaseOrderApprovalLevel2EditComponent implements OnInit, OnDestroy {
  title: string;
  subTitle: string;

  readonly m: M;

  private subscription: Subscription;

  purchaseOrderApproval: PurchaseOrderApprovalLevel2;

  print: Action;

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<PurchaseOrderApprovalLevel2EditComponent>,
    public printService: PrintService,
    public refreshService: RefreshService,
    private saveService: SaveService
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;

    this.print = printService.print(this.m.PurchaseOrderApprovalLevel2.PurchaseOrder);
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
        this.purchaseOrderApproval = loaded.object<PurchaseOrderApprovalLevel2>(m.PurchaseOrderApprovalLevel2);

        this.title = this.purchaseOrderApproval.Title;
      });
  }

  ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  approve(): void {
    this.saveAndInvoke(() => this.allors.context.invoke(this.purchaseOrderApproval.Approve));
  }

  reject(): void {
    this.saveAndInvoke(() => this.allors.context.invoke(this.purchaseOrderApproval.Reject));
  }

  saveAndInvoke(methodCall: () => Observable<IResult>): void {
    const m = this.m;
    const { pullBuilder: pull } = m;

    this.allors.context
      .push()
      .pipe(
        switchMap(() => {
          return this.allors.context.pull([pull.PurchaseOrderApprovalLevel2({ objectId: this.data.id })]);
        }),
        switchMap(() => {
          this.allors.context.reset();
          return methodCall();
        })
      )
      .subscribe(() => {
        this.dialogRef.close(this.purchaseOrderApproval);
        this.refreshService.refresh();
      }, this.saveService.errorHandler);
  }
}
