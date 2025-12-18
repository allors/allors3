import { Component, Optional } from '@angular/core';
import { NgForm } from '@angular/forms';
import { RoleField } from '@allors/base/workspace/angular/foundation';

@Component({
  selector: 'a-mat-input',
  templateUrl: './input.component.html',
})
export class AllorsMaterialInputComponent extends RoleField {
  constructor(@Optional() form: NgForm) {
    super(form);
  }
}
