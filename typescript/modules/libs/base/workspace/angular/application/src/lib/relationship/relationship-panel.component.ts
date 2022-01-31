import { Directive, HostBinding } from '@angular/core';
import { RoleType } from '@allors/system/workspace/meta';
import { M } from '@allors/default/workspace/meta';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import { PanelService } from '../panel/panel-manager.service';
import { OverviewPageService } from '../overview/overview.service';
import { Panel, PanelKind, PanelMode } from '../panel/panel';

@Directive()
export abstract class AllorsRelationshipPanelComponent implements Panel {
  @HostBinding('attr.data-allors-kind')
  abstract dataAllorsKind: string;

  @HostBinding('attr.data-allors-id')
  get dataAllorsId() {
    return this.overviewService.id;
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

  abstract panelId: string;

  panelKind: PanelKind = 'RelationShip';

  abstract panelMode: PanelMode;

  m: M;

  constructor(
    public overviewService: OverviewPageService,
    public panelService: PanelService,
    workspaceService: WorkspaceService
  ) {
    this.m = workspaceService.workspace.configuration.metaPopulation as M;
  }
}
