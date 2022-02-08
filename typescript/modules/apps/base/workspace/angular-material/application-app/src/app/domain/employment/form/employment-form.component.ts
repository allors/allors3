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
  EditIncludeHandler,
  Node,
  PostCreatePullHandler,
} from '@allors/system/workspace/domain';

@Component({
  templateUrl: './employment-form.component.html',
  providers: [ContextService],
})
export class EmploymentFormComponent
  extends AllorsFormComponent<Employment>
  implements PostCreatePullHandler, EditIncludeHandler
{
  organisationsFilter: SearchFactory;
  peopleFilter: SearchFactory;

  constructor(
    @Self() allors: ContextService,
    errorService: ErrorService,
    form: NgForm
  ) {
    super(allors, errorService, form);

    this.organisationsFilter = new SearchFactory({
      objectType: this.m.Organisation,
      roleTypes: [this.m.Organisation.Name],
    });

    this.peopleFilter = new SearchFactory({
      objectType: this.m.Person,
      roleTypes: [this.m.Person.FirstName, this.m.Person.LastName],
    });
  }

  onPostCreatePull(object: Employment) {
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
