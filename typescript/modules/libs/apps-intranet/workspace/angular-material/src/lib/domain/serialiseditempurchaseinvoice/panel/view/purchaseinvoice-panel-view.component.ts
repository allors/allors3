import { Component, HostBinding, OnInit } from '@angular/core';
import { RoleType } from '@allors/system/workspace/meta';
import {
  IPullResult,
  Pull,
  Initializer,
  SharedPullHandler,
} from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  AllorsCustomViewExtentPanelComponent,
  PanelService,
  ScopedService,
} from '@allors/base/workspace/angular/application';
import {
  DisplayService,
  RefreshService,
  SharedPullService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { IconService } from '@allors/base/workspace/angular-material/application';
import { PurchaseInvoice } from '@allors/default/workspace/domain';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'purchaseinvoice-panel-view',
  templateUrl: './purchaseinvoice-panel-view.component.html',
})
export class PurchaseInvoicePanelViewComponent
  extends AllorsCustomViewExtentPanelComponent
  implements SharedPullHandler, OnInit
{
  override readonly panelKind = 'Extent';

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
    // TODO: Martien
    // return { propertyType: this.init, id: this.scoped.id };
  }

  title = 'PurchaseInvoices';

  m: M;

  objects: PurchaseInvoice[];

  display: RoleType[];
  description: string;

  constructor(
    scopedService: ScopedService,
    panelService: PanelService,
    sharedPullService: SharedPullService,
    refreshService: RefreshService,
    workspaceService: WorkspaceService,
    private displayService: DisplayService,
    private iconService: IconService
  ) {
    super(scopedService, panelService, sharedPullService, refreshService);

    this.m = workspaceService.workspace.configuration.metaPopulation as M;
    panelService.register(this);
    sharedPullService.register(this);
  }

  ngOnInit() {
    this.display = this.displayService.primary(this.m.PurchaseInvoice);
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
              PurchaseInvoiceWherePurchaseInvoiceItem: {
                include: {
                  PurchaseInvoiceItems: {
                    InvoiceItemType: {},
                  },
                },
              },
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
    this.enabled = this.enabler ? this.enabler() : true;

    const unfiltered = pullResult.collection<PurchaseInvoice>(prefix) ?? [];

    this.objects = unfiltered.filter((v) => {
      return v.PurchaseInvoiceItems?.find(
        (i) =>
          i.InvoiceItemType.UniqueId ===
            'ff2b943d-57c9-4311-9c56-9ff37959653b' ||
          i.InvoiceItemType.UniqueId === '0d07f778-2735-44cb-8354-fb887ada42ad'
      );
    });

    this.description = `${this.objects.length} PurchaseInvoices`;
  }

  toggle() {
    this.panelService.startEdit(this.panelId).subscribe();
  }
}
