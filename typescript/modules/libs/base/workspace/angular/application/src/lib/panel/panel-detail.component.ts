import { IObject } from '@allors/system/workspace/domain';
import { Directive } from '@angular/core';
import { PanelService } from './panel.service';
import { AllorsPanelComponent } from './panel.component';
import { ContextService } from '@allors/base/workspace/angular/foundation';

@Directive()
export abstract class AllorsPanelDetailComponent<
  T extends IObject
> extends AllorsPanelComponent<T> {
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
