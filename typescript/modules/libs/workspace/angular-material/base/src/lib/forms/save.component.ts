import { Component, Input } from '@angular/core';
import { NgForm } from '@angular/forms';
import { ContextService } from '@allors/workspace/angular/core';
import { AllorsComponent } from '@allors/workspace/angular/base';

@Component({
  selector: 'a-mat-save',
  template: `
    <button
      mat-button
      color="primary"
      type="submit"
      [disabled]="!canSave || !form.form.valid || !allors.context.hasChanges"
    >
      SAVE
    </button>
  `,
})
export class AllorsMaterialSaveComponent extends AllorsComponent {
  dataAllorsKind = 'save';

  @Input() canSave = true;

  constructor(public allors: ContextService, public form: NgForm) {
    super();
  }
}
