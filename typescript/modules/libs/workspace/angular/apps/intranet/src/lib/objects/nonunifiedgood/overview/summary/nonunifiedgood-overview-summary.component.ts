import { Component, Self } from '@angular/core';

import { M } from '@allors/workspace/meta/default';
import { NavigationService, PanelService, TestScope } from '@allors/workspace/angular/base';
import { NonUnifiedGood } from '@allors/workspace/domain/default';
import { WorkspaceService } from '@allors/workspace/angular/core';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'nonunifiedgood-overview-summary',
  templateUrl: './nonunifiedgood-overview-summary.component.html',
  providers: [PanelService],
})
export class NonUnifiedGoodOverviewSummaryComponent extends TestScope {
  m: M;

  good: NonUnifiedGood;

  constructor(@Self() public panel: PanelService, public workspaceService: WorkspaceService, public navigation: NavigationService) {
    super();

    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;

    panel.name = 'summary';

    const pullName = `${panel.name}_${this.m.NonUnifiedGood.tag}`;

    panel.onPull = (pulls) => {
      const m = this.m;
      const { pullBuilder: pull } = m;
      const x = {};

      const id = this.panel.manager.id;

      pulls.push(
        pull.NonUnifiedGood({
          name: pullName,
          objectId: id,
          include: {
            ProductIdentifications: {
              ProductIdentificationType: x,
            },
            Part: {
              Brand: x,
              Model: x,
            },
          },
        })
      );
    };

    panel.onPulled = (loaded) => {
      this.good = loaded.object<NonUnifiedGood>(pullName);
    };
  }
}