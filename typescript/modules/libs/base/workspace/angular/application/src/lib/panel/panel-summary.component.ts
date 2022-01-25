import { Directive } from '@angular/core';
import { IObject } from '@allors/system/workspace/domain';
import { PanelService } from './panel.service';
import { AllorsPanelComponent } from './panel.component';

@Directive()
export abstract class AllorsPanelSummaryComponent<
  T extends IObject
> extends AllorsPanelComponent<T> {
  override dataAllorsKind = 'panel-summary';

  constructor(public override panel: PanelService) {
    super(panel);

    panel.name = 'summary';
  }
}
