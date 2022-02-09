import { Component, Self } from '@angular/core';
import { Employment } from '@allors/default/workspace/domain';
import {
  AllorsFormComponent,
  ContextService,
} from '@allors/base/workspace/angular/foundation';
import {
  ErrorService,
  SearchFactory,
} from '@allors/base/workspace/angular/foundation';
import { NgForm } from '@angular/forms';
import {
  CreatePullHandler,
  EditIncludeHandler,
  Initializer,
  Node,
  PostCreatePullHandler,
  Pull,
} from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import { PullResult } from 'libs/system/workspace/adapters-json/src/lib/database/pull/pull-result';

@Component({
  templateUrl: './employment-form.component.html',
  providers: [ContextService],
})
export class EmploymentFormComponent
  extends AllorsFormComponent<Employment>
  implements CreatePullHandler, EditIncludeHandler
{
  m: M;

  organisationsFilter: SearchFactory;
  peopleFilter: SearchFactory;

  constructor(
    @Self() allors: ContextService,
    errorService: ErrorService,
    form: NgForm
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

  onPreCreatePull(pulls: Pull[], initializer?: Initializer): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    switch (initializer.propertyType) {
      case m.Employment.Employer:
        pulls.push(
          p.Organisation({
            objectId: initializer.id,
          })
        );
        break;

      case m.Employment.Employee:
        pulls.push(
          p.Person({
            objectId: initializer.id,
          })
        );
        break;
    }
  }

  onPostCreatePull(
    object: Employment,
    pullResult: PullResult,
    initializer: Initializer
  ) {
    const { m } = this;

    switch (initializer.propertyType) {
      case m.Employment.Employer:
        this.object.Employer = pullResult.object(m.Organisation);
        break;

      case m.Employment.Employee:
        this.object.Employee = pullResult.object(m.Person);
        break;
    }

    object.FromDate = new Date();
  }

  onEditInclude(): Node[] {
    const {
      m: { treeBuilder: t },
    } = this;

    return t.Employment({
      Employee: {},
      Employer: {},
    });
  }
}
