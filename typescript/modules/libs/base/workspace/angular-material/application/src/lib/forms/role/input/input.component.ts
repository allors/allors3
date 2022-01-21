import { Component, Optional } from '@angular/core';
import { NgForm } from '@angular/forms';
import { RoleField } from '@allors/workspace/angular/base';

@Component({
  selector: 'a-mat-input',
  templateUrl: './input.component.html',
})
export class AllorsMaterialInputComponent extends RoleField {
  constructor(@Optional() parentForm: NgForm) {
    super(parentForm);
  }
}
