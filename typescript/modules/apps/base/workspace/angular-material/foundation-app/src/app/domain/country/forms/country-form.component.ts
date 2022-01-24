import { Component, Self } from '@angular/core';
import { Country } from '@allors/default/workspace/domain';
import { SaveService } from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { AllorsFormComponent } from '@allors/base/workspace/angular-material/application';
import { NgForm } from '@angular/forms';

@Component({
  selector: 'country-form',
  templateUrl: 'country-form.component.html',
  providers: [ContextService],
})
export class CountryFormComponent extends AllorsFormComponent<Country> {
  constructor(
    @Self() allors: ContextService,
    saveService: SaveService,
    form: NgForm
  ) {
    super(allors, saveService, form);
  }
}
