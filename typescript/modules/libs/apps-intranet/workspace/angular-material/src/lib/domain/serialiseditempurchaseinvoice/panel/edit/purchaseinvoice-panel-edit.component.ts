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
  RefreshService,
  SharedPullService,
  Table,
  TableConfig,
  TableRow,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { PeriodSelection } from '@allors/base/workspace/angular-material/foundation';
import {
  IconService,
  ViewActionService,
} from '@allors/base/workspace/angular-material/application';
import { PurchaseInvoice } from '@allors/default/workspace/domain';

interface Row extends TableRow {
  object: IObject;
  number: string;
}

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'purchaseinvoice-panel-edit',
  templateUrl: './purchaseinvoice-panel-edit.component.html',
})
export class PurchaseInvoicePanelEditComponent
  extends AllorsCustomEditExtentPanelComponent
  implements SharedPullHandler, OnInit
{
  override readonly panelKind = 'Extent';

  override readonly panelMode = 'Edit';

  override get panelId() {
    return this.m?.PurchaseInvoice.tag;
  }

  @HostBinding('class.expanded-panel')
  get expandedPanelClass() {
    return true;
    // return this.panel.isExpanded;
  }

  get icon() {
    return this.iconService.icon(this.m.PurchaseInvoice);
  }

  get initializer(): Initializer {
    return null;
  }

  title = 'PurchaseInvoices';

  m: M;

  periodSelection: PeriodSelection = PeriodSelection.Current;

  table: Table<Row>;
  view: Action;

  objects: PurchaseInvoice[];

  display: RoleType[];

  constructor(
    scopedService: ScopedService,
    panelService: PanelService,
    sharedPullService: SharedPullService,
    refreshService: RefreshService,
    workspaceService: WorkspaceService,
    public navigation: NavigationService,
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
    this.display = this.displayService.primary(this.m.PurchaseInvoice);

    this.view = this.viewService.view();

    const tableConfig: TableConfig = {
      selection: true,
      columns: [{ name: 'number' }],
      actions: [this.view],
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
        p.SerialisedItem({
          objectId: id,
          name: prefix,
          select: {
            PurchaseInvoiceItemsWhereSerialisedItem: {
              PurchaseInvoiceWherePurchaseInvoiceItem: {},
            },
          },
        }),
        p.SerialisedItem({
          objectId: id,
          name: `${prefix}_items`,
          select: {
            PurchaseInvoiceItemsWhereSerialisedItem: {
              include: {
                InvoiceItemType: {},
              },
            },
          },
        })
      );
    }
  }

  onPostSharedPull(pullResult: IPullResult, prefix?: string) {
    if (this.panelEnabled) {
      this.enabled = this.enabler ? this.enabler() : true;

      const unfiltered = pullResult.collection<PurchaseInvoice>(prefix) ?? [];
      this.objects = unfiltered.filter((v) => {
        return v.PurchaseInvoiceItems?.find(
          (i) =>
            i.InvoiceItemType.UniqueId ===
              'ff2b943d-57c9-4311-9c56-9ff37959653b' ||
            i.InvoiceItemType.UniqueId ===
              '0d07f778-2735-44cb-8354-fb887ada42ad'
        );
      });

      this.table.data = this.objects.map((v) => {
        const row: Row = {
          object: v,
          number: v.InvoiceNumber,
        };
        return row;
      });
    }
  }

  toggle() {
    this.panelService.stopEdit().subscribe();
  }
}
