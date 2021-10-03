import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { Organisation, InternalOrganisation, ExchangeRate, Currency } from '@allors/workspace/domain/default';
import { ObjectData, RefreshService, SaveService, TestScope } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';
import { IObject } from '@allors/workspace/domain/system';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { FetcherService } from '../../../services/fetcher/fetcher-service';

@Component({
  templateUrl: './exchangerate-edit.component.html',
  providers: [ContextService],
})
export class ExchangeRateEditComponent extends TestScope implements OnInit, OnDestroy {
  public title: string;
  public subTitle: string;

  public m: M;

  public exchangeRate: ExchangeRate;
  internalOrganisation: Organisation;
  currencies: Currency[];

  private subscription: Subscription;

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<ExchangeRateEditComponent>,

    public refreshService: RefreshService,
    private saveService: SaveService,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super();

    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest([this.refreshService.refresh$, this.internalOrganisationId.observable$])
      .pipe(
        switchMap(([, internalOrganisationId]) => {
          const isCreate = this.data.id === undefined;

          const pulls = [
            this.fetcher.internalOrganisation,
            pull.Currency({
              predicate: { kind: 'Equals', propertyType: m.Currency.IsActive, value: true },
              sorting: [{ roleType: m.Currency.Name }],
            }),
          ];

          if (!isCreate) {
            pulls.push(
              pull.ExchangeRate({
                objectId: this.data.id,
              })
            );
          }

          return this.allors.context.pull(pulls).pipe(map((loaded) => ({ loaded, isCreate })));
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.context.reset();
        this.internalOrganisation = loaded.object<Organisation>(m.InternalOrganisation);
        this.currencies = loaded.collection<Currency>(m.Currency);

        if (isCreate) {
          this.title = 'Add Position Type';
          this.exchangeRate = this.allors.context.create<ExchangeRate>(m.ExchangeRate);
          this.exchangeRate.ToCurrency = this.internalOrganisation.PreferredCurrency;
        } else {
          this.exchangeRate = loaded.object<ExchangeRate>(m.ExchangeRate);

          if (this.exchangeRate.canWriteRate) {
            this.title = 'Edit Exchange Rate';
          } else {
            this.title = 'View Exchange Rate';
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
    this.allors.context.push().subscribe(() => {
      this.dialogRef.close(this.exchangeRate);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }
}
