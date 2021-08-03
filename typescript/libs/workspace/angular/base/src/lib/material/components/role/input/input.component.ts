import { Component, Optional, Input } from '@angular/core';
import { NgForm } from '@angular/forms';

import { RoleField } from '../../../../components/forms/RoleField';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'a-mat-input',
  templateUrl: './input.component.html',
})
export class AllorsMaterialInputComponent extends RoleField {

  constructor(@Optional() parentForm: NgForm) {
    super(parentForm);
  }
}
