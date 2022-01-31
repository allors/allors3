import { Directive, HostBinding } from '@angular/core';
import { M } from '@allors/default/workspace/meta';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import { PanelService } from '../panel/panel-manager.service';
import { OverviewPageService } from '../overview/overview.service';
import { Panel, PanelKind, PanelMode } from '../panel/panel';

@Directive()
export abstract class AllorsDetailPanelComponent implements Panel {
  @HostBinding('attr.data-allors-kind')
  abstract dataAllorsKind: string;

  @HostBinding('attr.data-allors-id')
  get dataAllorsId() {
    return this.overviewService.id;
  }

  @HostBinding('attr.data-allors-objecttype')
  get dataAllorsFromRelationType() {
    return this.overviewService.objectType.tag;
  }

  abstract panelId: string;

  panelKind: PanelKind = 'Detail';

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
