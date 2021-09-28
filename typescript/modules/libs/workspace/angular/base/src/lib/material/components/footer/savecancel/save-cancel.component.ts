import { Component } from '@angular/core';
import { Location } from '@angular/common';
import { NgForm } from '@angular/forms';
import { SessionService } from '@allors/workspace/angular/core';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'a-mat-footer-save-cancel',
  templateUrl: './save-cancel.component.html',
})
export class AllorsMaterialFooterSaveCancelComponent {
  constructor(public form: NgForm, public allors: SessionService, public location: Location) {}
}
