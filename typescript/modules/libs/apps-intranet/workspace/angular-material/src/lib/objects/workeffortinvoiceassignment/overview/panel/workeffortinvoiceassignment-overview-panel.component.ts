import { Component, Self, OnInit, HostBinding } from '@angular/core';

import { M } from '@allors/default/workspace/meta';
import {
  WorkEffort,
  WorkEffortInvoiceItemAssignment,
  WorkEffortInvoiceItem,
} from '@allors/default/workspace/domain';
import {
  Action,
  DeleteService,
  EditService,
  NavigationService,
  ObjectData,
  PanelService,
  RefreshService,
  Table,
  TableRow,
} from '@allors/base/workspace/angular/foundation';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';

interface Row extends TableRow {
  object: WorkEffortInvoiceItemAssignment;
  type: string;
  amount: string;
}

@Component({
  selector: 'workeffortinvoiceitemeassignment-overview-panel',
  templateUrl: './workeffortinvoiceassignment-overview-panel.component.html',
  providers: [PanelService],
})
export class WorkEffortInvoiceItemAssignmentOverviewPanelComponent
  implements OnInit
{
  workEffort: WorkEffort;

  @HostBinding('class.expanded-panel') get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  m: M;

  objects: WorkEffortInvoiceItemAssignment[] = [];

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
    @Self() public panel: PanelService,

    public workspaceService: WorkspaceService,
    public refreshService: RefreshService,
    public navigation: NavigationService,
    public deleteService: DeleteService,
    public editService: EditService
  ) {
    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;
  }

  ngOnInit() {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.edit = this.editService.edit();
    this.delete = this.deleteService.delete(this.panel.manager.context);

    this.panel.name = 'workeffortinvoiceitemassignment';
    this.panel.title = 'Invoice Items';
    this.panel.icon = 'business';
    this.panel.expandable = true;

    const sort = true;
    this.table = new Table({
      selection: true,
      columns: [{ name: 'type' }, { name: 'amount' }],
      actions: [this.edit, this.delete],
      defaultAction: this.edit,
      autoSort: true,
      autoFilter: true,
    });

    const pullName = `${this.panel.name}_${this.m.WorkEffortInvoiceItemAssignment.tag}`;

    this.panel.onPull = (pulls) => {
      const id = this.panel.manager.id;

      pulls.push(
        pull.WorkEffort({
          name: pullName,
          objectId: id,
          select: {
            WorkEffortInvoiceItemAssignmentsWhereAssignment: {
              include: {
                WorkEffortInvoiceItem: {
                  InvoiceItemType: x,
                },
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
        loaded.collection<WorkEffortInvoiceItemAssignment>(pullName);

      this.table.total = (loaded.value(`${pullName}_total`) ??
        this.objects?.length ??
        0) as number;
      this.table.data = this.objects?.map((v) => {
        return {
          object: v,
          type: `${
            v.WorkEffortInvoiceItem.InvoiceItemType &&
            v.WorkEffortInvoiceItem.InvoiceItemType.Name
          }`,
          amount: v.WorkEffortInvoiceItem.Amount,
        } as Row;
      });
    };
  }
}
