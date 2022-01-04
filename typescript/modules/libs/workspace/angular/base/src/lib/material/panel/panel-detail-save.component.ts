import { Component } from '@angular/core';
import { NgForm } from '@angular/forms';
import { ContextService } from '@allors/workspace/angular/core';
import { AllorsComponent } from '../../component';

@Component({
  selector: 'a-panel-detail-save',
  template: `<button
    mat-button
    class="ml-2"
    color="primary"
    type="submit"
    [disabled]="!form.form.valid || !allors.context.hasChanges"
  >
    SAVE
  </button>`,
})
export class AllorsMaterialPanelDetailSaveComponent extends AllorsComponent {
  dataAllorsKind = 'panel-detail-save';

  constructor(public allors: ContextService, public form: NgForm) {
    super();
  }
}
