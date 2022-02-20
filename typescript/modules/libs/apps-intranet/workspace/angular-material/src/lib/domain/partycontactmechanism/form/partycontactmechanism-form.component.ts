import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  ContactMechanism,
  ContactMechanismPurpose,
  Enumeration,
  Party,
  PartyContactMechanism,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './partycontactmechanism-form.component.html',
  providers: [ContextService],
})
export class PartyContactmechanismFormComponent extends AllorsFormComponent<PartyContactMechanism> {
  readonly m: M;
  contactMechanismPurposes: Enumeration[];
  contactMechanisms: ContactMechanism[] = [];
  organisationContactMechanisms: ContactMechanism[];
  ownContactMechanisms: ContactMechanism[] = [];
  party: Party;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      p.ContactMechanismPurpose({
        predicate: {
          kind: 'Equals',
          propertyType: m.ContactMechanismPurpose.IsActive,
          value: true,
        },
        sorting: [{ roleType: this.m.ContactMechanismPurpose.Name }],
      })
    );

    if (this.editRequest) {
      pulls.push(
        p.PartyContactMechanism({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            ContactMechanism: {},
          },
        }),
        p.PartyContactMechanism({
          objectId: this.editRequest.objectId,
          select: {
            Party: {
              include: {
                CurrentPartyContactMechanisms: {
                  ContactMechanism: {},
                },
              },
            },
          },
        })
      );
    }

    const initializer = this.createRequest.initializer;
    if (initializer) {
      pulls.push(
        p.Party({
          objectId: initializer.id,
          include: {
            CurrentPartyContactMechanisms: {
              ContactMechanism: {},
            },
          },
        }),
        p.Person({
          objectId: initializer.id,
          select: {
            CurrentOrganisationContactMechanisms: {},
          },
        })
      );
    }
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.contactMechanisms = [];
    this.ownContactMechanisms = [];

    this.contactMechanismPurposes =
      pullResult.collection<ContactMechanismPurpose>(
        this.m.ContactMechanismPurpose
      );

    this.party =
      pullResult.object<Party>(this.m.Party) ||
      pullResult.object<Party>('PARTYWHEREPARTYCONTACTMECHANISM');

    this.organisationContactMechanisms =
      pullResult.collection<ContactMechanism>(
        this.m.Person.CurrentOrganisationContactMechanisms
      );

    this.party.CurrentPartyContactMechanisms.forEach((v) =>
      this.ownContactMechanisms.push(v.ContactMechanism)
    );

    if (this.organisationContactMechanisms != null) {
      this.contactMechanisms = this.contactMechanisms.concat(
        this.organisationContactMechanisms
      );
    }

    if (this.ownContactMechanisms != null) {
      this.contactMechanisms = this.contactMechanisms.concat(
        this.ownContactMechanisms
      );
    }

    if (this.createRequest) {
      this.object.FromDate = new Date();
      this.object.UseAsDefault = true;
      this.object.Party = this.party;
    }
  }
}
