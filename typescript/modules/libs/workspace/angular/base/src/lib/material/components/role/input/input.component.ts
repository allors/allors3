import { Component, Optional } from '@angular/core';
import { NgForm } from '@angular/forms';

import { RoleField } from '../../../../components/forms/role-field';

@Component({
  // eslint-disable-next-line @angular-eslint/component-selector
  selector: 'a-mat-input',
  templateUrl: './input.component.html',
})
export class AllorsMaterialInputComponent extends RoleField {
  constructor(@Optional() parentForm: NgForm) {
    super(parentForm);
  }
}
