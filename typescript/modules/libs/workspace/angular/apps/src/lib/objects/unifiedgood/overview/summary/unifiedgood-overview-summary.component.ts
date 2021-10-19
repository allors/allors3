import { Component, Self } from '@angular/core';

import { M } from '@allors/workspace/meta/default';
import { UnifiedGood } from '@allors/workspace/domain/default';
import { NavigationService, PanelService } from '@allors/workspace/angular/base';
import { WorkspaceService } from '@allors/workspace/angular/core';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'unifiedgood-overview-summary',
  templateUrl: './unifiedgood-overview-summary.component.html',
  providers: [PanelService],
})
export class UnifiedGoodOverviewSummaryComponent {
  m: M;

  good: UnifiedGood;
  suppliers: string;

  constructor(@Self() public panel: PanelService, public workspaceService: WorkspaceService, public navigation: NavigationService) {
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
        this.suppliers = this.good.SuppliedBy.map((v) => v.DisplayName).reduce((acc: string, cur: string) => acc + ', ' + cur);
      }
    };
  }
}