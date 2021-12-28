import { Component } from '@angular/core';
import { Location } from '@angular/common';
import { NgForm } from '@angular/forms';
import { ContextService } from '@allors/workspace/angular/core';
import { AllorsComponent } from '../../../component';

@Component({
  
  selector: 'a-mat-footer-save-cancel',
  templateUrl: './save-cancel.component.html',
})
export class AllorsMaterialFooterSaveCancelComponent extends AllorsComponent {
  constructor(public form: NgForm, public allors: ContextService, public location: Location) {
    super();
  }
}
