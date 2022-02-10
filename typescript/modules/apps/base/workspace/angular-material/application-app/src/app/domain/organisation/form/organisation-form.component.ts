import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Country, Organisation } from '@allors/default/workspace/domain';
import {
  AllorsFormComponent,
  ContextService,
  SearchFactory,
} from '@allors/base/workspace/angular/foundation';
import { ErrorService } from '@allors/base/workspace/angular/foundation';
import { IPullResult, Pull } from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';

@Component({
  templateUrl: './organisation-form.component.html',
  providers: [ContextService],
})
export class OrganisationFormComponent extends AllorsFormComponent<Organisation> {
  m: M;

  countries: Country[];
  peopleFilter: SearchFactory;

  constructor(
    @Self() allors: ContextService,
    errorService: ErrorService,
    form: NgForm
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;

    this.peopleFilter = new SearchFactory({
      objectType: this.m.Person,
      roleTypes: [this.m.Person.FirstName, this.m.Person.LastName],
    });
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    if (this.editRequest) {
      pulls.push(
        p.Organisation({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            Owner: {},
            Country: {},
          },
        })
      );
    }

    this.onPrePullInitialize(pulls);

    pulls.push(
      p.Country({
        sorting: [{ roleType: m.Country.Name }],
      })
    );
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.onPostPullInitialize(pullResult);

    this.countries = pullResult.collection<Country>(this.m.Country);
  }
}
