import { Component, Self, HostBinding } from '@angular/core';

import { M } from '@allors/workspace/meta/default';
import { Organisation, InternalOrganisation, PurchaseOrder } from '@allors/workspace/domain/default';
import { Action, DeleteService, MethodService, NavigationService, ObjectData, ObjectService, PanelService, RefreshService, Table, TableRow, TestScope, OverviewService } from '@allors/workspace/angular/base';
import { SessionService } from '@allors/workspace/angular/core';

import { PrintService } from '../../../../actions/print/print.service';
import { FetcherService } from '../../../../services/fetcher/fetcher-service';

interface Row extends TableRow {
  object: PurchaseOrder;
  number: string;
  description: string;
  reference: string;
  totalExVat: string;
  state: string;
  shipmentState: string;
  paymentState: string;
}

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'purchaseorder-overview-panel',
  templateUrl: './purchaseorder-overview-panel.component.html',
  providers: [SessionService, PanelService],
})
export class PurchaseOrderOverviewPanelComponent extends TestScope {
  internalOrganisation: Organisation;

  @HostBinding('class.expanded-panel') get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  m: M;

  objects: PurchaseOrder[];
  table: Table<Row>;

  delete: Action;
  invoice: Action;
  edit: Action;

  get createData(): ObjectData {
    return {
      associationId: this.panel.manager.id,
      associationObjectType: this.panel.manager.objectType,
    };
  }

  constructor(
    @Self() public allors: SessionService,
    @Self() public panel: PanelService,

    public objectService: ObjectService,
    public methodService: MethodService,
    public refreshService: RefreshService,
    public navigationService: NavigationService,
    public overviewService: OverviewService,
    public deleteService: DeleteService,
    public printService: PrintService,
    private fetcher: FetcherService
  ) {
    super();

    this.m = this.allors.workspace.configuration.metaPopulation as M;

    this.panel.name = 'purchaseorder';
    this.panel.title = 'Purchase Orders';
    this.panel.icon = 'message';
    this.panel.expandable = true;

    this.delete = this.deleteService.delete(this.panel.manager.context);
    this.invoice = methodService.create(allors.context, this.m.PurchaseOrder.Invoice, { name: 'Invoice' });

    const sort = true;
    this.table = new Table({
      selection: true,
      columns: [
        { name: 'number', sort },
        { name: 'description', sort },
        { name: 'reference', sort },
        { name: 'totalExVat', sort },
        { name: 'state', sort },
        { name: 'shipmentState', sort },
        { name: 'paymentState', sort },
      ],
      actions: [this.overviewService.overview(), this.printService.print(), this.invoice],
      defaultAction: this.overviewService.overview(),
      autoSort: true,
      autoFilter: true,
    });

    if (this.panel.manager.objectType === this.metaService.m.PurchaseInvoice) {
      this.table.actions.push(this.delete);
    }

    const pullName = `${this.panel.name}_${this.m.PurchaseOrder.name}`;

    this.panel.onPull = (pulls) => {
      const { x, pull } = this.metaService;
      const { id } = this.panel.manager;

      pulls.push(
        this.fetcher.internalOrganisation,
        pull.Organisation({
          name: pullName,
          objectId: id,
          select: {
            PurchaseOrdersWhereTakenViaSupplier: {
              include: {
                PurchaseOrderState: x,
                PurchaseOrderShipmentState: x,
                PurchaseOrderPaymentState: x,
                PrintDocument: {
                  Media: x,
                },
              },
            },
          },
        })
      );
    };

    this.panel.onPulled = (loaded) => {
      this.internalOrganisation = loaded.object<InternalOrganisation>(m.InternalOrganisation);

      const purchaseOrders = loaded.collections[pullName] as PurchaseOrder[];
      this.objects = purchaseOrders.filter((v) => v.OrderedBy === this.internalOrganisation);
      this.objects.sort((a, b) => (a.OrderNumber > b.OrderNumber ? 1 : b.OrderNumber > a.OrderNumber ? -1 : 0));

      if (this.objects) {
        this.table.total = loaded.values[`${pullName}_total`] || this.objects.length;
        this.table.data = this.objects.map((v) => {
          return {
            object: v,
            number: v.OrderNumber,
            description: v.Description,
            reference: v.CustomerReference,
            totalExVat: v.TotalExVat.toString(),
            state: v.PurchaseOrderState.Name,
            shipmentState: v.PurchaseOrderShipmentState.Name,
            paymentState: v.PurchaseOrderPaymentState.Name,
          } as Row;
        });
      }
    };
  }
}
