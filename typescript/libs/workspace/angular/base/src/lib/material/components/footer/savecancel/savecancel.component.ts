import { Component, Optional } from '@angular/core';
import { Location } from '@angular/common';
import { NgForm } from '@angular/forms';

import { ContextService } from '../../../../services/framework/ContextService';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'a-mat-footer-save-cancel',
  templateUrl: './savecancel.component.html',
})
export class AllorsMaterialFooterSaveCancelComponent {

  constructor(public form: NgForm, public allors: ContextService, public location: Location) {
  }

}
