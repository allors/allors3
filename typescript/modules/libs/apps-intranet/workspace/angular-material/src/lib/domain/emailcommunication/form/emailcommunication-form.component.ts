import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import {
  EditIncludeHandler,
  Node,
  CreateOrEditPullHandler,
  Pull,
  IPullResult,
  PostCreatePullHandler,
  IObject,
} from '@allors/system/workspace/domain';
import {
  BasePrice,
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
import {
  PreEditPullHandler,
  PostEditPullHandler,
} from '../../../../../../../../system/workspace/domain/src/lib/pull/edit-pull-handler';

@Component({
  templateUrl: './emailcommunication-form.component.html',
  providers: [ContextService],
})
export class EmailCommunicationFormComponent
  extends AllorsFormComponent<EmailCommunication>
  implements
    CreateOrEditPullHandler,
    EditIncludeHandler,
    PostCreatePullHandler,
    PreEditPullHandler,
    PostEditPullHandler
{
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

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  onPreCreateOrEditPull(pulls: Pull[]): void {
    const m = this.m;
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
  }

  onPreEditPull(_, pulls: Pull[]): void {
    const m = this.m;
    const { pullBuilder: p } = m;

    pulls.push(
      p.CommunicationEvent({
        objectId: this.object.id,
        select: {
          InvolvedParties: {},
        },
      })
    );
  }

  onEditInclude(): Node[] {
    const { treeBuilder: t } = this.m;

    return t.EmailCommunication({
      FromParty: {
        PartyContactMechanisms: {
          ContactMechanism: {},
        },
        CurrentPartyContactMechanisms: {
          ContactMechanism: {},
        },
      },
      ToParty: {
        PartyContactMechanisms: {
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
    });
  }

  onPostCreatePull(): void {
    this.emailTemplate = this.allors.context.create<EmailTemplate>(
      this.m.EmailTemplate
    );
    this.object.EmailTemplate = this.emailTemplate;
  }

  onPostCreateOrEditPull(_, loaded: IPullResult): void {
    this.purposes = loaded.collection<CommunicationEventPurpose>(
      this.m.CommunicationEventPurpose
    );
    this.eventStates = loaded.collection<CommunicationEventState>(
      this.m.CommunicationEventState
    );
    this.parties = loaded.collection<Party>(
      this.m.CommunicationEvent.InvolvedParties
    );

    this.person = loaded.object<Person>(this.m.Person);
    this.organisation = loaded.collection<Organisation>(
      this.m.OrganisationContactRelationship.Organisation
    )[0];

    const internalOrganisation = loaded.object<InternalOrganisation>(
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

  onPostEditPull(): void {
    if (this.object.FromParty) {
      this.updateFromParty(this.object.FromParty);
    }

    if (this.object.ToParty) {
      this.updateToParty(this.object.ToParty);
    }
  }

  public fromEmailAdded(partyContactMechanism: PartyContactMechanism): void {
    if (this.object.FromParty) {
      this.object.FromParty.addPartyContactMechanism(partyContactMechanism);
    }

    const emailAddress = partyContactMechanism.ContactMechanism as EmailAddress;

    this.fromEmails.push(emailAddress);
    this.object.FromEmail = emailAddress;
  }

  public toEmailAdded(partyContactMechanism: PartyContactMechanism): void {
    if (this.object.ToParty) {
      this.object.ToParty.addPartyContactMechanism(partyContactMechanism);
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
          PartyContactMechanisms: {
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
          m.Party.PartyContactMechanisms
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
          PartyContactMechanisms: {
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
          m.Party.PartyContactMechanisms
        );
      this.toEmails = partyContactMechanisms
        ?.filter((v) => v.ContactMechanism.strategy.cls === this.m.EmailAddress)
        ?.map((v) => v.ContactMechanism);
    });
  }

  // TODO: KOEN
  // Pre
  // if (isCreate && this.data.associationId) {
  //   pulls = [
  //     ...pulls,
  //     pull.Organisation({
  //       object: this.data.associationId,
  //       include: {
  //         CurrentContacts: x,
  //         CurrentPartyContactMechanisms: {
  //           ContactMechanism: x,
  //         }
  //       }
  //     }),
  //     pull.Person({
  //       object: this.data.associationId,
  //     }),
  //     pull.Person({
  //       object: this.data.associationId,
  //       fetch: {
  //         OrganisationContactRelationshipsWhereContact: {
  //           Organisation: {
  //             include: {
  //               CurrentContacts: x,
  //               CurrentPartyContactMechanisms: {
  //                 ContactMechanism: x,
  //               }
  //             }
  //           }
  //         }
  //       }
  //     })
  //   ];
  // }
}
