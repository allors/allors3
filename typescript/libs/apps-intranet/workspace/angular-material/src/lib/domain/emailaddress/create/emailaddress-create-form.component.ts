import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  ContactMechanismPurpose,
  EmailAddress,
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
  templateUrl: './emailaddress-create-form.component.html',
  providers: [ContextService],
})
export class EmailAddressCreateFormComponent extends AllorsFormComponent<EmailAddress> {
  readonly m: M;
  contactMechanismTypes: Enumeration[];
  partyContactMechanism: PartyContactMechanism;
  party: Party;
  contactMechanismPurposes: Enumeration[];

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

    const initializer = this.createRequest?.initializer;
    if (initializer) {
      pulls.push(
        p.Party({
          objectId: initializer.id,
          include: {
            PartyContactMechanismsWhereParty: {},
          },
        })
      );
    }
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.context.create(this.createRequest.objectType);

    this.party = pullResult.object<Party>(this.m.Party);
    this.contactMechanismPurposes =
      pullResult.collection<ContactMechanismPurpose>(
        this.m.ContactMechanismPurpose
      );

    this.partyContactMechanism =
      this.allors.context.create<PartyContactMechanism>(
        this.m.PartyContactMechanism
      );

    this.partyContactMechanism.UseAsDefault = true;
    this.partyContactMechanism.ContactMechanism = this.object;

    this.partyContactMechanism.Party = this.party;
  }
}
