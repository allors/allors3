import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult, IObject } from '@allors/system/workspace/domain';
import {
  ContactMechanism,
  Currency,
  CustomerRelationship,
  InternalOrganisation,
  Organisation,
  OrganisationContactRelationship,
  Party,
  PartyContactMechanism,
  Person,
  Quote,
  RequestForQuote,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
  SearchFactory,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { Filters } from '../../../filters/filters';

@Component({
  selector: 'requestforquote-edit-form',
  templateUrl: './requestforquote-edit-form.component.html',
  providers: [ContextService],
})
export class RequestForQuoteEditFormComponent extends AllorsFormComponent<RequestForQuote> {
  readonly m: M;

  quote: Quote;
  currencies: Currency[];
  contactMechanisms: ContactMechanism[] = [];
  contacts: Person[] = [];
  internalOrganisation: InternalOrganisation;

  addContactPerson = false;
  addContactMechanism = false;
  addOriginator = false;
  previousOriginator: Party;

  customersFilter: SearchFactory;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;

    this.customersFilter = Filters.customersFilter(
      this.m,
      this.internalOrganisationId.value
    );
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      this.fetcher.internalOrganisation,
      p.Currency({ sorting: [{ roleType: m.Currency.Name }] }),
      p.RequestForQuote({
        name: '_object',
        objectId: this.editRequest.objectId,
        include: {
          FullfillContactMechanism: {
            PostalAddress_Country: {},
          },
          RequestItems: {
            Product: {},
          },
          Originator: {},
          ContactPerson: {},
          RequestState: {},
          DerivedCurrency: {},
          CreatedBy: {},
          LastModifiedBy: {},
        },
      }),
      p.RequestForQuote({
        objectId: this.editRequest.objectId,
        select: {
          QuoteWhereRequest: {},
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

    if (this.object.Originator) {
      this.previousOriginator = this.object.Originator;
      this.update(this.object.Originator);
    }
  }

  get originatorIsPerson(): boolean {
    return (
      !this.object.Originator ||
      this.object.Originator.strategy.cls === this.m.Person
    );
  }

  public originatorSelected(party: IObject) {
    if (party) {
      this.update(party as Party);
    }
  }

  public partyContactMechanismAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.contactMechanisms.push(partyContactMechanism.ContactMechanism);
    partyContactMechanism.Party = this.object.Originator;
    this.object.FullfillContactMechanism =
      partyContactMechanism.ContactMechanism;
  }

  public personAdded(person: Person): void {
    const organisationContactRelationship =
      this.allors.context.create<OrganisationContactRelationship>(
        this.m.OrganisationContactRelationship
      );
    organisationContactRelationship.Organisation = this.object
      .Originator as Organisation;
    organisationContactRelationship.Contact = person;

    this.contacts.push(person);
    this.object.ContactPerson = person;
  }

  public originatorAdded(party: Party): void {
    const customerRelationship =
      this.allors.context.create<CustomerRelationship>(
        this.m.CustomerRelationship
      );
    customerRelationship.Customer = party;
    customerRelationship.InternalOrganisation = this.internalOrganisation;

    this.object.Originator = party;
  }

  private update(party: Party) {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const pulls = [
      pull.Party({
        objectId: party.id,
        select: {
          PartyContactMechanismsWhereParty: x,
          CurrentPartyContactMechanisms: {
            include: {
              ContactMechanism: {
                PostalAddress_Country: x,
              },
            },
          },
        },
      }),
      pull.Party({
        objectId: party.id,
        select: {
          CurrentContacts: x,
        },
      }),
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
      if (this.object.Originator !== this.previousOriginator) {
        this.object.FullfillContactMechanism = null;
        this.object.ContactPerson = null;
        this.previousOriginator = this.object.Originator;
      }

      const partyContactMechanisms: PartyContactMechanism[] =
        loaded.collection<PartyContactMechanism>(
          m.Party.CurrentPartyContactMechanisms
        );
      this.contactMechanisms = partyContactMechanisms?.map(
        (v: PartyContactMechanism) => v.ContactMechanism
      );
      this.contacts = loaded.collection<Person>(m.Party.CurrentContacts);
    });
  }
}
