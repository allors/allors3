import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  Currency,
  CustomerRelationship,
  CustomOrganisationClassification,
  IndustryClassification,
  InternalOrganisation,
  LegalForm,
  Locale,
  Organisation,
  OrganisationRole,
  SupplierRelationship,
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
  templateUrl: './organisation-create-form.component.html',
  providers: [ContextService],
})
export class OrganisationCreateFormComponent extends AllorsFormComponent<Organisation> {
  public m: M;

  public locales: Locale[];
  public classifications: CustomOrganisationClassification[];
  public industries: IndustryClassification[];

  public internalOrganisation: InternalOrganisation;
  public roles: OrganisationRole[];
  public selectableRoles: OrganisationRole[] = [];
  public activeRoles: OrganisationRole[] = [];
  private customerRole: OrganisationRole;
  private supplierRole: OrganisationRole;
  private manufacturerRole: OrganisationRole;

  legalForms: LegalForm[];
  currencies: Currency[];

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService,
    private singletonId: SingletonId
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
      p.OrganisationRole({}),
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
    this.object = this.context.create(this.createRequest.objectType);

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
    this.roles = pullResult.collection<OrganisationRole>(
      this.m.OrganisationRole
    );

    this.customerRole = this.roles?.find(
      (v: OrganisationRole) =>
        v.UniqueId === '8b5e0cee-4c98-42f1-8f18-3638fba943a0'
    );

    this.supplierRole = this.roles?.find(
      (v: OrganisationRole) =>
        v.UniqueId === '8c6d629b-1e27-4520-aa8c-e8adf93a5095'
    );

    this.manufacturerRole = this.roles?.find(
      (v: OrganisationRole) =>
        v.UniqueId === '32e74bef-2d79-4427-8902-b093afa81661'
    );

    this.selectableRoles.push(this.customerRole);
    this.selectableRoles.push(this.supplierRole);

    this.object.IsManufacturer = false;
    this.object.IsInternalOrganisation = false;
    this.object.CollectiveWorkEffortInvoice = false;
    this.object.PreferredCurrency = this.internalOrganisation.PreferredCurrency;

    this.onPostPullInitialize(pullResult);
  }

  public override save(): void {
    if (this.activeRoles.indexOf(this.customerRole) > -1) {
      const customerRelationship =
        this.allors.context.create<CustomerRelationship>(
          this.m.CustomerRelationship
        );
      customerRelationship.Customer = this.object;
      customerRelationship.InternalOrganisation = this.internalOrganisation;
    }

    if (this.activeRoles.indexOf(this.supplierRole) > -1) {
      const supplierRelationship =
        this.allors.context.create<SupplierRelationship>(
          this.m.SupplierRelationship
        );
      supplierRelationship.Supplier = this.object;
      supplierRelationship.InternalOrganisation = this.internalOrganisation;
    }

    super.save();
  }
}
