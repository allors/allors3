import { Component } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

import {
  ErrorService,
  InvokeService,
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
import { IrpfRegime } from '@allors/default/workspace/domain';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'irpfregime-summary-panel',
  templateUrl: './irpfregime-summary-panel.component.html',
})
export class IrpfRegimeSummaryPanelComponent extends AllorsViewSummaryPanelComponent {
  m: M;

  irpfregime: IrpfRegime;

  constructor(
    scopedService: ScopedService,
    panelService: PanelService,
    refreshService: RefreshService,
    sharedPullService: SharedPullService,
    workspaceService: WorkspaceService,
    public navigation: NavigationService,
    private snackBar: MatSnackBar,
    private invokeService: InvokeService,
    private errorService: ErrorService
  ) {
    super(scopedService, panelService, sharedPullService, refreshService);
    this.m = workspaceService.workspace.configuration.metaPopulation as M;
  }

  onPreSharedPull(pulls: Pull[], prefix?: string) {
    const { m } = this;
    const { pullBuilder: p } = m;

    const id = this.scoped.id;

    pulls.push(
      p.IrpfRegime({
        name: prefix,
        objectId: id,
      })
    );
  }

  onPostSharedPull(loaded: IPullResult, prefix?: string) {
    this.irpfregime = loaded.object<IrpfRegime>(prefix);
  }
}
