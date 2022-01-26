import { Component, Self, OnInit, HostBinding } from '@angular/core';
import { formatDistance } from 'date-fns';

import { M } from '@allors/default/workspace/meta';
import { SalesOrder } from '@allors/default/workspace/domain';
import {
  Action,
  DeleteService,
  NavigationService,
  ObjectData,
  PanelService,
  RefreshService,
  Table,
  TableRow,
  OverviewService,
} from '@allors/base/workspace/angular/foundation';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';

interface Row extends TableRow {
  object: SalesOrder;
  number: string;
  state: string;
  customer: string;
  lastModifiedDate: string;
}

@Component({
  selector: 'salesorder-overview-panel',
  templateUrl: './salesorder-overview-panel.component.html',
  providers: [PanelService],
})
export class SalesOrderOverviewPanelComponent implements OnInit {
  @HostBinding('class.expanded-panel') get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  m: M;

  objects: SalesOrder[] = [];

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
    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;
  }

  ngOnInit() {
    this.delete = this.deleteService.delete(this.panel.manager.context);

    this.panel.name = 'salesorder';
    this.panel.title = 'Sales Orders';
    this.panel.icon = 'money';
    this.panel.expandable = true;

    const sort = true;
    this.table = new Table({
      selection: true,
      columns: [
        { name: 'number', sort },
        { name: 'state', sort },
        { name: 'customer', sort },
        { name: 'lastModifiedDate', sort },
      ],
      actions: [this.overviewService.overview(), this.delete],
      defaultAction: this.overviewService.overview(),
      autoSort: true,
      autoFilter: true,
    });

    const assetPullName = `${this.panel.name}_${this.m.SalesOrder.tag}_fixedasset`;
    const customerPullName = `${this.panel.name}_${this.m.SalesOrder.tag}_customer`;

    this.panel.onPull = (pulls) => {
      const m = this.m;
      const { pullBuilder: pull } = m;
      const x = {};

      const id = this.panel.manager.id;

      pulls.push(
        pull.SerialisedItem({
          name: assetPullName,
          objectId: id,
          select: {
            SalesOrderItemsWhereSerialisedItem: {
              SalesOrderWhereSalesOrderItem: {
                include: {
                  SalesOrderState: x,
                  BillToCustomer: x,
                },
              },
            },
          },
        }),
        pull.Party({
          name: customerPullName,
          objectId: id,
          select: {
            SalesOrdersWhereBillToCustomer: {
              include: {
                SalesOrderState: x,
                BillToCustomer: x,
              },
            },
          },
        })
      );
    };

    this.panel.onPulled = (loaded) => {
      const fromAsset = loaded.collection<SalesOrder>(assetPullName);
      const fromParty = loaded.collection<SalesOrder>(customerPullName);

      if (fromAsset != null && fromAsset.length > 0) {
        this.objects = fromAsset;
      }

      if (fromParty != null && fromParty.length > 0) {
        this.objects = fromParty;
      }

      this.table.total = this.objects?.length ?? 0;
      this.table.data = this.objects?.map((v) => {
        return {
          object: v,
          number: v.OrderNumber,
          customer: v.BillToCustomer.DisplayName,
          state: v.SalesOrderState ? v.SalesOrderState.Name : '',
          lastModifiedDate: formatDistance(
            new Date(v.LastModifiedDate),
            new Date()
          ),
        } as Row;
      });
    };
  }
}