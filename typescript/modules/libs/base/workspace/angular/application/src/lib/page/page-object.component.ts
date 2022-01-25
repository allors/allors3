import { HostBinding, Directive } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { M } from '@allors/default/workspace/meta';
import { IObject } from '@allors/system/workspace/domain';
import {
  AllorsComponent,
  ContextService,
} from '@allors/base/workspace/angular/foundation';
import { PanelManagerService } from '../panel/panel-manager.service';

@Directive()
export abstract class AllorsPageObjectComponent<
  T extends IObject
> extends AllorsComponent {
  override dataAllorsKind = 'overview';

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

  title: string;

  private _object: T;

  constructor(
    public allors: ContextService,
    public panelManager: PanelManagerService,
    private titleService: Title
  ) {
    super();

    this.m = this.allors.context.configuration.metaPopulation as M;
    this.allors.context.name = this.constructor.name;
  }

  protected onObject() {
    // TODO: add to configure
    this.title = this.object?.strategy.cls.singularName;
    this.titleService.setTitle(this.title);
  }
}
