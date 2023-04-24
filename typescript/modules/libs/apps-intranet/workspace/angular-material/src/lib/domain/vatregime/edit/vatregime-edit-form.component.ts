import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  Country,
  Locale,
  VatClause,
  VatRegime,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { FetcherService } from '../../../services/fetcher/fetcher-service';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'vatregime-edit-form',
  templateUrl: './vatregime-edit-form.component.html',
  providers: [ContextService],
})
export class VatRegimeEditFormComponent extends AllorsFormComponent<VatRegime> {
  readonly m: M;
  public title: string;

  locales: Locale[];
  countries: Country[];
  vatClauses: VatClause[];

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
      this.fetcher.internalOrganisation,
      this.fetcher.locales,
      p.VatRegime({
        name: '_object',
        objectId: this.editRequest.objectId,
        include: {
          LocalisedNames: {
            Locale: {},
          },
          Country: {},
          LastModifiedBy: {},
        },
      }),
      p.Country({
        sorting: [{ roleType: m.Country.Name }],
      }),
      p.VatClause({
        sorting: [{ roleType: m.VatClause.Name }],
      })
    );

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.countries = pullResult.collection<Country>(this.m.Country);
    this.vatClauses = pullResult.collection<VatClause>(this.m.VatClause);
    this.locales = this.fetcher.getAdditionalLocales(pullResult);

    this.onPostPullInitialize(pullResult);
  }

  public override save(): void {
    super.save();
  }
}
