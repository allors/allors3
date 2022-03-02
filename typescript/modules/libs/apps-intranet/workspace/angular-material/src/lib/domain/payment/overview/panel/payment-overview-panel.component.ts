import { format } from 'date-fns';
import { Component, HostBinding, OnInit } from '@angular/core';
import {
  AssociationType,
  Composite,
  humanize,
  RoleType,
} from '@allors/system/workspace/meta';
import {
  IObject,
  IPullResult,
  Pull,
  Initializer,
  SharedPullHandler,
} from '@allors/system/workspace/domain';
import {
  Action,
  SharedPullService,
  RefreshService,
  WorkspaceService,
  DisplayService,
  TableRow,
  Table,
  TableConfig,
  MetaService,
} from '@allors/base/workspace/angular/foundation';
import {
  NavigationService,
  AllorsEditExtentPanelComponent,
  PanelService,
  ScopedService,
} from '@allors/base/workspace/angular/application';
import { PeriodSelection } from '@allors/base/workspace/angular-material/foundation';
import {
  DeleteActionService,
  IconService,
  ViewActionService,
} from '@allors/base/workspace/angular-material/application';
import {
  Invoice,
  Payment,
  Period,
  PurchaseInvoice,
  SalesInvoice,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';

interface Row extends TableRow {
  object: Payment;
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

  objects: IObject[];
  filtered: IObject[];

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

    this.objects = pullResult.collection<IObject>(prefix) ?? [];
    this.updateFilter();
    this.refreshTable();
  }

  onPeriodSelectionChange(newPeriodSelection: PeriodSelection) {
    this.periodSelection = newPeriodSelection;

    if (this.objects != null) {
      this.updateFilter();
      this.refreshTable();
    }
  }

  toggle() {
    this.panelService.stopEdit().subscribe();
  }

  private updateFilter() {
    if (!this.hasPeriod) {
      this.filtered = this.objects;
      return;
    }

    const now = new Date(Date.now());
    switch (this.periodSelection) {
      case PeriodSelection.Current:
        this.filtered = this.objects.filter((v: Period) => {
          if (v.ThroughDate) {
            return v.FromDate < now && v.ThroughDate > now;
          } else {
            return v.FromDate < now;
          }
        });
        break;
      case PeriodSelection.Inactive:
        this.filtered = this.objects.filter((v: Period) => {
          if (v.ThroughDate) {
            return v.FromDate > now || v.ThroughDate < now;
          } else {
            return v.FromDate > now;
          }
        });
        break;
      default:
        this.filtered = this.objects;
        break;
    }
  }

  private refreshTable() {
    this.table.total = this.filtered.length;
    this.table.data = this.filtered.map((v) => {
      const row: TableRow = {
        object: v,
      };
      if (this.objectType.isInterface) {
        row['type'] = humanize(v.strategy.cls.singularName);
      }

      if (this.display.length === 0) {
        const display = this.displayService.name(v.strategy.cls);
        if (display != null) {
          row['name'] = v.strategy.getUnitRole(display);
        }
      }

      if (this.include) {
        const include = this.include.isRoleType
          ? v.strategy.getCompositeRole(this.include as RoleType)
          : v.strategy.getCompositeAssociation(this.include as AssociationType);
        for (const w of this.includeDisplay) {
          if (w.objectType.isUnit) {
            row[w.name] = include.strategy.getUnitRole(w);
          } else {
            const role = include.strategy.getCompositeRole(w);
            if (role) {
              const roleName = this.displayService.name(role.strategy.cls);
              row[w.name] = role.strategy.getUnitRole(roleName);
            } else {
              row[w.name] = '';
            }
          }
        }
      }

      for (const w of this.display) {
        if (w.objectType.isUnit) {
          row[w.name] = v.strategy.getUnitRole(w);
        } else {
          if (w.isOne) {
            const composite = v.strategy.getCompositeRole(w);
            if (composite) {
              const roleName = this.displayService.name(composite.strategy.cls);
              row[w.name] = composite.strategy.getUnitRole(roleName);
            } else {
              row[w.name] = '';
            }
          } else {
            const composites = v.strategy.getCompositesRole(w);
            if (composites.length > 0) {
              row[w.name] = composites
                .map((v) => {
                  const display = this.displayService.name(v.strategy.cls);
                  return v.strategy.getUnitRole(display);
                })
                .join(', ');
            } else {
              row[w.name] = '';
            }
          }
        }
      }
      if (this.hasPeriod) {
        const fromDate = v.strategy.getUnitRole(this.m.Period.FromDate) as Date;
        row['from'] = format(fromDate, 'dd-MM-yyyy');
        const throughDate = v.strategy.getUnitRole(
          this.m.Period.ThroughDate
        ) as Date;
        row['through'] =
          throughDate != null ? format(throughDate, 'dd-MM-yyyy') : '';
      }
      return row;
    });
  }
}
