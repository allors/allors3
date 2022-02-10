import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import {
  EditIncludeHandler,
  Node,
  CreateOrEditPullHandler,
  Pull,
  IPullResult,
  PostCreatePullHandler,
} from '@allors/system/workspace/domain';
import {
  BasePrice,
  Employment,
  InternalOrganisation,
  Organisation,
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

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { IObject } from '../../../../../../../../system/workspace/domain/src/lib/iobject';

@Component({
  templateUrl: './employment-form.component.html',
  providers: [ContextService],
})
export class EmploymentFormComponent
  extends AllorsFormComponent<Employment>
  implements CreateOrEditPullHandler, EditIncludeHandler, PostCreatePullHandler
{
  readonly m: M;
  internalOrganisation: InternalOrganisation;
  internalOrganisations: InternalOrganisation[];
  addEmployee = false;
  organisationsFilter: SearchFactory;
  peopleFilter: SearchFactory;
  employee: Person;
  employer: Organisation;

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

  onPreCreateOrEditPull(pulls: Pull[]): void {
    const m = this.m;
    const { pullBuilder: p } = m;

    pulls.push(this.fetcher.internalOrganisation);
  }

  onEditInclude(): Node[] {
    const { treeBuilder: t } = this.m;

    return t.Employment({
      Employee: {},
      Employer: {},
    });
  }

  onPostCreatePull(_, loaded: IPullResult): void {
    this.object.FromDate = new Date();

    const party = loaded.object<Party>(this.m.Party);

    // TODO KOEN
    if (party.strategy.cls === this.m.Person) {
      this.employee = party as Person;
    }

    if (party.strategy.cls === this.m.Organisation) {
      this.employer = party as Organisation;
    }
  }

  onPostCreateOrEditPull(_, loaded: IPullResult): void {
    this.internalOrganisation = this.fetcher.getInternalOrganisation(loaded);
  }

  public employeeAdded(employee: Person): void {
    this.object.Employee = employee;
  }

  // TODO: KOEN
  // Pre
  // if (isCreate && this.data.associationId) {
  //   pulls.push(
  //     pull.Party({
  //       objectId: this.data.associationId,
  //     })
  //   );
  // }
}
