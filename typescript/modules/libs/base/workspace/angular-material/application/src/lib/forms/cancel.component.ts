import { Component, Input } from '@angular/core';
import {
  AllorsComponent,
  AllorsForm,
} from '@allors/base/workspace/angular/foundation';

@Component({
  selector: 'a-mat-cancel',
  template: `
    <button *ngIf="form" mat-button type="button" (click)="this.form.cancel()">
      CANCEL
    </button>
  `,
})
export class AllorsMaterialCancelComponent extends AllorsComponent {
  override dataAllorsKind = 'cancel';

  @Input()
  form: AllorsForm;
}
