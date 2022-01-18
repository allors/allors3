import { Component, Self } from '@angular/core';
import { Country } from '@allors/workspace/domain/default';
import { SaveService } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';
import { AllorsFormComponent } from '@allors/workspace/angular-material/base';
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
