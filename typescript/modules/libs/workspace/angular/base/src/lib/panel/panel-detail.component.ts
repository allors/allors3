import { ContextService } from '@allors/workspace/angular/core';
import { IObject } from '@allors/workspace/domain/system';
import { Directive } from '@angular/core';
import { PanelService } from './panel.service';
import { AllorsPanelObjectComponent } from './panel-object.component';

@Directive()
export abstract class AllorsPanelDetailComponent<
  T extends IObject
> extends AllorsPanelObjectComponent<T> {
  dataAllorsKind = 'panel-detail';

  constructor(public allors: ContextService, public panel: PanelService) {
    super(panel);

    this.allors.context.name = this.constructor.name;

    panel.name = 'detail';
  }

  protected onObject() {
    super.onObject();

    // TODO: add to configure
    const objectType = this.object?.strategy.cls;
    this.panel.title = objectType?.singularName;
    this.panel.icon = objectType?.singularName.toLowerCase();
    this.panel.expandable = true;
  }
}
