import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest, Observable } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { M } from '@allors/default/workspace/meta';
import { ProductQuoteApproval } from '@allors/default/workspace/domain';
import {
  Action,
  ObjectData,
  RefreshService,
  ErrorService,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { IResult } from '@allors/system/workspace/domain';

import { PrintService } from '../../../actions/print/print.service';

@Component({
  templateUrl: './productquoteapproval-edit.component.html',
  providers: [ContextService],
})
export class ProductQuoteApprovalEditComponent implements OnInit, OnDestroy {
  title: string;
  subTitle: string;

  readonly m: M;

  private subscription: Subscription;

  productQuoteApproval: ProductQuoteApproval;

  print: Action;

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<ProductQuoteApprovalEditComponent>,
    public printService: PrintService,
    public refreshService: RefreshService,
    private errorService: ErrorService
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;

    this.print = printService.print(this.m.ProductQuoteApproval.ProductQuote);
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest(this.refreshService.refresh$)
      .pipe(
        switchMap(() => {
          const pulls = [
            pull.ProductQuoteApproval({
              objectId: this.data.id,
              include: {
                ProductQuote: {
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
        this.productQuoteApproval = loaded.object<ProductQuoteApproval>(
          m.ProductQuoteApproval
        );

        this.title = this.productQuoteApproval.Title;
      });
  }

  ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  approve(): void {
    this.saveAndInvoke(() =>
      this.allors.context.invoke(this.productQuoteApproval.Approve)
    );
  }

  reject(): void {
    this.saveAndInvoke(() =>
      this.allors.context.invoke(this.productQuoteApproval.Reject)
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
            pull.ProductQuoteApproval({ objectId: this.data.id }),
          ]);
        }),
        switchMap(() => {
          this.allors.context.reset();
          return methodCall();
        })
      )
      .subscribe(() => {
        this.dialogRef.close(this.productQuoteApproval);
        this.refreshService.refresh();
      }, this.errorService.errorHandler);
  }
}
