import { Directive } from '@angular/core';
import { M } from '@allors/workspace/meta/default';
import { AllorsComponent } from '../component';
import { PanelService } from './panel.service';
import { IObject } from '@allors/workspace/domain/system';

@Directive()
export abstract class AllorsPanelComponent<
  T extends IObject
> extends AllorsComponent {
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
