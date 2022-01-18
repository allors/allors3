import { Component, Input } from '@angular/core';
import { AllorsComponent, AllorsForm } from '@allors/workspace/angular/base';

@Component({
  selector: 'a-mat-cancel',
  template: `
    <button *ngIf="form" mat-button type="button" (click)="this.form.cancel()">
      CANCEL
    </button>
  `,
})
export class AllorsMaterialCancelComponent extends AllorsComponent {
  dataAllorsKind = 'cancel';

  @Input()
  form: AllorsForm;
}
