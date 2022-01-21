import { Directive } from '@angular/core';
import { IObject } from '@allors/system/workspace/domain';
import { PanelService } from './panel.service';
import { AllorsObjectPanelComponent } from './object-panel.component';

@Directive()
export abstract class AllorsPanelSummaryComponent<
  T extends IObject
> extends AllorsObjectPanelComponent<T> {
  override dataAllorsKind = 'panel-summary';

  constructor(public override panel: PanelService) {
    super(panel);

    panel.name = 'summary';
  }
}
