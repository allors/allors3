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
import { IPullResult, Pull } from '@allors/system/workspace/domain';
import { NgForm } from '@angular/forms';
import { M } from '@allors/default/workspace/meta';
@Component({
  selector: 'person-form',
  templateUrl: './person-form.component.html',
  providers: [ContextService],
})
export class PersonFormComponent extends AllorsFormComponent<Person> {
  m: M;

  locales: Locale[];
  genders: Enumeration[];

  constructor(
    @Self() allors: ContextService,
    errorService: ErrorService,
    form: NgForm
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    if (this.editRequest) {
      pulls.push(
        p.Person({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            Gender: {},
            Locale: {},
          },
        })
      );
    }

    pulls.push(p.Locale({}), p.Gender({}));

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.onPostPullInitialize(pullResult);

    this.genders = pullResult.collection<Gender>(this.m.Gender);
    this.locales = pullResult.collection<Locale>(this.m.Locale) || [];
  }
}
