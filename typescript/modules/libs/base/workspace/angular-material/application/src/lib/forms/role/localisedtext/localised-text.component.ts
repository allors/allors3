import { Component, Optional } from '@angular/core';
import { NgForm } from '@angular/forms';
import { LocalisedRoleField } from '@allors/base/workspace/angular/foundation';

@Component({
  selector: 'a-mat-localised-text',
  templateUrl: './localised-text.component.html',
})
export class AllorsMaterialLocalisedTextComponent extends LocalisedRoleField {
  constructor(@Optional() form: NgForm) {
    super(form);
  }
}
