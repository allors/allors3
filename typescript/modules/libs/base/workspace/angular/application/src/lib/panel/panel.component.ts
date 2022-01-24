import { Directive, HostBinding } from '@angular/core';
import { M } from '@allors/default/workspace/meta';
import { PanelService } from './panel.service';
import { IObject } from '@allors/system/workspace/domain';
import { AllorsComponent } from '@allors/base/workspace/angular/foundation';

@Directive()
export abstract class AllorsPanelComponent<
  T extends IObject
> extends AllorsComponent {
  @HostBinding('attr.data-allors-id')
  get dataAllorsId() {
    return this.object?.strategy.id;
  }

  get object(): T {
    return this._object;
  }

  set object(value: T) {
    this._object = value;
    this.onObject();
  }

  m: M;

  private _object: T;

  constructor(public panel: PanelService) {
    super();

    this.m = this.panel.manager.context.configuration.metaPopulation as M;
  }

  protected onObject() {}
}
