import { Component, Self } from '@angular/core';

import { M } from '@allors/workspace/meta/default';
import { Organisation } from '@allors/workspace/domain/default';
import { NavigationService, PanelService, TestScope } from '@allors/workspace/angular/base';


@Component({
  // tslint:disable-next-line:component-selector
  selector: 'organisation-overview-summary',
  templateUrl: './organisation-overview-summary.component.html',
  providers: [PanelService]
})
export class OrganisationOverviewSummaryComponent extends TestScope {

  m: M;

  organisation: Organisation;
  contactKindsText: string;

  constructor(
    @Self() public panel: PanelService,
    
    public navigation: NavigationService
  ) {
    super();

    this.m = this.allors.workspace.configuration.metaPopulation as M;

    panel.name = 'summary';

    const organisationPullName = `${panel.name}_${this.m.Organisation.tag}`;

    panel.onPull = (pulls) => {
      const m = this.m; const { pullBuilder: pull } = m; const x = {};

      const id = this.panel.manager.id;

      pulls.push(
        pull.Organisation({
          name: organisationPullName,
          objectId: id,
          include: {
            Locale: x,
            LastModifiedBy: x,
          }
        }));
    };

    panel.onPulled = (loaded) => {
      this.organisation = loaded.objects[organisationPullName] as Organisation;
    };
  }
}
