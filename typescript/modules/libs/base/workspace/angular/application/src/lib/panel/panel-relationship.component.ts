import { Directive, HostBinding } from '@angular/core';
import { IObject } from '@allors/system/workspace/domain';
import { ObjectType, RoleType } from '@allors/system/workspace/meta';
import { PanelService } from './panel.service';
import { M } from '@allors/default/workspace/meta';

@Directive()
export abstract class AllorsPanelRelationshipComponent<T extends IObject> {
  @HostBinding('attr.data-allors-kind')
  dataAllorsKind = 'panel-relation-object';

  @HostBinding('attr.data-allors-id')
  get dataAllorsId() {
    return this.panel.manager.id;
  }

  @HostBinding('attr.data-allors-relation-objecttype')
  get dataAllorsRelationObjectType() {
    return this.objectType?.tag;
  }

  @HostBinding('attr.data-allors-association-roletype')
  get dataAllorsAssociationRoleType() {
    return this.associationRoleType.relationType.tag;
  }

  @HostBinding('attr.data-allors-role-roletype')
  get dataAllorsRoleRoleType() {
    return this.roleRoleType.relationType.tag;
  }

  get objectType(): ObjectType {
    return this._relationObjectType;
  }

  set objectType(value: ObjectType) {
    this._relationObjectType = value;
    this.onRelationObjectType();
  }

  associationRoleType: RoleType;

  roleRoleType: RoleType;

  m: M;

  private _relationObjectType: ObjectType;

  constructor(public panel: PanelService) {
    this.m = this.panel.manager.context.configuration.metaPopulation as M;
  }

  protected onRelationObjectType() {
    // TODO: add to configure
    this.panel.name = this.objectType?.singularName;
    this.panel.title = this.objectType?.pluralName;
    this.panel.icon = this.objectType?.singularName.toLocaleLowerCase();
    this.panel.expandable = true;
  }
}
