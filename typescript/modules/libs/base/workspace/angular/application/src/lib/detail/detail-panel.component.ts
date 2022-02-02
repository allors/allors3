import { Directive, HostBinding } from '@angular/core';
import { M } from '@allors/default/workspace/meta';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import { OverviewPageService } from '../overview/overview.service';
import { Panel, PanelKind, PanelMode } from '../panel/panel';
import { PanelService } from '../panel/panel.service';

@Directive()
export abstract class AllorsDetailPanelComponent implements Panel {
  @HostBinding('attr.data-allors-kind')
  abstract dataAllorsKind: string;

  @HostBinding('attr.data-allors-id')
  get dataAllorsId() {
    return this.overviewService.id;
  }

  @HostBinding('attr.data-allors-objecttype')
  get dataAllorsObjectType() {
    return this.overviewService.objectType.tag;
  }

  abstract panelId: string;

  abstract panelMode: PanelMode;

  panelKind: PanelKind = 'Detail';

  panelEnabled: boolean;

  m: M;

  constructor(
    public overviewService: OverviewPageService,
    public panelService: PanelService,
    workspaceService: WorkspaceService
  ) {
    this.m = workspaceService.workspace.configuration.metaPopulation as M;
  }
}