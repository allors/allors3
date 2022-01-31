import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Country, Organisation } from '@allors/default/workspace/domain';
import {
  AllorsFormComponent,
  ContextService,
  SearchFactory,
} from '@allors/base/workspace/angular/foundation';
import { ErrorService } from '@allors/base/workspace/angular/foundation';
import {
  IObject,
  IPullResult,
  OnCreate,
  Pull,
} from '@allors/system/workspace/domain';

@Component({
  selector: 'organisation-form',
  templateUrl: './organisation-form.component.html',
  providers: [ContextService],
})
export class OrganisationFormComponent
  extends AllorsFormComponent<Organisation>
  implements OnCreate
{
  countries: Country[];
  peopleFilter: SearchFactory;

  constructor(
    @Self() allors: ContextService,
    errorService: ErrorService,
    form: NgForm
  ) {
    super(allors, errorService, form);

    this.peopleFilter = new SearchFactory({
      objectType: this.m.Person,
      roleTypes: [this.m.Person.FirstName, this.m.Person.LastName],
    });
  }

  onObjectPreCreate(pulls: Pull[]) {
    const m = this.m;
    const { pullBuilder: p } = m;

    pulls.push(
      p.Country({
        sorting: [{ roleType: m.Country.Name }],
      })
    );
  }

  onObjectPostCreate(object: IObject, pullResult: IPullResult) {
    this.countries = pullResult.collection<Country>(this.m.Country);
  }
}
