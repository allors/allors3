import { HostBinding, Directive } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Composite } from '@allors/system/workspace/meta';
import { M } from '@allors/default/workspace/meta';
import {
  AllorsComponent,
  ContextService,
} from '@allors/base/workspace/angular/foundation';

@Directive()
export abstract class AllorsListPageComponent extends AllorsComponent {
  override dataAllorsKind = 'list-page';

  @HostBinding('attr.data-allors-objecttype')
  get dataAllorsObjectType() {
    return this.objectType?.tag;
  }

  get objectType(): Composite {
    return this._objectType;
  }

  set objectType(value: Composite) {
    this._objectType = value;
    this.onObjectType();
  }

  m: M;

  title: string;

  private _objectType: Composite;

  constructor(public allors: ContextService, private titleService: Title) {
    super();

    this.m = this.allors.context.configuration.metaPopulation as M;
    this.allors.context.name = this.constructor.name;
  }

  private onObjectType() {
    // TODO: add to configure
    this.title = this.objectType.pluralName;
    this.titleService.setTitle(this.title);
  }
}
