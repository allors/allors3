import { Component, Self, OnInit, HostBinding } from '@angular/core';
import { formatDistance } from 'date-fns';

import { M } from '@allors/workspace/meta/default';
import { SerialisedItem, WorkEffort } from '@allors/workspace/domain/default';
import { Action, DeleteService, NavigationService, ObjectData, PanelService, RefreshService, Table, TableRow, TestScope, OverviewService } from '@allors/workspace/angular/base';
import { WorkspaceService } from '@allors/workspace/angular/core';

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
  // tslint:disable-next-line:component-selector
  selector: 'worktask-overview-panel',
  templateUrl: './worktask-overview-panel.component.html',
  providers: [PanelService],
})
export class WorkTaskOverviewPanelComponent extends TestScope implements OnInit {
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
    @Self() public panel: PanelService,
    public workspaceService: WorkspaceService,
    public refreshService: RefreshService,
    public navigation: NavigationService,
    public overviewService: OverviewService,
    public deleteService: DeleteService
  ) {
    super();

    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;
  }

  ngOnInit() {
    const m = this.m;

    this.delete = this.deleteService.delete(this.panel.manager.client, this.panel.manager.session);

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

      if (fromCustomer !== undefined && fromCustomer.length > 0) {
        this.objects = fromCustomer;
      }

      if (fromContact !== undefined && fromContact.length > 0) {
        this.objects = fromContact;
      }

      if (fromAsset !== undefined && fromAsset.length > 0) {
        this.objects = fromAsset;
      }

      if (this.objects) {
        this.table.total = this.objects.length;
        this.table.data = this.objects.map((v) => {
          return {
            object: v,
            number: v.WorkEffortNumber,
            name: v.Name,
            customer: v.Customer.DisplayName,
            state: v.WorkEffortState ? v.WorkEffortState.Name : '',
            cost: v.TotalCost,
            lastModifiedDate: formatDistance(new Date(v.LastModifiedDate), new Date()),
          } as Row;
        });
      }
    };
  }
}
