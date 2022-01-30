import { Component, Self, OnInit, HostBinding } from '@angular/core';
import { formatDistance } from 'date-fns';

import { M } from '@allors/default/workspace/meta';
import { SerialisedItem, WorkEffort } from '@allors/default/workspace/domain';
import {
  Action,
  DeleteService,
  NavigationService,
  ObjectData,
  OldPanelService,
  RefreshService,
  Table,
  TableRow,
  OverviewService,
} from '@allors/base/workspace/angular/foundation';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';

interface Row extends TableRow {
  object: WorkEffort;
  number: string;
  name: string;
  state: string;
  customer: string;
  cost: string;
  lastModifiedDate: string;
}

@Component({
  selector: 'worktask-overview-panel',
  templateUrl: './worktask-overview-panel.component.html',
  providers: [OldPanelService],
})
export class WorkTaskOverviewPanelComponent implements OnInit {
  serialisedItem: SerialisedItem;

  @HostBinding('class.expanded-panel') get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  m: M;

  objects: WorkEffort[] = [];

  delete: Action;
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
    public overviewService: OverviewService,
    public deleteService: DeleteService
  ) {
    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;
  }

  ngOnInit() {
    const m = this.m;

    this.delete = this.deleteService.delete(this.panel.manager.context);

    this.panel.name = 'workeffort';
    this.panel.title = 'Work Efforts';
    this.panel.icon = 'work';
    this.panel.expandable = true;

    const sort = true;
    this.table = new Table({
      selection: true,
      columns: [
        { name: 'number', sort },
        { name: 'name', sort },
        { name: 'state', sort },
        { name: 'customer', sort },
        { name: 'cost', sort },
        { name: 'lastModifiedDate', sort },
      ],
      actions: [this.overviewService.overview(), this.delete],
      defaultAction: this.overviewService.overview(),
      autoSort: true,
      autoFilter: true,
    });

    const customerPullName = `${this.panel.name}_${m.WorkEffort.tag}_customer`;
    const contactPullName = `${this.panel.name}_${m.WorkEffort.tag}_contact`;
    const assetPullName = `${this.panel.name}_${m.WorkEffort.tag}_fixedasset`;

    this.panel.onPull = (pulls) => {
      const m = this.m;
      const { pullBuilder: pull } = m;
      const x = {};

      const id = this.panel.manager.id;

      pulls.push(
        pull.Party({
          name: customerPullName,
          objectId: id,
          select: {
            WorkEffortsWhereCustomer: {
              include: {
                WorkEffortState: x,
                Customer: x,
              },
            },
          },
        }),
        pull.Person({
          name: contactPullName,
          objectId: id,
          select: {
            WorkEffortsWhereContactPerson: {
              include: {
                WorkEffortState: x,
                Customer: x,
              },
            },
          },
        }),
        pull.SerialisedItem({
          name: assetPullName,
          objectId: id,
          select: {
            WorkEffortFixedAssetAssignmentsWhereFixedAsset: {
              Assignment: {
                include: {
                  WorkEffortState: x,
                  Customer: x,
                },
              },
            },
          },
        }),
        pull.SerialisedItem({
          objectId: id,
        })
      );
    };

    this.panel.onPulled = (loaded) => {
      this.serialisedItem = loaded.object<SerialisedItem>(m.SerialisedItem);
      const fromCustomer = loaded.collection<WorkEffort>(customerPullName);
      const fromContact = loaded.collection<WorkEffort>(contactPullName);
      const fromAsset = loaded.collection<WorkEffort>(assetPullName);

      if (fromCustomer != null && fromCustomer.length > 0) {
        this.objects = fromCustomer;
      }

      if (fromContact != null && fromContact.length > 0) {
        this.objects = fromContact;
      }

      if (fromAsset != null && fromAsset.length > 0) {
        this.objects = fromAsset;
      }

      this.table.total = this.objects?.length ?? 0;
      this.table.data = this.objects?.map((v) => {
        return {
          object: v,
          number: v.WorkEffortNumber,
          name: v.Name,
          customer: v.Customer.DisplayName,
          state: v.WorkEffortState ? v.WorkEffortState.Name : '',
          cost: v.TotalCost,
          lastModifiedDate: formatDistance(
            new Date(v.LastModifiedDate),
            new Date()
          ),
        } as Row;
      });
    };
  }
}
