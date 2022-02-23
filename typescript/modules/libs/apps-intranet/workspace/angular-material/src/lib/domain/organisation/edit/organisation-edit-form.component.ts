import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  Currency,
  CustomOrganisationClassification,
  IndustryClassification,
  InternalOrganisation,
  LegalForm,
  Locale,
  Organisation,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
  SingletonId,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { FetcherService } from '../../../services/fetcher/fetcher-service';

@Component({
  selector: 'organisation-edit-form',
  templateUrl: './organisation-edit-form.component.html',
  providers: [ContextService],
})
export class OrganisationEditFormComponent extends AllorsFormComponent<Organisation> {
  readonly m: M;
  locales: Locale[];
  classifications: CustomOrganisationClassification[];
  industries: IndustryClassification[];
  internalOrganisation: InternalOrganisation;
  legalForms: LegalForm[];
  currencies: Currency[];

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private singletonId: SingletonId,
    private fetcher: FetcherService
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      this.fetcher.internalOrganisation,
      p.Singleton({
        objectId: this.singletonId.value,
        select: {
          Locales: {
            include: {
              Language: {},
              Country: {},
            },
          },
        },
      }),
      p.Organisation({
        name: '_object',
        objectId: this.editRequest.objectId,
        include: {
          LegalForm: {},
          IndustryClassifications: {},
          CustomClassifications: {},
          PreferredCurrency: {},
          LogoImage: {},
        },
      }),
      p.Currency({
        predicate: {
          kind: 'Equals',
          propertyType: m.Currency.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.Currency.Name }],
      }),
      p.CustomOrganisationClassification({
        sorting: [{ roleType: m.CustomOrganisationClassification.Name }],
      }),
      p.IndustryClassification({
        sorting: [{ roleType: m.IndustryClassification.Name }],
      }),
      p.LegalForm({
        sorting: [{ roleType: m.LegalForm.Description }],
      })
    );

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    this.object = pullResult.object('_object');

    this.onPostPullInitialize(pullResult);

    this.internalOrganisation =
      this.fetcher.getInternalOrganisation(pullResult);
    this.currencies = pullResult.collection<Currency>(this.m.Currency);
    this.locales =
      pullResult.collection<Locale>(this.m.Singleton.Locales) || [];
    this.classifications =
      pullResult.collection<CustomOrganisationClassification>(
        this.m.CustomOrganisationClassification
      );
    this.industries = pullResult.collection<IndustryClassification>(
      this.m.IndustryClassification
    );
    this.legalForms = pullResult.collection<LegalForm>(this.m.LegalForm);
  }
}
