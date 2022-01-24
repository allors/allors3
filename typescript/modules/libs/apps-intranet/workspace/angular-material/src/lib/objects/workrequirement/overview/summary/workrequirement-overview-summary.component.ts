import { Component, Self } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

import { M } from '@allors/default/workspace/meta';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import { WorkEffort, WorkRequirement } from '@allors/default/workspace/domain';
import {
  NavigationService,
  PanelService,
  RefreshService,
  SaveService,
} from '@allors/base/workspace/angular/foundation';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'workrequirement-overview-summary',
  templateUrl: './workrequirement-overview-summary.component.html',
  providers: [PanelService],
})
export class WorkRequirementOverviewSummaryComponent {
  m: M;

  requirement: WorkRequirement;
  workEffort: WorkEffort;

  constructor(
    @Self() public panel: PanelService,
    public workspaceService: WorkspaceService,
    public navigation: NavigationService,
    public refreshService: RefreshService,
    private saveService: SaveService,
    public snackBar: MatSnackBar
  ) {
    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;
    const m = this.m;

    panel.name = 'summary';

    panel.onPull = (pulls) => {
      const { pullBuilder: pull } = m;
      const x = {};

      const id = this.panel.manager.id;

      pulls.push(
        pull.WorkRequirement({
          objectId: id,
          include: {
            Originator: x,
            FixedAsset: x,
            WorkRequirementFulfillmentWhereFullfilledBy: {
              FullfillmentOf: x,
            },
            LastModifiedBy: x,
          },
        })
      );
    };

    panel.onPulled = (loaded) => {
      this.requirement = loaded.object<WorkRequirement>(m.WorkRequirement);
      this.workEffort =
        this.requirement.WorkRequirementFulfillmentWhereFullfilledBy?.FullfillmentOf;
    };
  }

  public cancel(): void {
    this.panel.manager.context.invoke(this.requirement.Cancel).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully cancelled.', 'close', {
        duration: 5000,
      });
    }, this.saveService.errorHandler);
  }

  public reopen(): void {
    this.panel.manager.context.invoke(this.requirement.Reopen).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully reopened.', 'close', { duration: 5000 });
    }, this.saveService.errorHandler);
  }

  public start(): void {
    this.panel.manager.context.invoke(this.requirement.Start).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully started.', 'close', { duration: 5000 });
    }, this.saveService.errorHandler);
  }

  public close(): void {
    this.panel.manager.context.invoke(this.requirement.Close).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully closed.', 'close', { duration: 5000 });
    }, this.saveService.errorHandler);
  }

  public createWorkTask(): void {
    this.panel.manager.context
      .invoke(this.requirement.CreateWorkTask)
      .subscribe(() => {
        this.refreshService.refresh();
        this.snackBar.open('Work order created.', 'close', { duration: 5000 });
      }, this.saveService.errorHandler);
  }
}
