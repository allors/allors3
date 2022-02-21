import { Component } from '@angular/core';

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
import { IPullResult, Pull } from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import { Organisation } from '@allors/default/workspace/domain';

@Component({
  selector: 'organisation-summary-panel',
  templateUrl: './organisation-summary-panel.component.html',
})
export class OrganisationSummaryPanelComponent extends AllorsViewSummaryPanelComponent {
  m: M;

  organisation: Organisation;
  contactKindsText: string;

  constructor(
    scopedService: ScopedService,
    panelService: PanelService,
    refreshService: RefreshService,
    sharedPullService: SharedPullService,
    workspaceService: WorkspaceService
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
      p.Organisation({
        name: prefix,
        objectId: id,
        include: {
          Locale: {},
          LastModifiedBy: {},
        },
      })
    );
  }

  onPostSharedPull(loaded: IPullResult, prefix?: string) {
    this.organisation = loaded.object<Organisation>(prefix);
  }
}
