import { Component, Self, OnInit, HostBinding } from '@angular/core';
import { formatDistance } from 'date-fns';

import { M } from '@allors/default/workspace/meta';
import { SalesInvoice } from '@allors/workspace/domain/default';
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
} from '@allors/workspace/angular/base';
import { WorkspaceService } from '@allors/workspace/angular/core';

interface Row extends TableRow {
  object: SalesInvoice;
  number: string;
  state: string;
  totalExVat: string;
  customer: string;
  lastModifiedDate: string;
}

@Component({
  selector: 'salesinvoice-overview-panel',
  templateUrl: './salesinvoice-overview-panel.component.html',
  providers: [PanelService],
})
export class SalesInvoiceOverviewPanelComponent implements OnInit {
  @HostBinding('class.expanded-panel') get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  m: M;

  objects: SalesInvoice[] = [];

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

    this.panel.name = 'salesinvoice';
    this.panel.title = 'Sales Invoices';
    this.panel.icon = 'money';
    this.panel.expandable = true;

    const sort = true;
    this.table = new Table({
      selection: true,
      columns: [
        { name: 'number', sort },
        { name: 'state', sort },
        { name: 'totalExVat', sort },
        { name: 'customer', sort },
        { name: 'lastModifiedDate', sort },
      ],
      actions: [this.overviewService.overview(), this.delete],
      defaultAction: this.overviewService.overview(),
      autoSort: true,
      autoFilter: true,
    });

    const assetPullName = `${this.panel.name}_${this.m.SalesInvoice.tag}_fixedasset`;
    const customerPullName = `${this.panel.name}_${this.m.SalesInvoice.tag}_customer`;

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
            SalesInvoiceItemsWhereSerialisedItem: {
              SalesInvoiceWhereSalesInvoiceItem: {
                include: {
                  SalesInvoiceState: x,
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
            SalesInvoicesWhereBillToCustomer: {
              include: {
                SalesInvoiceState: x,
                BillToCustomer: x,
              },
            },
          },
        })
      );
    };

    this.panel.onPulled = (loaded) => {
      const fromAsset = loaded.collection<SalesInvoice>(assetPullName);
      const fromParty = loaded.collection<SalesInvoice>(customerPullName);

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
          number: v.InvoiceNumber,
          customer: v.BillToCustomer.DisplayName,
          totalExVat: v.TotalExVat,
          state: v.SalesInvoiceState ? v.SalesInvoiceState.Name : '',
          lastModifiedDate: formatDistance(
            new Date(v.LastModifiedDate),
            new Date()
          ),
        } as Row;
      });
    };
  }
}
