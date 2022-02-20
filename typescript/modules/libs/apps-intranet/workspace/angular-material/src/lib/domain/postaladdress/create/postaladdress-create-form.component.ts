import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  ContactMechanismPurpose,
  Country,
  Enumeration,
  Party,
  PartyContactMechanism,
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
  templateUrl: './postaladdress-create-form.component.html',
  providers: [ContextService],
})
export class PostalAddressCreateFormComponent extends AllorsFormComponent<PostalAddress> {
  readonly m: M;

  countries: Country[];
  party: Party;
  contactMechanismPurposes: Enumeration[];
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
      p.Country({
        sorting: [{ roleType: m.Country.Name }],
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
          include: { PartyContactMechanismsWhereParty: {} },
        })
      );
    }
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.context.create(this.createRequest.objectType);

    this.countries = pullResult.collection<Country>(this.m.Country);
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
    this.partyContactMechanism.Party = this.party;
  }
}
