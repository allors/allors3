import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import { Bank, BankAccount, Currency } from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { FetcherService } from '../../../services/fetcher/fetcher-service';

@Component({
  templateUrl: './bankaccount-form.component.html',
  providers: [ContextService],
})
export class BankAccountFormComponent extends AllorsFormComponent<BankAccount> {
  m: M;
  currencies: Currency[];
  addBank = false;
  banks: Bank[];

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      this.fetcher.locales,
      p.Currency({
        predicate: {
          kind: 'Equals',
          propertyType: m.Currency.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.Currency.IsoCode }],
      }),
      p.Bank({ sorting: [{ roleType: m.Bank.Name }] })
    );

    if (this.editRequest) {
      pulls.push(
        p.BankAccount({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            Bank: {},
            Currency: {},
          },
        })
      );
    }

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.currencies = pullResult.collection<Currency>(this.m.Currency);
    this.banks = pullResult.collection<Bank>(this.m.Bank);

    this.onPostPullInitialize(pullResult);
  }

  public bankAdded(bank: Bank): void {
    this.banks.push(bank);
    this.object.Bank = bank;
  }
}
