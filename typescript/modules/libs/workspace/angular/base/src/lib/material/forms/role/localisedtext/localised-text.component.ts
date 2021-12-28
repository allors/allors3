import { Component, Optional } from '@angular/core';
import { NgForm } from '@angular/forms';
import { LocalisedRoleField } from '../../../../forms/localised-role-field';

@Component({
  
  selector: 'a-mat-localised-text',
  templateUrl: './localised-text.component.html',
})
export class AllorsMaterialLocalisedTextComponent extends LocalisedRoleField {
  constructor(@Optional() parentForm: NgForm) {
    super(parentForm);
  }
}
