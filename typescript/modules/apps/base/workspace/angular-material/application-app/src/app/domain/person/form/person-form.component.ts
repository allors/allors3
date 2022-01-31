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
  OnCreate,
  OnEdit,
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
  implements OnCreate, OnEdit
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

  onPreCreate(pulls: Pull[]) {
    this.onPreCommon(pulls);
  }

  onPostCreate(object: Person, pullResult: IPullResult) {
    this.onPostCommon(object, pullResult);
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

    this.onPreCreate(pulls);
  }

  onPostEdit(object: Person, pullResult: IPullResult) {
    this.onPostCreate(object, pullResult);
  }

  private onPreCommon(pulls: Pull[]) {
    const m = this.m;
    const { pullBuilder: p } = m;

    pulls.push(p.Locale({}), p.Gender({}));
  }

  private onPostCommon(object: Person, pullResult: IPullResult) {
    const m = this.m;

    this.genders = pullResult.collection<Gender>(m.Gender);
    this.locales = pullResult.collection<Locale>(m.Locale) || [];
  }
}
