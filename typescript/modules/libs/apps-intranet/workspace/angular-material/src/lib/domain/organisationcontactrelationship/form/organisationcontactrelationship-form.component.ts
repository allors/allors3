import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  Organisation,
  OrganisationContactKind,
  OrganisationContactRelationship,
  Party,
  Person,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
  SearchFactory,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { Filters } from '../../../filters/filters';

@Component({
  templateUrl: './organisationcontactrelationship-form.component.html',
  providers: [ContextService],
})
export class OrganisationContactRelationshipFormComponent extends AllorsFormComponent<OrganisationContactRelationship> {
  readonly m: M;
  addContact = false;
  party: Party;
  person: Person;
  organisation: Organisation;
  contactKinds: OrganisationContactKind[];
  generalContact: OrganisationContactKind;

  peopleFilter: SearchFactory;
  organisationsFilter: SearchFactory;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;

    this.peopleFilter = Filters.peopleFilter(this.m);
    this.organisationsFilter = Filters.organisationsFilter(this.m);
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      p.OrganisationContactKind({
        sorting: [{ roleType: this.m.OrganisationContactKind.Description }],
      })
    );

    if (this.editRequest) {
      pulls.push(
        p.OrganisationContactRelationship({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            ContactKinds: {},
            Organisation: {},
            Contact: {},
            Parties: {},
          },
        })
      );
    }

    const initializer = this.createRequest?.initializer;
    if (initializer) {
      pulls.push(
        p.Party({
          objectId: initializer.id,
        })
      );
    }
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.contactKinds = pullResult.collection<OrganisationContactKind>(
      this.m.OrganisationContactKind
    );

    this.generalContact = this.contactKinds?.find(
      (v) => v.UniqueId === 'eebe4d65-c452-49c9-a583-c0ffec385e98'
    );

    if (this.createRequest) {
      this.object.FromDate = new Date();
      this.object.addContactKind(this.generalContact);

      this.party = pullResult.object<Party>(this.m.Party);

      if (this.party.strategy.cls === this.m.Person) {
        this.person = this.party as Person;
        this.object.Contact = this.person;
      }

      if (this.party.strategy.cls === this.m.Organisation) {
        this.organisation = this.party as Organisation;
        this.object.Organisation = this.organisation;
      }
    } else {
      this.person = this.object.Contact;
      this.organisation = this.object.Organisation as Organisation;
    }
  }

  public contactAdded(contact: Person): void {
    this.object.Contact = contact;
  }
}
