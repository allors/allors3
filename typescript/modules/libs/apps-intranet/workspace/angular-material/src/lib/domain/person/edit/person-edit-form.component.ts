import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  Currency,
  EmailAddress,
  Enumeration,
  GenderType,
  InternalOrganisation,
  Locale,
  PartyContactMechanism,
  Person,
  Salutation,
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
  selector: 'person-edit-form',
  templateUrl: './person-edit-form.component.html',
  providers: [ContextService],
})
export class PersonEditFormComponent extends AllorsFormComponent<Person> {
  readonly m: M;
  emailAddresses: string[] = [];

  internalOrganisation: InternalOrganisation;
  locales: Locale[];
  genders: Enumeration[];
  salutations: Enumeration[];
  public confirmPassword: string;

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
      p.Person({
        name: '_object',
        objectId: this.editRequest.objectId,
        include: {
          PreferredCurrency: {},
          Gender: {},
          Salutation: {},
          Locale: {},
          Picture: {},
        },
      }),
      p.Person({
        objectId: this.editRequest.objectId,
        select: {
          PartyContactMechanismsWhereParty: {
            include: {
              ContactMechanism: {
                ContactMechanismType: {},
              },
            },
          },
        },
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
    this.genders = pullResult.collection<GenderType>(this.m.GenderType);
    this.salutations = pullResult.collection<Salutation>(this.m.Salutation);

    const partyContactMechanisms: PartyContactMechanism[] =
      pullResult.collection<PartyContactMechanism>(
        this.m.Party.PartyContactMechanismsWhereParty
      );
    this.emailAddresses =
      partyContactMechanisms
        ?.filter((v) => v.ContactMechanism.strategy.cls === this.m.EmailAddress)
        ?.map((v) => v.ContactMechanism)
        .map((v: EmailAddress) => v.ElectronicAddressString) ?? [];
  }

  private onSave(): void {
    if (
      this.object.UserEmail != null &&
      this.emailAddresses.indexOf(this.object.UserEmail) == -1
    ) {
      const emailAddress = this.allors.context.create<EmailAddress>(
        this.m.EmailAddress
      );
      emailAddress.ElectronicAddressString = this.object.UserEmail;

      const partyContactMechanism =
        this.allors.context.create<PartyContactMechanism>(
          this.m.PartyContactMechanism
        );
      partyContactMechanism.ContactMechanism = emailAddress;

      partyContactMechanism.Party = this.object;
      this.emailAddresses.push(this.object.UserEmail);
    }
  }

  public override save(): void {
    this.onSave();

    super.save();
  }
}
