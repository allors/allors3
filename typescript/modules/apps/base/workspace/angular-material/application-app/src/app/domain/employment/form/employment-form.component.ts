import { Subscription } from 'rxjs';
import { Component, OnDestroy, Self } from '@angular/core';
import { Employment } from '@allors/default/workspace/domain';
import {
  AllorsFormComponent,
  ContextService,
  CreateRequest,
  EditRequest,
} from '@allors/base/workspace/angular/foundation';
import {
  ErrorService,
  SearchFactory,
} from '@allors/base/workspace/angular/foundation';
import { NgForm } from '@angular/forms';
import { Pull } from '@allors/system/workspace/domain';

@Component({
  templateUrl: './employment-form.component.html',
  providers: [ContextService],
})
export class EmploymentEditComponent
  extends AllorsFormComponent<Employment>
  implements OnDestroy
{
  organisationsFilter: SearchFactory;
  peopleFilter: SearchFactory;

  private subscription: Subscription;

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

  edit(request: EditRequest): void {
    const m = this.m;
    const { pullBuilder: p } = m;

    const pull = p.Employment({
      objectId: request.object.id,
      include: {
        Employee: {},
        Employer: {},
      },
    });

    this.subscription?.unsubscribe();
    this.subscription = this.context.pull(pull).subscribe((loaded) => {
      this.object = loaded.objects.values().next()?.value;
    });
  }

  public ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }
}
