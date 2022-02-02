import { Component, Self } from '@angular/core';
import {
  Enumeration,
  Gender,
  Locale,
  Person,
} from '@allors/default/workspace/domain';
import {
  AllorsFormComponent,
  ContextService,
} from '@allors/base/workspace/angular/foundation';
import { ErrorService } from '@allors/base/workspace/angular/foundation';
import {
  IPullResult,
  OnCreateOrEdit,
  OnPreEdit,
  Pull,
} from '@allors/system/workspace/domain';
import { NgForm } from '@angular/forms';
@Component({
  selector: 'person-form',
  templateUrl: './person-form.component.html',
  providers: [ContextService],
})
export class PersonDetailComponent
  extends AllorsFormComponent<Person>
  implements OnPreEdit, OnCreateOrEdit
{
  locales: Locale[];
  genders: Enumeration[];

  constructor(
    @Self() allors: ContextService,
    errorService: ErrorService,
    form: NgForm
  ) {
    super(allors, errorService, form);
  }

  onPreEdit(objectId: number, pulls: Pull[]) {
    const m = this.m;
    const { pullBuilder: p } = m;

    pulls.push(
      p.Person({
        objectId,
        include: {
          Gender: {},
          Locale: {},
        },
      })
    );
  }

  onPreCreateOrEdit(pulls: Pull[]) {
    const m = this.m;
    const { pullBuilder: p } = m;

    pulls.push(p.Locale({}), p.Gender({}));
  }

  onPostCreateOrEdit(object: Person, pullResult: IPullResult) {
    const m = this.m;

    this.genders = pullResult.collection<Gender>(m.Gender);
    this.locales = pullResult.collection<Locale>(m.Locale) || [];
  }
}
