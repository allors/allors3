import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult, IObject } from '@allors/system/workspace/domain';
import {
  CommunicationEventPurpose,
  CommunicationEventState,
  ContactMechanism,
  InternalOrganisation,
  LetterCorrespondence,
  Organisation,
  OrganisationContactRelationship,
  Party,
  PartyContactMechanism,
  Person,
  PostalAddress,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './lettercorrespondence-form.component.html',
  providers: [ContextService],
})
export class LetterCorrespondenceFormComponent extends AllorsFormComponent<LetterCorrespondence> {
  readonly m: M;

  addFromParty = false;
  addToParty = false;
  addFromAddress = false;
  addToAddress = false;

  party: Party;
  person: Person;
  organisation: Organisation;
  purposes: CommunicationEventPurpose[];
  fromPostalAddresses: ContactMechanism[] = [];
  toPostalAddresses: ContactMechanism[] = [];
  eventStates: CommunicationEventState[];
  contacts: Party[] = [];
  parties: Party[];
  organisationPullName: string;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;

    this.organisationPullName =
      'OrganisationContactRelationshipWhereOrganisation';
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      p.Organisation({
        objectId: this.internalOrganisationId.value,
        name: 'InternalOrganisation',
        include: {
          ActiveEmployees: {
            CurrentPartyContactMechanisms: {
              ContactMechanism: {
                PostalAddress_Country: {},
              },
            },
          },
        },
      })
    );
    pulls.push(
      p.CommunicationEventPurpose({
        predicate: {
          kind: 'Equals',
          propertyType: m.CommunicationEventPurpose.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.CommunicationEventPurpose.Name }],
      })
    );
    pulls.push(
      p.CommunicationEventState({
        sorting: [{ roleType: m.CommunicationEventState.Name }],
      })
    );

    if (this.editRequest) {
      pulls.push(
        p.LetterCorrespondence({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            FromParty: {
              PartyContactMechanismsWhereParty: {
                ContactMechanism: {},
              },
              CurrentPartyContactMechanisms: {
                ContactMechanism: {},
              },
            },
            ToParty: {
              PartyContactMechanismsWhereParty: {
                ContactMechanism: {},
              },
              CurrentPartyContactMechanisms: {
                ContactMechanism: {},
              },
            },
            PostalAddress: {
              Country: {},
            },
            EventPurposes: {},
            CommunicationEventState: {},
          },
        }),
        p.CommunicationEvent({
          objectId: this.editRequest.objectId,
          select: {
            InvolvedParties: {},
          },
        })
      );
    }

    const initializer = this.createRequest?.initializer;
    if (initializer) {
      pulls.push(
        p.Organisation({
          objectId: initializer.id,
          include: {
            CurrentContacts: {},
            CurrentPartyContactMechanisms: {
              ContactMechanism: {
                PostalAddress_Country: {},
              },
            },
          },
        }),
        p.Person({
          objectId: initializer.id,
        }),
        p.Person({
          name: this.organisationPullName,
          objectId: initializer.id,
          select: {
            OrganisationContactRelationshipsWhereContact: {
              Organisation: {
                include: {
                  CurrentContacts: {},
                  CurrentPartyContactMechanisms: {
                    ContactMechanism: {
                      PostalAddress_Country: {},
                    },
                  },
                },
              },
            },
          },
        })
      );
    }
  }

  onPostPull(pullResult: IPullResult): void {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.purposes = pullResult.collection<CommunicationEventPurpose>(
      this.m.CommunicationEventPurpose
    );
    this.eventStates = pullResult.collection<CommunicationEventState>(
      this.m.CommunicationEventState
    );
    this.parties = pullResult.collection<Party>(
      this.m.CommunicationEvent.InvolvedParties
    );

    this.person = pullResult.object<Person>(this.m.Person);
    this.organisation = pullResult.object<Organisation>(
      this.organisationPullName
    );

    if (this.createRequest) {
      this.party = this.organisation || this.person;
    } else {
      this.updateFromParty(this.object.FromParty);
      this.updateToParty(this.object.ToParty);
    }

    const internalOrganisation = pullResult.object<InternalOrganisation>(
      'InternalOrganisation'
    );

    const contacts = new Set<Party>();

    if (this.organisation) {
      contacts.add(this.organisation);
    }

    if (internalOrganisation.ActiveEmployees != null) {
      internalOrganisation.ActiveEmployees?.reduce(
        (c, e) => c.add(e),
        contacts
      );
    }

    if (this.organisation && this.organisation.CurrentContacts != null) {
      this.organisation.CurrentContacts?.reduce((c, e) => c.add(e), contacts);
    }

    if (this.person) {
      contacts.add(this.person);
    }

    if (this.parties) {
      this.parties?.reduce((c, e) => c.add(e), contacts);
    }

    this.contacts.push(...contacts);
    this.sortContacts();
  }

  public fromAddressAdded(partyContactMechanism: PartyContactMechanism): void {
    if (this.object.FromParty) {
      partyContactMechanism.Party = this.object.FromParty;
    }

    const address = partyContactMechanism.ContactMechanism as PostalAddress;

    this.fromPostalAddresses.push(address);
    this.object.PostalAddress = address;
  }

  public toAddressAdded(partyContactMechanism: PartyContactMechanism): void {
    if (this.object.ToParty) {
      partyContactMechanism.Party = this.object.ToParty;
    }

    const address = partyContactMechanism.ContactMechanism as PostalAddress;

    this.toPostalAddresses.push(address);
    this.object.PostalAddress = address;
  }

  public fromPartyAdded(fromParty: Person): void {
    this.addContactRelationship(fromParty);
    this.object.FromParty = fromParty;
    this.contacts.push(fromParty);
    this.sortContacts();
  }

  public toPartyAdded(toParty: Person): void {
    this.addContactRelationship(toParty);
    this.object.ToParty = toParty;
    this.contacts.push(toParty);
    this.sortContacts();
  }

  private addContactRelationship(party: Person): void {
    if (this.organisation) {
      const relationShip: OrganisationContactRelationship =
        this.allors.context.create<OrganisationContactRelationship>(
          this.m.OrganisationContactRelationship
        );
      relationShip.Contact = party;
      relationShip.Organisation = this.organisation;
    }
  }

  public fromPartySelected(party: IObject) {
    if (party) {
      this.updateFromParty(party as Party);
    }
  }

  private sortContacts(): void {
    this.contacts.sort((a, b) =>
      a.DisplayName > b.DisplayName ? 1 : b.DisplayName > a.DisplayName ? -1 : 0
    );
  }

  private updateFromParty(party: Party): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const pulls = [
      pull.Party({
        objectId: party.id,
        select: {
          PartyContactMechanismsWhereParty: {
            include: {
              ContactMechanism: {
                ContactMechanismType: x,
                PostalAddress_Country: x,
              },
            },
          },
        },
      }),
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
      const partyContactMechanisms: PartyContactMechanism[] =
        loaded.collection<PartyContactMechanism>(
          m.Party.PartyContactMechanismsWhereParty
        );
      this.fromPostalAddresses = partyContactMechanisms
        ?.filter(
          (v) => v.ContactMechanism.strategy.cls === this.m.PostalAddress
        )
        ?.map((v) => v.ContactMechanism);
    });
  }

  public toPartySelected(party: IObject) {
    if (party) {
      this.updateToParty(party as Party);
    }
  }

  private updateToParty(party: Party): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const pulls = [
      pull.Party({
        objectId: party.id,
        select: {
          PartyContactMechanismsWhereParty: {
            include: {
              ContactMechanism: {
                ContactMechanismType: x,
              },
            },
          },
        },
      }),
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
      const partyContactMechanisms: PartyContactMechanism[] =
        loaded.collection<PartyContactMechanism>(
          m.Party.PartyContactMechanismsWhereParty
        );
      this.toPostalAddresses = partyContactMechanisms
        ?.filter(
          (v) => v.ContactMechanism.strategy.cls === this.m.PostalAddress
        )
        ?.map((v) => v.ContactMechanism);
    });
  }
  public addressAdded(partyContactMechanism: PartyContactMechanism): void {
    partyContactMechanism.Party = this.party;

    const postalAddress =
      partyContactMechanism.ContactMechanism as PostalAddress;
    this.fromPostalAddresses.push(postalAddress);
    this.object.PostalAddress = postalAddress;
  }
}
