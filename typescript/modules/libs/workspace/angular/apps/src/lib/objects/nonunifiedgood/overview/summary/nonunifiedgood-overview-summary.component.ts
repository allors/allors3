import { Component, Self } from '@angular/core';

import { MetaService, NavigationService, PanelService } from '@allors/angular/services/core';
import { NonUnifiedGood } from '@allors/domain/generated';
import { Meta } from '@allors/meta/generated';
import { TestScope } from '@allors/angular/core';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'nonunifiedgood-overview-summary',
  templateUrl: './nonunifiedgood-overview-summary.component.html',
  providers: [PanelService]
})
export class NonUnifiedGoodOverviewSummaryComponent extends TestScope {

  m: M;

  good: NonUnifiedGood;

  constructor(
    @Self() public panel: PanelService,
    
    public navigation: NavigationService
  ) {
    super();

    this.m = this.allors.workspace.configuration.metaPopulation as M;

    panel.name = 'summary';

    const pullName = `${panel.name}_${this.m.NonUnifiedGood.tag}`;

    panel.onPull = (pulls) => {
      const m = this.m; const { pullBuilder: pull } = m; const x = {};

      const id = this.panel.manager.id;

      pulls.push(
        pull.NonUnifiedGood({
          name: pullName,
          objectId: id,
          include: {
            ProductIdentifications: {
              ProductIdentificationType: x
            },
            Part: {
              Brand: x,
              Model: x
            }
          }
        })
      );
    };

    panel.onPulled = (loaded) => {
      this.good = loaded.objects[pullName] as NonUnifiedGood;
    };
  }
}
