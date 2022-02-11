import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  Enumeration,
  TelecommunicationsNumber,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

@Component({
  templateUrl: './telecommunicationsnumber-edit-form.component.html',
  providers: [ContextService],
})
export class TelecommunicationsNumberFormComponent extends AllorsFormComponent<TelecommunicationsNumber> {
  readonly m: M;
  contactMechanismTypes: Enumeration[];

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
      p.ContactMechanismType({
        predicate: {
          kind: 'Equals',
          propertyType: m.ContactMechanismType.IsActive,
          value: true,
        },
        sorting: [{ roleType: this.m.ContactMechanismType.Name }],
      })
    );

    if (this.editRequest) {
      pulls.push(
        p.TelecommunicationsNumber({
          name: '_object',
          objectId: this.editRequest.objectId,
        })
      );
    }

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.onPostPullInitialize(pullResult);
  }
}
