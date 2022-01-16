import { Component } from '@angular/core';
import { AllorsComponent, FormService } from '@allors/workspace/angular/base';

@Component({
  selector: 'a-mat-cancel',
  template: `
    <button mat-button type="button" (click)="this.formService.cancel()">
      CANCEL
    </button>
  `,
})
export class AllorsMaterialCancelComponent extends AllorsComponent {
  dataAllorsKind = 'cancel';

  constructor(public formService: FormService) {
    super();
  }
}
