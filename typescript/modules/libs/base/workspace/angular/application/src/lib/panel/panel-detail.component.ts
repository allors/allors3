import { IObject } from '@allors/system/workspace/domain';
import { Directive } from '@angular/core';
import { PanelService } from './panel.service';
import { AllorsPanelComponent } from './panel.component';

@Directive()
export abstract class AllorsPanelDetailComponent<
  T extends IObject
> extends AllorsPanelComponent<T> {
  override dataAllorsKind = 'panel-detail';

  constructor(public override panel: PanelService) {
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
