import { IObject } from '@allors/system/workspace/domain';
import { Directive } from '@angular/core';
import { OldAllorsPanelComponent } from '../panel/old/panel.component';
import { OldPanelService } from '../panel/old/panel.service';

@Directive()
export abstract class AllorsPanelDetailComponent<
  T extends IObject
> extends OldAllorsPanelComponent<T> {
  override dataAllorsKind = 'panel-detail';

  constructor(public override panel: OldPanelService) {
    super(panel);

    panel.name = 'detail';
  }

  protected override onObject() {
    super.onObject();

    // TODO: add to configure
    const objectType = this.object?.strategy.cls;
    this.panel.title = objectType?.singularName;
    this.panel.icon = objectType?.singularName.toLowerCase();
    this.panel.expandable = true;
  }
}
