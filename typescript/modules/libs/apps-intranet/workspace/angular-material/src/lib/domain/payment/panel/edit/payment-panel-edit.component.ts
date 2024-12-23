import { format } from 'date-fns';
import { Component, HostBinding, OnInit } from '@angular/core';
import { RoleType } from '@allors/system/workspace/meta';
import {
  IObject,
  IPullResult,
  Pull,
  Initializer,
  SharedPullHandler,
} from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  AllorsCustomEditExtentPanelComponent,
  NavigationService,
  PanelService,
  ScopedService,
} from '@allors/base/workspace/angular/application';
import {
  Action,
  DisplayService,
  MetaService,
  RefreshService,
  SharedPullService,
  Table,
  TableConfig,
  TableRow,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { PeriodSelection } from '@allors/base/workspace/angular-material/foundation';
import {
  DeleteActionService,
  IconService,
  ViewActionService,
} from '@allors/base/workspace/angular-material/application';
import {
  Invoice,
  Order,
  Payment,
  PurchaseInvoice,
  SalesInvoice,
} from '@allors/default/workspace/domain';

interface Row extends TableRow {
  object: IObject;
  date: string;
  amount: string;
}

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'payment-panel-edit',
  templateUrl: './payment-panel-edit.component.html',
})
export class PaymentPanelEditComponent
  extends AllorsCustomEditExtentPanelComponent
  implements SharedPullHandler, OnInit
{
  override readonly panelKind = 'Extent';

  override readonly panelMode = 'Edit';
  invoicePullName: string;
  orderPullName: string;
  order: any;
  invoice: any;

  override get panelId() {
    return this.m?.Payment.tag;
  }

  @HostBinding('class.expanded-panel')
  get expandedPanelClass() {
    return true;
    // return this.panel.isExpanded;
  }

  get icon() {
    return this.iconService.icon(this.m.Payment);
  }

  get initializer(): Initializer {
    if (this.order) {
      return {
        propertyType: this.m.PaymentApplication.Order,
        id: this.scoped.id,
      };
    }
    if (this.invoice) {
      return {
        propertyType: this.m.PaymentApplication.Invoice,
        id: this.scoped.id,
      };
    }

    return null;
  }

  title = 'Payments';

  m: M;

  periodSelection: PeriodSelection = PeriodSelection.Current;

  table: Table<Row>;
  delete: Action;
  view: Action;

  objects: Payment[];

  display: RoleType[];

  receive: boolean;

  constructor(
    scopedService: ScopedService,
    panelService: PanelService,
    sharedPullService: SharedPullService,
    refreshService: RefreshService,
    metaService: MetaService,
    workspaceService: WorkspaceService,
    public navigation: NavigationService,
    public deleteService: DeleteActionService,
    public viewService: ViewActionService,
    private iconService: IconService,
    private displayService: DisplayService
  ) {
    super(scopedService, panelService, sharedPullService, refreshService);
    this.m = workspaceService.workspace.configuration.metaPopulation as M;

    panelService.register(this);
    sharedPullService.register(this);
  }

  ngOnInit() {
    this.display = this.displayService.primary(this.m.Payment);

    this.delete = this.deleteService.delete();
    this.view = this.viewService.view();

    const tableConfig: TableConfig = {
      selection: true,
      columns: [{ name: 'date' }, { name: 'amount' }],
      actions: [this.view, this.delete],
      defaultAction: this.view,
      autoSort: true,
      autoFilter: true,
    };

    this.table = new Table(tableConfig);
  }

  onPreSharedPull(pulls: Pull[], prefix?: string) {
    const { m } = this;
    const { pullBuilder: p } = m;

    if (this.panelEnabled) {
      const id = this.scoped.id;

      pulls.push(
        p.PaymentApplication({
          name: prefix,
          predicate: {
            kind: 'Equals',
            propertyType: m.PaymentApplication.Invoice,
            objectId: id,
          },
          select: {
            PaymentWherePaymentApplication: {
              include: {
                Sender: {},
                PaymentMethod: {},
              },
            },
          },
        }),
        p.PaymentApplication({
          name: prefix,
          predicate: {
            kind: 'Equals',
            propertyType: m.PaymentApplication.Order,
            objectId: id,
          },
          select: {
            PaymentWherePaymentApplication: {
              include: {
                Sender: {},
                PaymentMethod: {},
              },
            },
          },
        }),
        p.Invoice({
          objectId: id,
          include: {
            SalesInvoice_SalesInvoiceType: {},
            PurchaseInvoice_PurchaseInvoiceType: {},
          },
        }),
        p.Order({
          objectId: id,
        })
      );
    }
  }

  onPostSharedPull(pullResult: IPullResult, prefix?: string) {
    if (this.panelEnabled) {
      this.enabled = this.enabler ? this.enabler() : true;

      this.invoice = pullResult.object<Invoice>(this.m.Invoice);

      if (this.invoice) {
        if (this.invoice?.strategy.cls === this.m.SalesInvoice) {
          const salesInvoice = this.invoice as SalesInvoice;
          this.receive =
            salesInvoice.SalesInvoiceType.UniqueId ===
            '92411bf1-835e-41f8-80af-6611efce5b32';
        }

        if (this.invoice?.strategy.cls === this.m.PurchaseInvoice) {
          const salesInvoice = this.invoice as PurchaseInvoice;
          this.receive =
            salesInvoice.PurchaseInvoiceType.UniqueId ===
            '0187d927-81f5-4d6a-9847-58b674ad3e6a';
        }
      } else {
        this.order = pullResult.object<Order>(this.m.Order);
        if (this.order?.strategy.cls === this.m.SalesOrder) {
          this.receive = true;
        }

        if (this.order?.strategy.cls === this.m.PurchaseOrder) {
          this.receive = false;
        }
      }

      this.objects = pullResult.collection<Payment>(prefix) ?? [];

      this.table.data = this.objects.map((v) => {
        const row: Row = {
          object: v,
          date:
            v.EffectiveDate != null
              ? format(v.EffectiveDate, 'dd-MM-yyyy')
              : '',
          amount: v.Amount,
        };
        return row;
      });
    }
  }

  toggle() {
    this.panelService.stopEdit().subscribe();
  }
}
