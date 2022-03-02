import { format } from 'date-fns';
import { Component, HostBinding, OnInit } from '@angular/core';
import { Composite, RoleType } from '@allors/system/workspace/meta';
import {
  IObject,
  IPullResult,
  Pull,
  Initializer,
  SharedPullHandler,
} from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  AllorsEditExtentPanelComponent,
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
  selector: 'payment-overview-panel',
  templateUrl: './payment-overview-panel.component.html',
})
export class PaymentOverviewPanelComponent
  extends AllorsEditExtentPanelComponent
  implements SharedPullHandler, OnInit
{
  @HostBinding('class.expanded-panel')
  get expandedPanelClass() {
    return true;
    // return this.panel.isExpanded;
  }

  get icon() {
    return this.iconService.icon(this.objectType);
  }

  get initializer(): Initializer {
    return { propertyType: this.init, id: this.scoped.id };
  }

  get hasPeriod(): boolean {
    return this.objectType?.supertypes.has(this.m.Period);
  }

  m: M;

  periodSelection: PeriodSelection = PeriodSelection.Current;

  table: Table<Row>;
  delete: Action;
  view: Action;

  objects: Payment[];

  display: RoleType[];
  includeDisplay: RoleType[];

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
    super(
      scopedService,
      panelService,
      sharedPullService,
      refreshService,
      metaService
    );
    this.m = workspaceService.workspace.configuration.metaPopulation as M;

    panelService.register(this);
    sharedPullService.register(this);
  }

  ngOnInit() {
    this.display = this.displayService.primary(this.objectType);

    if (this.include) {
      const includeObjectType = this.include.objectType as Composite;
      this.includeDisplay = this.displayService.primary(includeObjectType);
    } else {
      this.includeDisplay = [];
    }

    this.delete = this.deleteService.delete();
    this.view = this.viewService.view();

    const sort = true;

    const tableConfig: TableConfig = {
      selection: true,
      columns: [{ name: 'date' }, { name: 'amount' }],
      actions: [this.view, this.delete],
      defaultAction: this.view,
      autoSort: true,
      autoFilter: true,
    };

    if (this.hasPeriod) {
      tableConfig.columns.push(
        ...[
          { name: 'from', sort },
          { name: 'through', sort },
        ]
      );
    }

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
        p.Invoice({
          objectId: id,
          include: {
            SalesInvoice_SalesInvoiceType: {},
            PurchaseInvoice_PurchaseInvoiceType: {},
          },
        })
      );
    }
  }

  onPostSharedPull(pullResult: IPullResult, prefix?: string) {
    this.enabled = this.enabler ? this.enabler() : true;

    const invoice = pullResult.object<Invoice>(this.m.Invoice);

    if (invoice.strategy.cls === this.m.SalesInvoice) {
      const salesInvoice = invoice as SalesInvoice;
      this.receive =
        salesInvoice.SalesInvoiceType.UniqueId ===
        '92411bf1-835e-41f8-80af-6611efce5b32';
    }

    if (invoice.strategy.cls === this.m.PurchaseInvoice) {
      const salesInvoice = invoice as PurchaseInvoice;
      this.receive =
        salesInvoice.PurchaseInvoiceType.UniqueId ===
        '0187d927-81f5-4d6a-9847-58b674ad3e6a';
    }

    this.objects = pullResult.collection<Payment>(prefix) ?? [];

    this.table.total = this.objects.length;
    this.table.data = this.objects.map((v) => {
      const row: Row = {
        object: v,
        date:
          v.EffectiveDate != null ? format(v.EffectiveDate, 'dd-MM-yyyy') : '',
        amount: v.Amount,
      };
      return row;
    });
  }

  toggle() {
    this.panelService.stopEdit().subscribe();
  }
}
