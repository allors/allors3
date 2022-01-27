import { Directive, HostBinding } from '@angular/core';
import { RoleType } from '@allors/system/workspace/meta';
import { PanelService } from './panel.service';
import { M } from '@allors/default/workspace/meta';

@Directive()
export abstract class AllorsPanelRelationshipComponent {
  @HostBinding('attr.data-allors-kind')
  dataAllorsKind = 'panel-relation-object';

  @HostBinding('attr.data-allors-id')
  get dataAllorsId() {
    return this.panel.manager.id;
  }

  @HostBinding('attr.data-allors-from-relationtype')
  get dataAllorsFromRelationType() {
    return this.anchor.relationType.tag;
  }

  @HostBinding('attr.data-allors-to-relationtype')
  get dataAllorsToRelationType() {
    return this.target.relationType.tag;
  }

  abstract anchor: RoleType;

  abstract target: RoleType;

  m: M;

  constructor(public panel: PanelService) {
    this.m = this.panel.manager.context.configuration.metaPopulation as M;
  }
}
