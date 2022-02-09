import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Carrier } from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

@Component({
  templateUrl: './carrier-form.component.html',
  providers: [ContextService],
})
export class CarrierFormComponent extends AllorsFormComponent<Carrier> {
  public m: M;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }
}
