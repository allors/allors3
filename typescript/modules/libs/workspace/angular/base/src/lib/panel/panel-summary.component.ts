import { Directive } from '@angular/core';
import { IObject } from '@allors/workspace/domain/system';
import { PanelService } from './panel.service';
import { AllorsPanelObjectComponent } from './panel-object.component';

@Directive()
export abstract class AllorsPanelSummaryComponent<
  T extends IObject
> extends AllorsPanelObjectComponent<T> {
  dataAllorsKind = 'panel-summary';

  constructor(public panel: PanelService) {
    super(panel);

    panel.name = 'summary';
  }
}
