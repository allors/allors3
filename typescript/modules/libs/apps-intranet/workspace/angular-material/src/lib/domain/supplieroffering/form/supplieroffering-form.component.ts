import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult, IObject } from '@allors/system/workspace/domain';
import {
  Currency,
  InternalOrganisation,
  Ordinal,
  Organisation,
  Part,
  Party,
  RatingType,
  Settings,
  SupplierOffering,
  SupplierRelationship,
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
  internalOrganisation: InternalOrganisation;
  ratingTypes: RatingType[];
  preferences: Ordinal[];
  unitsOfMeasure: UnitOfMeasure[];
  currencies: Currency[];
  settings: Settings;
  title: string;

  allSuppliersFilter: SearchFactory;
  addSupplier = false;
  supplierIsNew = false;

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
      this.fetcher.internalOrganisation,
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

    const initializer = this.createRequest?.initializer;
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

    this.internalOrganisation =
      this.fetcher.getInternalOrganisation(pullResult);
    this.ratingTypes = pullResult.collection<RatingType>(this.m.RatingType);
    this.preferences = pullResult.collection<Ordinal>(this.m.Ordinal);
    this.unitsOfMeasure = pullResult.collection<UnitOfMeasure>(
      this.m.UnitOfMeasure
    );
    this.currencies = pullResult.collection<Currency>(this.m.Currency);
    this.settings = this.fetcher.getSettings(pullResult);

    if (this.createRequest) {
      this.part = pullResult.object<Part>(this.m.Part);
      this.object.Part = this.part;
      this.object.Currency = this.settings.PreferredCurrency;
    } else {
      this.part = this.object.Part;
    }
  }

  public supplierAdded(supplier: Organisation): void {
    const supplierRelationship =
      this.allors.context.create<SupplierRelationship>(
        this.m.SupplierRelationship
      );

    supplierRelationship.Supplier = supplier;
    supplierRelationship.InternalOrganisation = this.internalOrganisation;

    supplier.PreferredCurrency = this.object.Currency;
    this.object.Supplier = supplier;
    this.supplierIsNew = true;
  }

  public currencySelected(currency: IObject) {
    if (this.supplierIsNew || this.object.Supplier?.PreferredCurrency == null) {
      this.object.Supplier.PreferredCurrency = currency as Currency;
    }
  }

  public supplierSelected(supplier: IObject) {
    this.supplierIsNew = false;
    this.updateSupplier(supplier as Party);
  }

  private updateSupplier(supplier: Party): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const pulls = [
      pull.Organisation({
        object: supplier,
        name: 'selectedSupplier',
        include: {
          PreferredCurrency: x,
        },
      }),
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
      const selectedSupplier = loaded.object<Organisation>('selectedSupplier');
      this.object.Currency = selectedSupplier?.PreferredCurrency;
    });
  }
}
