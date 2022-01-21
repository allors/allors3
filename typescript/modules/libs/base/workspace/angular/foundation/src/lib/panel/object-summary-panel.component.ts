import { Directive } from '@angular/core';
import { IObject } from '@allors/system/workspace/domain';
import { PanelService } from './panel.service';
import { AllorsObjectPanelComponent } from './object-panel.component';

@Directive()
export abstract class AllorsPanelSummaryComponent<
  T extends IObject
> extends AllorsObjectPanelComponent<T> {
  dataAllorsKind = 'panel-summary';

  constructor(public panel: PanelService) {
    super(panel);

    panel.name = 'summary';
  }
}
