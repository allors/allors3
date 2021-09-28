import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { SessionService, MetaService, RefreshService } from '@allors/angular/services/core';
import { Disbursement, PurchaseInvoice, PaymentApplication, Invoice } from '@allors/domain/generated';
import { PullRequest } from '@allors/protocol/system';
import { Meta } from '@allors/meta/generated';
import { ObjectData, SaveService } from '@allors/angular/material/services/core';
import { IObject } from '@allors/domain/system';
import { TestScope } from '@allors/angular/core';

@Component({
  templateUrl: './disbursement-edit.component.html',
  providers: [SessionService],
})
export class DisbursementEditComponent extends TestScope implements OnInit, OnDestroy {
  readonly m: M;

  disbursement: Disbursement;
  invoice: Invoice;

  title: string;

  private subscription: Subscription;
  paymentApplication: PaymentApplication;

  constructor(
    @Self() public allors: SessionService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<DisbursementEditComponent>,
    
    public refreshService: RefreshService,
    private saveService: SaveService
  ) {
    super();

    this.m = this.allors.workspace.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m; const { pullBuilder: pull } = m; const x = {};

    this.subscription = combineLatest([this.refreshService.refresh$])
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id === undefined;

          const pulls = [
          ];
          
          if (!isCreate) {
            pulls.push(
              pull.Disbursement({
                objectId: this.data.id,
                include: {
                  PaymentApplications: x,
                },
              }),
            );
          }

          if (isCreate && this.data.associationId) {
            pulls.push(
              pull.Invoice({
                object: this.data.associationId,
              })
            );
          }

          return this.allors.client.pullReactive(this.allors.session, pulls).pipe(map((loaded) => ({ loaded, isCreate })));
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.session.reset();

        this.invoice = loaded.object<Invoice>(m.Invoice);

        if (isCreate) {
          this.title = 'Add Disbursement';
          this.paymentApplication = this.allors.session.create<PaymentApplication>(m.PaymentApplication);
          this.paymentApplication.Invoice = this.invoice;

          this.disbursement = this.allors.session.create<Disbursement>(m.Disbursement);
          this.disbursement.AddPaymentApplication(this.paymentApplication);
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

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.paymentApplication.AmountApplied = this.disbursement.Amount;

    this.allors.client.pushReactive(this.allors.session).subscribe(() => {
      const data: IObject = {
        id: this.disbursement.id,
        objectType: this.disbursement.objectType,
      };

      this.dialogRef.close(data);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }
}
