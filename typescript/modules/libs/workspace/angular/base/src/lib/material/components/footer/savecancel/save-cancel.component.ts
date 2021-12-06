import { Component } from '@angular/core';
import { Location } from '@angular/common';
import { NgForm } from '@angular/forms';
import { ContextService } from '@allors/workspace/angular/core';

@Component({
  // eslint-disable-next-line @angular-eslint/component-selector
  selector: 'a-mat-footer-save-cancel',
  templateUrl: './save-cancel.component.html',
})
export class AllorsMaterialFooterSaveCancelComponent {
  constructor(public form: NgForm, public allors: ContextService, public location: Location) {}
}
