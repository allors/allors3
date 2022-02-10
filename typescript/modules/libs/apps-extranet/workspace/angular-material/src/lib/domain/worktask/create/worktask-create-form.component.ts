import { Component, Self } from '@angular/core';

import {
  Person,
  ContactMechanism,
  WorkTask,
} from '@allors/default/workspace/domain';
import {
  ErrorService,
  UserId,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { NgForm } from '@angular/forms';
import { IObject, IPullResult, Pull } from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';

@Component({
  selector: 'worktask-create-form',
  templateUrl: './worktask-create-form.component.html',
  providers: [ContextService],
})
export class WorkTaskCreateFormComponent extends AllorsFormComponent<WorkTask> {
  m: M;

  contactMechanisms: ContactMechanism[];
  contacts: Person[];

  constructor(
    @Self() private allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private userId: UserId
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      p.Organisation({
        predicate: {
          kind: 'ContainedIn',
          propertyType: m.Organisation.CurrentOrganisationContactRelationships,
          extent: {
            kind: 'Filter',
            objectType: m.OrganisationContactRelationship,
            predicate: {
              kind: 'Equals',
              propertyType: m.OrganisationContactRelationship.Contact,
              value: this.userId.value,
            },
          },
        },
        results: [
          {
            name: 'contactmechanisms',
            select: {
              PartyContactMechanisms: {},
              CurrentPartyContactMechanisms: {
                include: {
                  ContactMechanism: {
                    PostalAddress_Country: {},
                  },
                },
              },
            },
          },
          {
            name: 'contacts',
            select: {
              CurrentContacts: {},
            },
          },
        ],
      })
    );

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.onPostPullInitialize(pullResult);

    this.contactMechanisms =
      pullResult.collection<ContactMechanism>('contactmechanisms');
    this.contacts = pullResult.collection<Person>('contacts');
  }
}
