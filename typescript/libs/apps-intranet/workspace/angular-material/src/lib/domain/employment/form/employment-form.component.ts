import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  Employment,
  InternalOrganisation,
  Organisation,
  Person,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
  SearchFactory,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { FetcherService } from '../../../services/fetcher/fetcher-service';

@Component({
  templateUrl: './employment-form.component.html',
  providers: [ContextService],
})
export class EmploymentFormComponent extends AllorsFormComponent<Employment> {
  readonly m: M;
  internalOrganisation: InternalOrganisation;
  addEmployee = false;
  organisationsFilter: SearchFactory;
  peopleFilter: SearchFactory;
  employee: Person;
  employer: Organisation;
  person: Person;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;

    this.organisationsFilter = new SearchFactory({
      objectType: this.m.Organisation,
      roleTypes: [this.m.Organisation.Name],
    });

    this.peopleFilter = new SearchFactory({
      objectType: this.m.Person,
      roleTypes: [this.m.Person.FirstName, this.m.Person.LastName],
    });
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(this.fetcher.internalOrganisation);

    if (this.editRequest) {
      pulls.push(
        p.Employment({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            Employee: {},
            Employer: {},
          },
        })
      );
    }

    const initializer = this.createRequest?.initializer;
    if (initializer) {
      pulls.push(
        p.Person({
          objectId: initializer.id,
        })
      );
    }
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.person = pullResult.object<Person>(this.m.Person);
    this.internalOrganisation =
      this.fetcher.getInternalOrganisation(pullResult);

    if (this.createRequest) {
      this.object.FromDate = new Date();
      this.object.Employee = this.person;
      this.object.Employer = this.internalOrganisation;
    }
  }

  public employeeAdded(employee: Person): void {
    this.object.Employee = employee;
  }
}
