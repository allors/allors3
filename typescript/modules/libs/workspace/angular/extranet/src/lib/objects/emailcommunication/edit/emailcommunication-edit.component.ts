import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import {
  Person,
  Organisation,
  OrganisationContactRelationship,
  Party,
  InternalOrganisation,
  CommunicationEventPurpose,
  ContactMechanism,
  CommunicationEventState,
  PartyContactMechanism,
  EmailCommunication,
  EmailTemplate,
  EmailAddress,
} from '@allors/workspace/domain/default';
import { NavigationService, ObjectData, RefreshService, SaveService, TestScope } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';
import { IObject } from '@allors/workspace/domain/system';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './emailcommunication-edit.component.html',
  providers: [ContextService],
})
export class EmailCommunicationEditComponent extends TestScope implements OnInit, OnDestroy {
  readonly m: M;

  addFromParty = false;
  addToParty = false;
  addFromEmail = false;
  addToEmail = false;

  communicationEvent: EmailCommunication;
  party: Party;
  person: Person;
  organisation: Organisation;
  purposes: CommunicationEventPurpose[];
  contacts: Party[] = [];
  fromEmails: ContactMechanism[] = [];
  toEmails: ContactMechanism[] = [];
  title: string;

  emailTemplate: EmailTemplate;

  private subscription: Subscription;
  eventStates: CommunicationEventState[];
  parties: Party[];

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<EmailCommunicationEditComponent>,
    public refreshService: RefreshService,

    public navigation: NavigationService,
    private saveService: SaveService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super();

    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest(this.refreshService.refresh$, this.internalOrganisationId.observable$)
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id == null;

          let pulls = [
            pull.Organisation({
              objectId: this.internalOrganisationId.value,
              name: 'InternalOrganisation',
              include: {
                ActiveEmployees: {
                  CurrentPartyContactMechanisms: {
                    ContactMechanism: x,
                  },
                },
              },
            }),
            pull.CommunicationEventPurpose({
              predicate: { kind: 'Equals', propertyType: m.CommunicationEventPurpose.IsActive, value: true },
              sorting: [{ roleType: m.CommunicationEventPurpose.Name }],
            }),
            pull.CommunicationEventState({
              sorting: [{ roleType: m.CommunicationEventState.Name }],
            }),
          ];

          if (!isCreate) {
            pulls.push(
              pull.EmailCommunication({
                objectId: this.data.id,
                include: {
                  FromParty: {
                    PartyContactMechanisms: {
                      ContactMechanism: x,
                    },
                    CurrentPartyContactMechanisms: {
                      ContactMechanism: x,
                    },
                  },
                  ToParty: {
                    PartyContactMechanisms: {
                      ContactMechanism: x,
                    },
                    CurrentPartyContactMechanisms: {
                      ContactMechanism: x,
                    },
                  },
                  FromEmail: x,
                  ToEmail: x,
                  EmailTemplate: x,
                  EventPurposes: x,
                  CommunicationEventState: x,
                },
              }),
              pull.CommunicationEvent({
                objectId: this.data.id,
                select: {
                  InvolvedParties: x,
                },
              })
            );
          }

          if (isCreate && this.data.associationId) {
            pulls = [
              ...pulls,
              pull.Organisation({
                objectId: this.data.associationId,
                include: {
                  CurrentContacts: x,
                  CurrentPartyContactMechanisms: {
                    ContactMechanism: x,
                  },
                },
              }),
              pull.Person({
                objectId: this.data.associationId,
              }),
              pull.Person({
                objectId: this.data.associationId,
                select: {
                  OrganisationContactRelationshipsWhereContact: {
                    Organisation: {
                      include: {
                        CurrentContacts: x,
                        CurrentPartyContactMechanisms: {
                          ContactMechanism: x,
                        },
                      },
                    },
                  },
                },
              }),
            ];
          }

          return this.allors.context.pull(pulls).pipe(map((loaded) => ({ loaded, isCreate })));
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.context.reset();

        this.purposes = loaded.collection<CommunicationEventPurpose>(m.CommunicationEventPurpose);
        this.eventStates = loaded.collection<CommunicationEventState>(m.CommunicationEventState);
        this.parties = loaded.collection<Party>(m.CommunicationEvent.InvolvedParties);

