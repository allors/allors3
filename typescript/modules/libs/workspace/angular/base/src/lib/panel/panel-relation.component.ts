import { Directive, HostBinding } from '@angular/core';
import { IObject } from '@allors/workspace/domain/system';
import { ObjectType } from '@allors/workspace/meta/system';
import { PanelService } from './panel.service';
import { M } from '@allors/workspace/meta/default';

@Directive()
export abstract class AllorsPanelRelationComponent<T extends IObject> {
  dataAllorsKind = 'panel-relation';

  @HostBinding('attr.data-allors-id')
  get dataAllorsId() {
    return this.panel.manager.id;
  }

  @HostBinding('attr.data-allors-relation-objecttype')
  get dataAllorsRelationObjectType() {
    return this.relationObjectType?.tag;
  }

  get relationObjectType(): ObjectType {
    return this._relationObjectType;
  }

  set relationObjectType(value: ObjectType) {
    this._relationObjectType = value;
    this.onRelationObjectType();
  }

  m: M;

  private _relationObjectType: ObjectType;

  constructor(public panel: PanelService) {
    this.m = this.panel.manager.context.configuration.metaPopulation as M;
  }

  protected onRelationObjectType() {
    // TODO: add to configure
    this.panel.name = this.relationObjectType?.singularName;
    this.panel.title = this.relationObjectType?.pluralName;
    this.panel.icon = this.relationObjectType?.singularName.toLocaleLowerCase();
    this.panel.expandable = true;
  }
}
