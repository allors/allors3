import { Component, Self } from '@angular/core';
import { M } from '@allors/default/workspace/meta';
import { Organisation } from '@allors/default/workspace/domain';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import { NavigationService } from '@allors/base/workspace/angular/application';
import { OldPanelService } from '@allors/base/workspace/angular/application';

@Component({
  selector: 'organisation-summary',
  templateUrl: './organisation-summary.component.html',
  providers: [OldPanelService],
})
export class OrganisationSummaryComponent {
  m: M;

  organisation: Organisation;
  contactKindsText: string;

  constructor(
    @Self() public panel: OldPanelService,
    public workspaceService: WorkspaceService,
    public navigation: NavigationService
  ) {
    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    panel.name = 'summary';

    const organisationPullName = `${panel.name}_${this.m.Organisation.tag}`;

    panel.onPull = (pulls) => {
      const id = this.panel.manager.id;

      pulls.push(
        pull.Organisation({
          name: organisationPullName,
          objectId: id,
          include: {
            Country: x,
          },
        })
      );
    };

    panel.onPulled = (loaded) => {
      this.organisation = loaded.object<Organisation>(organisationPullName);
    };
  }
}
