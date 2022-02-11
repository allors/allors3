import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  ContactMechanismPurpose,
  ContactMechanismType,
  Enumeration,
  Party,
  PartyContactMechanism,
  TelecommunicationsNumber,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './telecommunicationsnumber-create-form.component.html',
  providers: [ContextService],
})
export class TelecommunicationsNumberCreateFormComponent extends AllorsFormComponent<TelecommunicationsNumber> {
  readonly m: M;

  contactMechanismTypes: Enumeration[];
  contactMechanismPurposes: Enumeration[];
  party: Party;
  partyContactMechanism: PartyContactMechanism;

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
      p.ContactMechanismType({
        predicate: {
          kind: 'Equals',
          propertyType: m.ContactMechanismType.IsActive,
          value: true,
        },
        sorting: [{ roleType: this.m.ContactMechanismType.Name }],
      }),
      p.ContactMechanismPurpose({
        predicate: {
          kind: 'Equals',
          propertyType: m.ContactMechanismPurpose.IsActive,
          value: true,
        },
        sorting: [{ roleType: this.m.ContactMechanismPurpose.Name }],
      })
    );

    const initializer = this.createRequest.initializer;
    if (initializer) {
      pulls.push(
        p.Party({
          objectId: initializer.id,
          include: { PartyContactMechanisms: {} },
        })
      );
    }
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.context.create(this.createRequest.objectType);

    this.contactMechanismTypes = pullResult.collection<ContactMechanismType>(
      this.m.ContactMechanismType
    );
    this.contactMechanismPurposes =
      pullResult.collection<ContactMechanismPurpose>(
        this.m.ContactMechanismPurpose
      );

    this.party = pullResult.object<Party>(this.m.Party);

    this.partyContactMechanism =
      this.allors.context.create<PartyContactMechanism>(
        this.m.PartyContactMechanism
      );

    this.partyContactMechanism.UseAsDefault = true;
    this.partyContactMechanism.ContactMechanism = this.object;

    this.party.addPartyContactMechanism(this.partyContactMechanism);
  }
}
