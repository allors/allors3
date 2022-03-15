import { Component, Self } from '@angular/core';

import {
  ErrorService,
  AllorsFormComponent,
  UserId,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { IPullResult, Pull } from '@allors/system/workspace/domain';
import { NgForm } from '@angular/forms';
import {
  ContactMechanism,
  Person,
  WorkTask,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';

@Component({
  selector: 'worktask-edit-form',
  templateUrl: './worktask-edit-form.component.html',
  providers: [ContextService],
})
export class WorkTaskEditFormComponent extends AllorsFormComponent<WorkTask> {
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
      p.WorkTask({
        name: '_object',
        objectId: this.editRequest.objectId,
        include: {
          WorkEffortState: {},
          CreatedBy: {},
        },
      }),
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
              PartyContactMechanismsWhereParty: {},
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

  onPostPull(pullResult: IPullResult): void {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.onPostPullInitialize(pullResult);

    this.contactMechanisms =
      pullResult.collection<ContactMechanism>('contactmechanisms');
    this.contacts = pullResult.collection<Person>('contacts');
  }
}
