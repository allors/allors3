import { Component, OnInit, Self, HostBinding } from '@angular/core';

import { M } from '@allors/default/workspace/meta';
import {
  WorkEffort,
  WorkEffortPurchaseOrderItemAssignment,
} from '@allors/default/workspace/domain';
import {
  Action,
  DeleteService,
  EditService,
  NavigationService,
  ObjectData,
  OldPanelService,
  RefreshService,
  Table,
  TableRow,
} from '@allors/base/workspace/angular/foundation';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';

interface Row extends TableRow {
  object: WorkEffortPurchaseOrderItemAssignment;
  supplier: string;
  orderNumber: string;
  description: string;
  quantity: number;
}

@Component({
  selector: 'workeffortpurchaseorderitemassignment-overview-panel',
  templateUrl: './workeffortpoiassignment-overview-panel.component.html',
  providers: [OldPanelService],
})
export class WorkEffortPOIAssignmentOverviewPanelComponent implements OnInit {
  workEffort: WorkEffort;

  @HostBinding('class.expanded-panel') get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  m: M;

  objects: WorkEffortPurchaseOrderItemAssignment[] = [];

  delete: Action;
  edit: Action;
  table: Table<TableRow>;

  get createData(): ObjectData {
    return {
      associationId: this.panel.manager.id,
      associationObjectType: this.panel.manager.objectType,
    };
  }

  constructor(
    @Self() public panel: OldPanelService,
    public workspaceService: WorkspaceService,
    public refreshService: RefreshService,
    public navigation: NavigationService,
    public deleteService: DeleteService,
    public editService: EditService
  ) {
    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;
  }

  ngOnInit() {
    this.panel.name = 'workeffortpurchaseorderitemassignment';
    // this.panel.title = 'PurchaseOrder Item Assignment';
    this.panel.title = 'Subcontracted Work';
    this.panel.icon = 'work';
    this.panel.expandable = true;

    this.edit = this.editService.edit();
    this.delete = this.deleteService.delete(this.panel.manager.context);

    this.table = new Table({
      selection: true,
      columns: [
        { name: 'supplier' },
        { name: 'orderNumber' },
        { name: 'description' },
        { name: 'quantity' },
      ],
      actions: [this.edit, this.delete],
      defaultAction: this.edit,
      autoSort: true,
      autoFilter: true,
    });

    const pullName = `${this.panel.name}_${this.m.WorkEffortPurchaseOrderItemAssignment.tag}`;

    this.panel.onPull = (pulls) => {
      const m = this.m;
      const { pullBuilder: pull } = m;
      const x = {};

      const id = this.panel.manager.id;

      pulls.push(
        pull.WorkEffort({
          name: pullName,
          objectId: id,
          select: {
            WorkEffortPurchaseOrderItemAssignmentsWhereAssignment: {
              include: {
                PurchaseOrder: {
                  TakenViaSupplier: x,
                  TakenViaSubcontractor: x,
                },
                PurchaseOrderItem: x,
              },
            },
          },
        }),
        pull.WorkEffort({
          objectId: id,
        })
      );
    };

    this.panel.onPulled = (loaded) => {
      this.workEffort = loaded.object<WorkEffort>(this.m.WorkEffort);
      this.objects =
        loaded.collection<WorkEffortPurchaseOrderItemAssignment>(pullName);

      this.table.total = this.objects?.length ?? 0;
      this.table.data = this.objects?.map((v) => {
        return {
          object: v,
          supplier:
            (v.PurchaseOrder.TakenViaSupplier &&
              v.PurchaseOrder.TakenViaSupplier.DisplayName) ||
            (v.PurchaseOrder.TakenViaSubcontractor &&
              v.PurchaseOrder.TakenViaSubcontractor.DisplayName),
          description: v.PurchaseOrderItem.Description,
          orderNumber: v.PurchaseOrder.OrderNumber,
          quantity: v.Quantity,
        } as Row;
      });
    };
  }
}
