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
  CreateOrEditPullHandler,
  PreEditPullHandler,
  Pull,
} from '@allors/system/workspace/domain';
import { NgForm } from '@angular/forms';
@Component({
  selector: 'person-form',
  templateUrl: './person-form.component.html',
  providers: [ContextService],
})
export class PersonFormComponent
  extends AllorsFormComponent<Person>
  implements PreEditPullHandler, CreateOrEditPullHandler
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

  onPreEditPull(objectId: number, pulls: Pull[]) {
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

  onPreCreateOrEditPull(pulls: Pull[]) {
    const m = this.m;
    const { pullBuilder: p } = m;

    pulls.push(p.Locale({}), p.Gender({}));
  }

  onPostCreateOrEditPull(object: Person, pullResult: IPullResult) {
    const m = this.m;

    this.genders = pullResult.collection<Gender>(m.Gender);
    this.locales = pullResult.collection<Locale>(m.Locale) || [];
  }
}
