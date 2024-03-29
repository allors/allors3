import { Component } from '@angular/core';

import {
  RefreshService,
  SharedPullService,
} from '@allors/base/workspace/angular/foundation';

import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import {
  AllorsViewSummaryPanelComponent,
  NavigationService,
  PanelService,
  ScopedService,
} from '@allors/base/workspace/angular/application';
import { IPullResult, Pull } from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import { NonUnifiedGood } from '@allors/default/workspace/domain';

@Component({
  selector: 'nonunifiedgood-summary-panel',
  templateUrl: './nonunifiedgood-summary-panel.component.html',
})
export class NonUnifiedGoodSummaryPanelComponent extends AllorsViewSummaryPanelComponent {
  m: M;

  good: NonUnifiedGood;

  constructor(
    scopedService: ScopedService,
    panelService: PanelService,
    refreshService: RefreshService,
    sharedPullService: SharedPullService,
    workspaceService: WorkspaceService,
    public navigation: NavigationService
  ) {
    super(scopedService, panelService, sharedPullService, refreshService);
    this.m = workspaceService.workspace.configuration.metaPopulation as M;
  }

  onPreSharedPull(pulls: Pull[], prefix?: string) {
    const {
      m: { pullBuilder: p },
    } = this;

    const id = this.scoped.id;

    pulls.push(
      p.NonUnifiedGood({
        name: prefix,
        objectId: id,
        include: {
          ProductIdentifications: {
            ProductIdentificationType: {},
          },
          Part: {
            Brand: {},
            Model: {},
          },
        },
      })
    );
  }

  onPostSharedPull(loaded: IPullResult, prefix?: string) {
    this.good = loaded.object<NonUnifiedGood>(prefix);
  }
}
