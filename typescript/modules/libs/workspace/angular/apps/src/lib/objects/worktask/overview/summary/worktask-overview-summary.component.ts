import { Component, Self } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

import { M } from '@allors/workspace/meta/default';
import { WorkspaceService } from '@allors/workspace/angular/core';
import { WorkTask, SalesInvoice, FixedAsset, Printable } from '@allors/workspace/domain/default';
import { Action, NavigationService, PanelService, RefreshService, SaveService, ActionTarget } from '@allors/workspace/angular/base';

import { PrintService } from '../../../../actions/print/print.service';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'worktask-overview-summary',
  templateUrl: './worktask-overview-summary.component.html',
  providers: [PanelService],
})
export class WorkTaskOverviewSummaryComponent {
  m: M;

  workTask: WorkTask;
  parent: WorkTask;

  print: Action;
  printForWorker: Action;
  salesInvoices: Set<SalesInvoice>;
  assets: FixedAsset[];

  constructor(
    @Self() public panel: PanelService,
    public workspaceService: WorkspaceService,
    public navigation: NavigationService,
    public refreshService: RefreshService,
    public printService: PrintService,
    private saveService: SaveService,
    public snackBar: MatSnackBar
  ) {
    const m = this.workspaceService.workspace.configuration.metaPopulation as M;

    this.print = printService.print();
    this.printForWorker = {
      name: 'printforworker',
      displayName: () => 'printforworker',
      description: () => 'printforworker',
      disabled: () => false,
      execute: (target: ActionTarget) => {
        const printable = target as Printable;

        const url = `${this.printService.config.url}printforworker/${printable.id}`;
        window.open(url);
      },
      result: null,
    };

    panel.name = 'summary';

    const workTaskPullName = `${panel.name}_${this.m.WorkTask.tag}`;
    const serviceEntryPullName = `${panel.name}_${this.m.ServiceEntry.tag}`;
    const workEffortBillingPullName = `${panel.name}_${this.m.WorkEffortBilling.tag}`;
    const fixedAssetPullName = `${panel.name}_${this.m.FixedAsset.tag}`;
    const parentPullName = `${panel.name}_${this.m.WorkTask.tag}_parent`;

    panel.onPull = (pulls) => {
      const { pullBuilder: pull } = m;
      const x = {};

      const id = this.panel.manager.id;

      pulls.push(
        pull.WorkTask({
          name: workTaskPullName,
          objectId: id,
          include: {
            Customer: x,
            WorkEffortState: x,
            LastModifiedBy: x,
            PrintDocument: {
              Media: x,
            },
          },
        }),
        pull.WorkTask({
          name: parentPullName,
          objectId: id,
          select: {
            WorkEffortWhereChild: x,
          },
        }),
        pull.WorkEffort({
          name: workEffortBillingPullName,
          objectId: id,
          select: {
            WorkEffortBillingsWhereWorkEffort: {
              InvoiceItem: {
                SalesInvoiceItem_SalesInvoiceWhereSalesInvoiceItem: x,
              },
            },
          },
        }),
        pull.WorkEffort({
          name: fixedAssetPullName,
          objectId: id,
          select: {
            WorkEffortFixedAssetAssignmentsWhereAssignment: {
              FixedAsset: x,
            },
          },
        }),
        pull.TimeEntryBilling({
          name: serviceEntryPullName,
          predicate: {
            kind: 'ContainedIn',
            propertyType: m.TimeEntryBilling.TimeEntry,
            extent: {
              kind: 'Filter',
              objectType: m.ServiceEntry,
              predicate: {
                kind: 'Equals',
                propertyType: m.ServiceEntry.WorkEffort,
                value: id,
              },
            },
          },
          select: {
            InvoiceItem: {
              SalesInvoiceItem_SalesInvoiceWhereSalesInvoiceItem: x,
            },
          },
        })
      );
    };

    panel.onPulled = (loaded) => {
      this.workTask = loaded.object<WorkTask>(workTaskPullName);
      this.parent = loaded.object<WorkTask>(parentPullName);

      this.assets = loaded.collection<FixedAsset>(fixedAssetPullName);

      const salesInvoices1 = loaded.collection<SalesInvoice>(workEffortBillingPullName);
      const salesInvoices2 = loaded.collection<SalesInvoice>(serviceEntryPullName);
      this.salesInvoices = new Set([...salesInvoices1, ...salesInvoices2]);
    };
  }

  public cancel(): void {
    this.panel.manager.client.invokeReactive(this.panel.manager.session, this.workTask.Cancel).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully cancelled.', 'close', { duration: 5000 });
    }, this.saveService.errorHandler);
  }

  public reopen(): void {
    this.panel.manager.client.invokeReactive(this.panel.manager.session, this.workTask.Reopen).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully reopened.', 'close', { duration: 5000 });
    }, this.saveService.errorHandler);
  }

  public revise(): void {
    this.panel.manager.client.invokeReactive(this.panel.manager.session, this.workTask.Revise).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Revise successfully executed.', 'close', { duration: 5000 });
    }, this.saveService.errorHandler);
  }

  public complete(): void {
    this.panel.manager.client.invokeReactive(this.panel.manager.session, this.workTask.Complete).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully completed.', 'close', { duration: 5000 });
    }, this.saveService.errorHandler);
  }

  public invoice(): void {
    this.panel.manager.client.invokeReactive(this.panel.manager.session, this.workTask.Invoice).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully invoiced.', 'close', { duration: 5000 });
    }, this.saveService.errorHandler);
  }
}
