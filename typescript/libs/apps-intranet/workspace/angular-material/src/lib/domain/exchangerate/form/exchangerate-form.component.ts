import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  Currency,
  ExchangeRate,
  InternalOrganisation,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { FetcherService } from '../../../services/fetcher/fetcher-service';

@Component({
  templateUrl: './exchangerate-form.component.html',
  providers: [ContextService],
})
export class ExchangeRateFormComponent extends AllorsFormComponent<ExchangeRate> {
  public m: M;

  internalOrganisation: InternalOrganisation;
  currencies: Currency[];

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      this.fetcher.internalOrganisation,
      p.Currency({
        predicate: {
          kind: 'Equals',
          propertyType: m.Currency.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.Currency.Name }],
      })
    );

    if (this.editRequest) {
      pulls.push(
        p.ExchangeRate({
          name: '_object',
          objectId: this.editRequest.objectId,
        })
      );
    }

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.onPostPullInitialize(pullResult);

    this.internalOrganisation =
      this.fetcher.getInternalOrganisation(pullResult);
    this.currencies = pullResult.collection<Currency>(this.m.Currency);

    if (this.createRequest) {
      this.object.ToCurrency = this.internalOrganisation.PreferredCurrency;
    }
  }
}
