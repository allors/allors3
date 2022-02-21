import { Component, Self } from '@angular/core';

import {
  RefreshService,
  SharedPullService,
} from '@allors/base/workspace/angular/foundation';

import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import {
  AllorsViewSummaryPanelComponent,
  PanelService,
  ScopedService,
} from '@allors/base/workspace/angular/application';
import { AllorsMaterialPanelService } from '@allors/base/workspace/angular-material/application';
import { IPullResult, Pull } from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import { UnifiedGood } from '@allors/default/workspace/domain';

@Component({
  selector: 'unifiedgood-summary-panel',
  templateUrl: './unifiedgood-summary-panel.component.html',
  providers: [
    ScopedService,
    {
      provide: PanelService,
      useClass: AllorsMaterialPanelService,
    },
  ],
})
export class UnifiedGoodSummaryPanelComponent extends AllorsViewSummaryPanelComponent {
  m: M;

  good: UnifiedGood;
  suppliers: string;

  constructor(
    @Self() scopedService: ScopedService,
    @Self() panelService: PanelService,
    refreshService: RefreshService,
    sharedPullService: SharedPullService,
    workspaceService: WorkspaceService
  ) {
    super(scopedService, panelService, sharedPullService, refreshService);
    this.m = workspaceService.workspace.configuration.metaPopulation as M;
  }

  onPreSharedPull(pulls: Pull[], prefix?: string) {
    const { m } = this;
    const { pullBuilder: p } = m;

    const id = this.scoped.id;

    pulls.push(
      p.UnifiedGood({
        name: prefix,
        objectId: id,
        include: {
          ProductType: {},
          Brand: {},
          Model: {},
          SuppliedBy: {},
          ManufacturedBy: {},
        },
      })
    );
  }

  onPostSharedPull(loaded: IPullResult, prefix?: string) {
    this.good = loaded.object<UnifiedGood>(prefix);

    if (this.good.SuppliedBy.length > 0) {
      this.suppliers = this.good.SuppliedBy?.map((v) => v.DisplayName)?.reduce(
        (acc: string, cur: string) => acc + ', ' + cur
      );
    }
  }
}
