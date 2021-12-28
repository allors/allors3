import { Component, Self } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

import { M } from '@allors/workspace/meta/default';
import { WorkspaceService } from '@allors/workspace/angular/core';
import { WorkTask, SalesInvoice, FixedAsset, Printable } from '@allors/workspace/domain/default';
import { Action, NavigationService, PanelService, RefreshService, SaveService } from '@allors/workspace/angular/base';
import { PrintService } from '../../../../actions/print/print.service';

@Component({
  
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
    private saveService: SaveService,
    public snackBar: MatSnackBar,
    public printService: PrintService
  ) {
    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;
    const m = this.m;

    this.print = printService.print();

    panel.name = 'summary';

    const workTaskPullName = `${panel.name}_${this.m.WorkTask.tag}`;
    const serviceEntryPullName = `${panel.name}_${this.m.ServiceEntry.tag}`;
    const workEffortBillingPullName = `${panel.name}_${this.m.WorkEffortBilling.tag}`;
    const fixedAssetPullName = `${panel.name}_${this.m.FixedAsset.tag}`;

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

      this.assets = loaded.collection<FixedAsset>(fixedAssetPullName);

      const salesInvoices1 = loaded.collection<SalesInvoice>(workEffortBillingPullName) ?? [];
      const salesInvoices2 = loaded.collection<SalesInvoice>(serviceEntryPullName) ?? [];
      this.salesInvoices = new Set([...salesInvoices1, ...salesInvoices2]);
    };
  }
}
