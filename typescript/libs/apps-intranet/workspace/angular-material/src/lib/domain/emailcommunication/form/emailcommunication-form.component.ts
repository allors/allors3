import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult, IObject } from '@allors/system/workspace/domain';
import {
  CommunicationEventPurpose,
  CommunicationEventState,
  ContactMechanism,
  EmailAddress,
  EmailCommunication,
  EmailTemplate,
  InternalOrganisation,
  Organisation,
  OrganisationContactRelationship,
  Party,
  PartyContactMechanism,
  Person,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './emailcommunication-form.component.html',
  providers: [ContextService],
})
export class EmailCommunicationFormComponent extends AllorsFormComponent<EmailCommunication> {
  readonly m: M;

  addFromParty = false;
  addToParty = false;
  addFromEmail = false;
  addToEmail = false;

  person: Person;
  organisation: Organisation;
  purposes: CommunicationEventPurpose[];
  contacts: Party[] = [];
  fromEmails: ContactMechanism[] = [];
  toEmails: ContactMechanism[] = [];
  emailTemplate: EmailTemplate;
  eventStates: CommunicationEventState[];
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
              ContactMechanism: {},
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
        p.EmailCommunication({
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
            FromEmail: {},
            ToEmail: {},
            EmailTemplate: {},
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
              ContactMechanism: {},
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
                    ContactMechanism: {},
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

    if (this.createRequest) {
      this.emailTemplate = this.allors.context.create<EmailTemplate>(
        this.m.EmailTemplate
      );
      this.object.EmailTemplate = this.emailTemplate;
    } else {
      if (this.object.FromParty) {
        this.updateFromParty(this.object.FromParty);
      }

      if (this.object.ToParty) {
        this.updateToParty(this.object.ToParty);
      }
    }

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

  public fromEmailAdded(partyContactMechanism: PartyContactMechanism): void {
    if (this.object.FromParty) {
      partyContactMechanism.Party = this.object.FromParty;
    }

    const emailAddress = partyContactMechanism.ContactMechanism as EmailAddress;

    this.fromEmails.push(emailAddress);
    this.object.FromEmail = emailAddress;
  }

  public toEmailAdded(partyContactMechanism: PartyContactMechanism): void {
    if (this.object.ToParty) {
      partyContactMechanism.Party = this.object.ToParty;
    }

    const emailAddress = partyContactMechanism.ContactMechanism as EmailAddress;

    this.toEmails.push(emailAddress);
    this.object.FromEmail = emailAddress;
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
      this.fromEmails = partyContactMechanisms
        ?.filter((v) => v.ContactMechanism.strategy.cls === this.m.EmailAddress)
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
      this.toEmails = partyContactMechanisms
        ?.filter((v) => v.ContactMechanism.strategy.cls === this.m.EmailAddress)
        ?.map((v) => v.ContactMechanism);
    });
  }
}
