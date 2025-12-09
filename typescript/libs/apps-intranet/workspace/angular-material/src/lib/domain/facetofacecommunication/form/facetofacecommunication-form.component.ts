import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  CommunicationEventPurpose,
  CommunicationEventState,
  FaceToFaceCommunication,
  InternalOrganisation,
  Organisation,
  OrganisationContactRelationship,
  Party,
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
  templateUrl: './facetofacecommunication-form.component.html',
  providers: [ContextService],
})
export class FaceToFaceCommunicationFormComponent extends AllorsFormComponent<FaceToFaceCommunication> {
  readonly m: M;

  addFromParty = false;
  addToParty = false;

  person: Person;
  organisation: Organisation;
  purposes: CommunicationEventPurpose[];
  contacts: Party[] = [];
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
        p.FaceToFaceCommunication({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            FromParty: {
              CurrentPartyContactMechanisms: {
                ContactMechanism: {},
              },
            },
            ToParty: {
              CurrentPartyContactMechanisms: {
                ContactMechanism: {},
              },
            },
            EventPurposes: {},
            CommunicationEventState: {},
          },
        }),
        p.CommunicationEvent({
          objectId: this.editRequest.objectId,
          select: {
            InvolvedParties: {
              include: {
                CurrentContacts: {},
              },
            },
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
}
