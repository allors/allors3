import { Directive } from '@angular/core';
import { IObject } from '@allors/workspace/domain/system';
import { AllorsPanelComponent } from './panel.component';
import { PanelService } from './panel.service';

@Directive()
export abstract class AllorsPanelSummaryComponent<
  T extends IObject
> extends AllorsPanelComponent<T> {
  dataAllorsKind = 'panel-summary';

  constructor(public panel: PanelService) {
    super(panel);

    panel.name = 'summary';
  }
}
