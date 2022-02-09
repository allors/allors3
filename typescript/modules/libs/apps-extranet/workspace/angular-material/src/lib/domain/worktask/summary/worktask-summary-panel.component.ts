import { Component } from '@angular/core';

import {
  SharedPullService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import {
  WorkTask,
  SalesInvoice,
  FixedAsset,
} from '@allors/default/workspace/domain';
import { RefreshService } from '@allors/base/workspace/angular/foundation';
import {
  Action,
  AllorsViewSummaryPanelComponent,
  NavigationService,
  ObjectService,
  PanelService,
} from '@allors/base/workspace/angular/application';
import { IPullResult, Pull } from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';

@Component({
  selector: 'worktask-summary-panel',
  templateUrl: './worktask-summary-panel.component.html',
})
export class WorkTaskSummaryPanel extends AllorsViewSummaryPanelComponent {
  workTask: WorkTask;
  parent: WorkTask;
  salesInvoices: Set<SalesInvoice>;
  assets: FixedAsset[];

  print: Action;
  printForWorker: Action;

  constructor(
    objectService: ObjectService,
    panelService: PanelService,
    sharedPullService: SharedPullService,
    private workspaceService: WorkspaceService,
    refreshService: RefreshService,
    public navigation: NavigationService
  ) {
    super(
      objectService,
      panelService,
      sharedPullService,
      refreshService,
      workspaceService
    );
  }

  onPreSharedPull(pulls: Pull[], scope?: string) {
    const m = this.workspaceService.workspace.configuration.metaPopulation as M;
    const { pullBuilder: p } = m;

    const workTaskPullName = `${scope}_${m.WorkTask.tag}`;
    const serviceEntryPullName = `${scope}_${m.ServiceEntry.tag}`;
    const workEffortBillingPullName = `${scope}_${m.WorkEffortBilling.tag}`;
    const fixedAssetPullName = `${scope}_${m.FixedAsset.tag}`;

    const id = this.objectInfo.id;

    pulls.push(
      p.WorkTask({
        name: workTaskPullName,
        objectId: id,
        include: {
          Customer: {},
          WorkEffortState: {},
          LastModifiedBy: {},
          PrintDocument: {
            Media: {},
          },
        },
      }),
      p.WorkEffort({
        name: workEffortBillingPullName,
        objectId: id,
        select: {
          WorkEffortBillingsWhereWorkEffort: {
            InvoiceItem: {
              SalesInvoiceItem_SalesInvoiceWhereSalesInvoiceItem: {},
            },
          },
        },
      }),
      p.WorkEffort({
        name: fixedAssetPullName,
        objectId: id,
        select: {
          WorkEffortFixedAssetAssignmentsWhereAssignment: {
            FixedAsset: {},
          },
        },
      }),
      p.TimeEntryBilling({
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
            SalesInvoiceItem_SalesInvoiceWhereSalesInvoiceItem: {},
          },
        },
      })
    );
  }

  onPostSharedPull(loaded: IPullResult, scope?: string) {
    const m = this.workspaceService.workspace.configuration.metaPopulation as M;

    const workTaskPullName = `${scope}_${m.WorkTask.tag}`;
    const serviceEntryPullName = `${scope}_${m.ServiceEntry.tag}`;
    const workEffortBillingPullName = `${scope}_${m.WorkEffortBilling.tag}`;
    const fixedAssetPullName = `${scope}_${m.FixedAsset.tag}`;

    this.workTask = loaded.object<WorkTask>(workTaskPullName);
    this.assets = loaded.collection<FixedAsset>(fixedAssetPullName);
    const salesInvoices1 =
      loaded.collection<SalesInvoice>(workEffortBillingPullName) ?? [];
    const salesInvoices2 =
      loaded.collection<SalesInvoice>(serviceEntryPullName) ?? [];
    this.salesInvoices = new Set([...salesInvoices1, ...salesInvoices2]);
  }
}