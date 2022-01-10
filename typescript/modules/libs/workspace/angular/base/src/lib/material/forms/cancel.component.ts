import { Component, EventEmitter, Input, Output } from '@angular/core';
import { AllorsComponent } from '../../component';

@Component({
  selector: 'a-mat-cancel',
  template: `
    <button
      mat-button
      (click)="cancel.emit()"
      type="button"
      [disabled]="!canCancel"
    >
      CANCEL
    </button>
  `,
})
export class AllorsMaterialCancelComponent extends AllorsComponent {
  dataAllorsKind = 'cancel';

  @Input() canCancel = true;

  @Output() cancel = new EventEmitter();

  constructor() {
    super();
  }
}
