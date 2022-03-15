import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  Currency,
  CustomerRelationship,
  EmailFrequency,
  Employment,
  Enumeration,
  GenderType,
  InternalOrganisation,
  Locale,
  Organisation,
  OrganisationContactKind,
  OrganisationContactRelationship,
  Person,
  PersonRole,
  Salutation,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
  SingletonId,
  SearchFactory,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { Filters } from '../../../filters/filters';

@Component({
  templateUrl: './person-create-form.component.html',
  providers: [ContextService],
})
export class PersonCreateFormComponent extends AllorsFormComponent<Person> {
  readonly m: M;

  internalOrganisation: InternalOrganisation;
  organisation: Organisation;

  locales: Locale[];
  genders: Enumeration[];
  salutations: Enumeration[];
  organisationContactKinds: OrganisationContactKind[];
  selectedContactKinds: OrganisationContactKind[] = [];

  roles: PersonRole[];
  selectedRoles: PersonRole[] = [];
  organisationsFilter: SearchFactory;

  private customerRole: PersonRole;
  private employeeRole: PersonRole;
  currencies: Currency[];
  emailFrequencies: EmailFrequency[];

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService,
    private singletonId: SingletonId
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;

    this.organisationsFilter = Filters.organisationsFilter(this.m);
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
      p.EmailFrequency({
        predicate: {
          kind: 'Equals',
          propertyType: m.EmailFrequency.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.EmailFrequency.Name }],
      }),
      p.Currency({
        predicate: {
          kind: 'Equals',
          propertyType: m.Currency.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.Currency.Name }],
      }),
      p.GenderType({
        predicate: {
          kind: 'Equals',
          propertyType: m.GenderType.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.GenderType.Name }],
      }),
      p.Salutation({
        predicate: {
          kind: 'Equals',
          propertyType: m.Salutation.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.Salutation.Name }],
      }),
      p.PersonRole({
        sorting: [{ roleType: m.PersonRole.Name }],
      }),
      p.OrganisationContactKind({
        sorting: [{ roleType: m.OrganisationContactKind.Description }],
      })
    );

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.onPostPullInitialize(pullResult);

    this.internalOrganisation = this.internalOrganisation =
      this.fetcher.getInternalOrganisation(pullResult);
    this.currencies = pullResult.collection<Currency>(this.m.Currency);
    this.locales =
      pullResult.collection<Locale>(this.m.Singleton.Locales) || [];
    this.genders = pullResult.collection<GenderType>(this.m.GenderType);
    this.salutations = pullResult.collection<Salutation>(this.m.Salutation);
    this.roles = pullResult.collection<PersonRole>(this.m.PersonRole);
    this.emailFrequencies = pullResult.collection<EmailFrequency>(
      this.m.EmailFrequency
    );
    this.organisationContactKinds =
      pullResult.collection<OrganisationContactKind>(
        this.m.OrganisationContactKind
      );

    this.customerRole = this.roles?.find(
      (v: PersonRole) => v.UniqueId === 'b29444ef-0950-4d6f-ab3e-9c6dc44c050f'
    );
    this.employeeRole = this.roles?.find(
      (v: PersonRole) => v.UniqueId === 'db06a3e1-6146-4c18-a60d-dd10e19f7243'
    );

    this.object.CollectiveWorkEffortInvoice = false;
    this.object.PreferredCurrency = this.internalOrganisation.PreferredCurrency;
  }

  public override save(): void {
    if (this.selectedRoles.indexOf(this.customerRole) > -1) {
      const customerRelationship =
        this.allors.context.create<CustomerRelationship>(
          this.m.CustomerRelationship
        );
      customerRelationship.Customer = this.object;
      customerRelationship.InternalOrganisation = this.internalOrganisation;
    }

    if (this.selectedRoles.indexOf(this.employeeRole) > -1) {
      const employment = this.allors.context.create<Employment>(
        this.m.Employment
      );
      employment.Employee = this.object;
      employment.Employer = this.internalOrganisation;
    }

    if (this.organisation != null) {
      const organisationContactRelationship =
        this.allors.context.create<OrganisationContactRelationship>(
          this.m.OrganisationContactRelationship
        );
      organisationContactRelationship.Contact = this.object;
      organisationContactRelationship.Organisation = this.organisation;
      organisationContactRelationship.ContactKinds = this.selectedContactKinds;
    }

    super.save();
  }
}
