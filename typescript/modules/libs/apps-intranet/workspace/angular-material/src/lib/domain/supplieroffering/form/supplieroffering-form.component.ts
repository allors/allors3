import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  Currency,
  Ordinal,
  Part,
  RatingType,
  Settings,
  SupplierOffering,
  UnitOfMeasure,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
  SearchFactory,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { Filters } from '../../../filters/filters';

@Component({
  templateUrl: './supplieroffering-form.component.html',
  providers: [ContextService],
})
export class SupplierOfferingFormComponent extends AllorsFormComponent<SupplierOffering> {
  readonly m: M;
  part: Part;
  ratingTypes: RatingType[];
  preferences: Ordinal[];
  unitsOfMeasure: UnitOfMeasure[];
  currencies: Currency[];
  settings: Settings;
  title: string;

  allSuppliersFilter: SearchFactory;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;

    this.allSuppliersFilter = Filters.allSuppliersFilter(this.m);
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      this.fetcher.Settings,
      p.RatingType({ sorting: [{ roleType: m.RateType.Name }] }),
      p.Ordinal({ sorting: [{ roleType: m.Ordinal.Name }] }),
      p.UnitOfMeasure({
        sorting: [{ roleType: m.UnitOfMeasure.Name }],
      }),
      p.Currency({ sorting: [{ roleType: m.Currency.Name }] })
    );

    if (this.editRequest) {
      pulls.push(
        p.SupplierOffering({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            Part: {},
            Rating: {},
            Preference: {},
            Supplier: {},
            Currency: {},
            UnitOfMeasure: {},
          },
        })
      );
    }

    const initializer = this.createRequest.initializer;
    if (initializer) {
      pulls.push(
        p.Part({
          objectId: initializer.id,
          include: {
            SuppliedBy: {},
          },
        })
      );
    }
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.ratingTypes = pullResult.collection<RatingType>(this.m.RatingType);
    this.preferences = pullResult.collection<Ordinal>(this.m.Ordinal);
    this.unitsOfMeasure = pullResult.collection<UnitOfMeasure>(
      this.m.UnitOfMeasure
    );
    this.currencies = pullResult.collection<Currency>(this.m.Currency);
    this.settings = this.fetcher.getSettings(pullResult);

    if (this.create) {
      this.part = pullResult.object<Part>(this.m.Part);
      this.object.Part = this.part;
      this.object.Currency = this.settings.PreferredCurrency;
    } else {
      this.part = this.object.Part;
    }
  }
}
