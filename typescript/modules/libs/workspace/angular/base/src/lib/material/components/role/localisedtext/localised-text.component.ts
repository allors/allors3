import { Component, Optional } from '@angular/core';
import { NgForm } from '@angular/forms';

import { LocalisedRoleField } from '../../../../components/forms/localised-role-field';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'a-mat-localised-text',
  templateUrl: './localised-text.component.html',
})
export class AllorsMaterialLocalisedTextComponent extends LocalisedRoleField {

  constructor(@Optional() parentForm: NgForm) {
    super(parentForm);
  }
}
