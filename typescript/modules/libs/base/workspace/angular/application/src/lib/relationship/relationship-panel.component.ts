import { Directive, HostBinding } from '@angular/core';
import { RoleType } from '@allors/system/workspace/meta';
import { M } from '@allors/default/workspace/meta';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import { PanelManagerService } from '../panel/panel-manager.service';

@Directive()
export abstract class AllorsRelationshipPanelComponent {
  @HostBinding('attr.data-allors-kind')
  abstract dataAllorsKind: string;

  @HostBinding('attr.data-allors-id')
  get dataAllorsId() {
    return null;
    // return this.panel.manager.id;
  }

  @HostBinding('attr.data-allors-anchor')
  get dataAllorsFromRelationType() {
    return this.anchor.relationType.tag;
  }

  @HostBinding('attr.data-allors-target')
  get dataAllorsToRelationType() {
    return this.target.relationType.tag;
  }

  abstract anchor: RoleType;

  abstract target: RoleType;

  m: M;

  constructor(
    public panelManagerService: PanelManagerService,
    workspaceService: WorkspaceService
  ) {
    this.m = workspaceService.workspace.configuration.metaPopulation as M;
  }
}
