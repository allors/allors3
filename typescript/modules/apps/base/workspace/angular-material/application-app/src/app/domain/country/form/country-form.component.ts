import { Component, Self } from '@angular/core';
import { Country } from '@allors/default/workspace/domain';
import {
  AllorsFormComponent,
  ErrorService,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { NgForm } from '@angular/forms';
import { Class } from '@allors/system/workspace/meta';

@Component({
  templateUrl: 'country-form.component.html',
  providers: [ContextService],
})
export class CountryFormComponent extends AllorsFormComponent<Country> {
  constructor(
    @Self() allors: ContextService,
    errorService: ErrorService,
    form: NgForm
  ) {
    super(allors, errorService, form);
  }

  get canWrite(): any {
    return true;
  }

  create(objectType: Class): void {
    this.object = this.context.create<Country>(objectType);
  }

  edit(objectId: number): void {
    this.context.pull({ objectId }).subscribe((loaded) => {
      this.object = loaded.objects.values().next()?.value;
    });
  }
}
