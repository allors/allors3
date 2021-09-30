import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest, Observable } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { ProductQuoteApproval } from '@allors/workspace/domain/default';
import { Action, ObjectData, RefreshService, SaveService, TestScope } from '@allors/workspace/angular/base';
import { SessionService } from '@allors/workspace/angular/core';
import { IResult } from '@allors/workspace/domain/system';

import { PrintService } from '../../../actions/print/print.service';

@Component({
  templateUrl: './productquoteapproval-edit.component.html',
  providers: [SessionService],
})
export class ProductQuoteApprovalEditComponent extends TestScope implements OnInit, OnDestroy {
  title: string;
  subTitle: string;

  readonly m: M;

  private subscription: Subscription;

  productQuoteApproval: ProductQuoteApproval;

  print: Action;

  constructor(
    @Self() public allors: SessionService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<ProductQuoteApprovalEditComponent>,
    public printService: PrintService,
    public refreshService: RefreshService,
    private saveService: SaveService
  ) {
    super();

    this.m = this.allors.workspace.configuration.metaPopulation as M;

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

          return this.allors.client.pullReactive(this.allors.session, pulls).pipe(map((loaded) => loaded));
        })
      )
      .subscribe((loaded) => {
        this.allors.session.reset();
        this.productQuoteApproval = loaded.object<ProductQuoteApproval>(m.ProductQuoteApproval);

        this.title = this.productQuoteApproval.Title;
      });
  }

  ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  approve(): void {
    this.saveAndInvoke(() => this.allors.client.invokeReactive(this.allors.session, this.productQuoteApproval.Approve));
  }

  reject(): void {
    this.saveAndInvoke(() => this.allors.client.invokeReactive(this.allors.session, this.productQuoteApproval.Reject));
  }

  saveAndInvoke(methodCall: () => Observable<IResult>): void {
    const m = this.m;
    const { pullBuilder: pull } = m;

    this.allors.client
      .pushReactive(this.allors.session)
      .pipe(
        switchMap(() => {
          return this.allors.client.pullReactive(this.allors.session, [pull.ProductQuoteApproval({ objectId: this.data.id })]);
        }),
        switchMap(() => {
          this.allors.session.reset();
          return methodCall();
        })
      )
      .subscribe(() => {
        this.dialogRef.close(this.productQuoteApproval);
        this.refreshService.refresh();
      }, this.saveService.errorHandler);
  }
}