        const internalOrganisation = loaded.object<InternalOrganisation>('InternalOrganisation');

        this.person = loaded.object<Person>(m.Person);
        this.organisation = loaded.object<Organisation>(m.OrganisationContactRelationship.Organisation);

        if (isCreate) {
          this.title = 'Add Email';
          this.communicationEvent = this.allors.context.create<EmailCommunication>(m.EmailCommunication);
          this.emailTemplate = this.allors.context.create<EmailTemplate>(m.EmailTemplate);
          this.communicationEvent.EmailTemplate = this.emailTemplate;

          this.party = this.organisation || this.person;
        } else {
          this.communicationEvent = loaded.object<EmailCommunication>(m.EmailCommunication);

          if (this.communicationEvent.FromParty) {
            this.updateFromParty(this.communicationEvent.FromParty);
          }

          if (this.communicationEvent.ToParty) {
            this.updateToParty(this.communicationEvent.ToParty);
          }

          if (this.communicationEvent.canWriteActualEnd) {
            this.title = 'Edit Email';
          } else {
            this.title = 'View Email';
          }
        }

        const contacts = new Set<Party>();

        if (this.organisation) {
          contacts.add(this.organisation);
        }

        if (internalOrganisation.ActiveEmployees != null) {
          internalOrganisation.ActiveEmployees?.reduce((c, e) => c.add(e), contacts);
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
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public fromEmailAdded(partyContactMechanism: PartyContactMechanism): void {
    if (this.communicationEvent.FromParty) {
      this.communicationEvent.FromParty.addPartyContactMechanism(partyContactMechanism);
    }

    const emailAddress = partyContactMechanism.ContactMechanism as EmailAddress;

    this.fromEmails.push(emailAddress);
    this.communicationEvent.FromEmail = emailAddress;
  }

  public toEmailAdded(partyContactMechanism: PartyContactMechanism): void {
    if (this.communicationEvent.ToParty) {
      this.communicationEvent.ToParty.addPartyContactMechanism(partyContactMechanism);
    }

    const emailAddress = partyContactMechanism.ContactMechanism as EmailAddress;

    this.toEmails.push(emailAddress);
    this.communicationEvent.FromEmail = emailAddress;
  }

  public fromPartyAdded(fromParty: Person): void {
    this.addContactRelationship(fromParty);
    this.communicationEvent.FromParty = fromParty;
    this.contacts.push(fromParty);
    this.sortContacts();
  }

  public toPartyAdded(toParty: Person): void {
    this.addContactRelationship(toParty);
    this.communicationEvent.ToParty = toParty;
    this.contacts.push(toParty);
    this.sortContacts();
  }

  public fromPartySelected(party: IObject) {
    if (party) {
      this.updateFromParty(party as Party);
    }
  }

  private sortContacts(): void {
    this.contacts.sort((a, b) => (a.DisplayName > b.DisplayName ? 1 : b.DisplayName > a.DisplayName ? -1 : 0));
  }

  private addContactRelationship(party: Person): void {
    if (this.organisation) {
      const relationShip: OrganisationContactRelationship = this.allors.context.create<OrganisationContactRelationship>(this.m.OrganisationContactRelationship);
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
      const partyContactMechanisms: PartyContactMechanism[] = loaded.collection<PartyContactMechanism>(m.Party.PartyContactMechanisms);
      this.fromEmails = partyContactMechanisms?.filter((v) => v.ContactMechanism.strategy.cls === this.m.EmailAddress)?.map((v) => v.ContactMechanism);
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
      const partyContactMechanisms: PartyContactMechanism[] = loaded.collection<PartyContactMechanism>(m.Party.PartyContactMechanisms);
      this.toEmails = partyContactMechanisms?.filter((v) => v.ContactMechanism.strategy.cls === this.m.EmailAddress)?.map((v) => v.ContactMechanism);
    });
  }

  public save(): void {
    this.allors.context.push().subscribe(() => {
      this.dialogRef.close(this.communicationEvent);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }
}
