import { Component } from '@angular/core';
import { AllorsComponent } from '../../component';
import { PanelService } from '../../panel/panel.service';

@Component({
  selector: 'a-panel-detail-cancel',
  template: `<button mat-button (click)="panel.toggle()" type="button">
    CANCEL
  </button>`,
})
export class AllorsMaterialPanelDetailCancelComponent extends AllorsComponent {
  dataAllorsKind = 'panel-detail-cancel';

  constructor(public panel: PanelService) {
    super();
  }
}
