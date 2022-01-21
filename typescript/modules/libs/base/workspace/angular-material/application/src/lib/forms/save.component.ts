import { Component, Input } from '@angular/core';
import {
  AllorsComponent,
  AllorsForm,
} from '@allors/base/workspace/angular/foundation';

@Component({
  selector: 'a-mat-save',
  template: `
    <button
      *ngIf="form"
      mat-button
      color="primary"
      type="button"
      [disabled]="!form.canSave"
      (click)="this.form.save()"
    >
      SAVE
    </button>
  `,
})
export class AllorsMaterialSaveComponent extends AllorsComponent {
  override dataAllorsKind = 'save';

  @Input()
  form: AllorsForm;
}
