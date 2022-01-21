import { IObject } from '@allors/system/workspace/domain';
import { Directive } from '@angular/core';
import { PanelService } from './panel.service';
import { AllorsObjectPanelComponent } from './object-panel.component';
import { ContextService } from '../context/context-service';

@Directive()
export abstract class AllorsObjectDetailPanelComponent<
  T extends IObject
> extends AllorsObjectPanelComponent<T> {
  override dataAllorsKind = 'panel-detail';

  constructor(
    public allors: ContextService,
    public override panel: PanelService
  ) {
    super(panel);

    this.allors.context.name = this.constructor.name;

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
