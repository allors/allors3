import { Directive } from '@angular/core';
import { IObject } from '@allors/system/workspace/domain';
import { OldPanelService } from '../panel/old/panel.service';
import { OldAllorsPanelComponent } from '../panel/old/panel.component';

@Directive()
export abstract class AllorsSummaryPanelComponent<
  T extends IObject
> extends OldAllorsPanelComponent<T> {
  override dataAllorsKind = 'panel-summary';

  constructor(public override panel: OldPanelService) {
    super(panel);

    panel.name = 'summary';
  }
}
