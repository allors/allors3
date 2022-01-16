import { Component } from '@angular/core';
import { NgForm } from '@angular/forms';
import { AllorsComponent, FormService } from '@allors/workspace/angular/base';

@Component({
  selector: 'a-mat-save',
  template: `
    <button
      mat-button
      color="primary"
      type="button"
      [disabled]="!formService.canSave()"
      (click)="this.formService.save()"
    >
      SAVE
    </button>
    canSave: {{ formService.canSave }}
  `,
})
export class AllorsMaterialSaveComponent extends AllorsComponent {
  dataAllorsKind = 'save';

  constructor(public form: NgForm, public formService: FormService) {
    super();
  }
}
