import { Component, Self } from '@angular/core';

import { M } from '@allors/default/workspace/meta';
import { UnifiedGood } from '@allors/default/workspace/domain';
import {
  NavigationService,
  PanelService,
} from '@allors/base/workspace/angular/foundation';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';

@Component({
  selector: 'unifiedgood-overview-summary',
  templateUrl: './unifiedgood-overview-summary.component.html',
  providers: [PanelService],
})
export class UnifiedGoodOverviewSummaryComponent {
  m: M;

  good: UnifiedGood;
  suppliers: string;

  constructor(
    @Self() public panel: PanelService,
    public workspaceService: WorkspaceService,
    public navigation: NavigationService
  ) {
    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    panel.name = 'summary';

    const pullName = `${panel.name}_${this.m.UnifiedGood.tag}`;

    panel.onPull = (pulls) => {
      const id = this.panel.manager.id;

      pulls.push(
        pull.UnifiedGood({
          name: pullName,
          objectId: id,
          include: {
            ProductType: x,
            Brand: x,
            Model: x,
            SuppliedBy: x,
            ManufacturedBy: x,
          },
        })
      );
    };

    panel.onPulled = (loaded) => {
      this.good = loaded.object<UnifiedGood>(pullName);

      if (this.good.SuppliedBy.length > 0) {
        this.suppliers = this.good.SuppliedBy?.map(
          (v) => v.DisplayName
        )?.reduce((acc: string, cur: string) => acc + ', ' + cur);
      }
    };
  }
}
