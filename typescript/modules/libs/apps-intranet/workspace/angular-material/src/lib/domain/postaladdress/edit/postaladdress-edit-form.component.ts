import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  Country,
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

@Component({
  templateUrl: './postaladdress-edit-form.component.html',
  providers: [ContextService],
})
export class PostalAddressEditFormComponent extends AllorsFormComponent<PostalAddress> {
  readonly m: M;
  countries: Country[];
  partyContactMechanism: PartyContactMechanism;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm
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
      })
    );

    if (this.editRequest) {
      pulls.push(
        p.PostalAddress({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            Country: {},
          },
        })
      );
    }

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);
      
      this.countries = pullResult.collection<Country>(this.m.Country);

    this.onPostPullInitialize(pullResult);
  }
}
