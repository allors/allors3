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
import { WorkEffort, WorkRequirement } from '@allors/default/workspace/domain';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'workrequirement-summary-panel',
  templateUrl: './workrequirement-summary-panel.component.html',
})
export class WorkRequirementSummaryPanelComponent extends AllorsViewSummaryPanelComponent {
  m: M;

  requirement: WorkRequirement;
  workEffort: WorkEffort;

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
      p.WorkRequirement({
        name: prefix,
        objectId: id,
        include: {
          Originator: {},
          FixedAsset: {},
          WorkRequirementFulfillmentWhereFullfilledBy: {
            FullfillmentOf: {},
          },
          LastModifiedBy: {},
        },
      })
    );
  }

  onPostSharedPull(loaded: IPullResult, prefix?: string) {
    this.requirement = loaded.object<WorkRequirement>(prefix);
    this.workEffort =
      this.requirement.WorkRequirementFulfillmentWhereFullfilledBy?.FullfillmentOf;
  }

  public cancel(): void {
    this.invokeService.invoke(this.requirement.Cancel).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully cancelled.', 'close', {
        duration: 5000,
      });
    }, this.errorService.errorHandler);
  }

  public reopen(): void {
    this.invokeService.invoke(this.requirement.Reopen).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully reopened.', 'close', { duration: 5000 });
    }, this.errorService.errorHandler);
  }

  public start(): void {
    this.invokeService.invoke(this.requirement.Start).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully started.', 'close', { duration: 5000 });
    }, this.errorService.errorHandler);
  }

  public close(): void {
    this.invokeService.invoke(this.requirement.Close).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully closed.', 'close', { duration: 5000 });
    }, this.errorService.errorHandler);
  }

  public createWorkTask(): void {
    this.invokeService.invoke(this.requirement.CreateWorkTask).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Work order created.', 'close', { duration: 5000 });
    }, this.errorService.errorHandler);
  }
}
