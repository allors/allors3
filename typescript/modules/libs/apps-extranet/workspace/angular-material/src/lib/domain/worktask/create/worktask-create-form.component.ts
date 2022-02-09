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
import {
  CreatePullHandler,
  IObject,
  IPullResult,
  Pull,
} from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';

@Component({
  selector: 'worktask-create-form',
  templateUrl: './worktask-create-form.component.html',
  providers: [ContextService],
})
export class WorkTaskCreateFormComponent
  extends AllorsFormComponent<WorkTask>
  implements CreatePullHandler
{
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

  onPreCreatePull(pulls: Pull[]): void {
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
  }

  onPostCreatePull(object: IObject, loaded: IPullResult): void {
    this.contactMechanisms =
      loaded.collection<ContactMechanism>('contactmechanisms');
    this.contacts = loaded.collection<Person>('contacts');
  }
}
