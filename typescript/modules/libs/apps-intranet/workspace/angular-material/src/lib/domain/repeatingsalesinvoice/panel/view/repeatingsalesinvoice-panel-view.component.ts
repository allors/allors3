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
import { RepeatingSalesInvoice } from '@allors/default/workspace/domain';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'repeatingsalesinvoice-panel-view',
  templateUrl: './repeatingsalesinvoice-panel-view.component.html',
})
export class RepeatingSalesInvoicePanelViewComponent
  extends AllorsCustomViewExtentPanelComponent
  implements SharedPullHandler, OnInit
{
  override readonly panelKind = 'Extent';

  override get panelId() {
    return this.m?.RepeatingSalesInvoice.tag;
  }

  @HostBinding('class.expanded-panel')
  get expandedPanelClass() {
    return true;
    // return this.panel.isExpanded;
  }

  get icon() {
    return this.iconService.icon(this.m.RepeatingSalesInvoice);
  }

  get initializer(): Initializer {
    return null;
    // TODO: Martien
    // return { propertyType: this.init, id: this.scoped.id };
  }

  title = 'Repeating Sales Invoices';

  m: M;

  objects: RepeatingSalesInvoice[];

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
    this.display = this.displayService.primary(this.m.RepeatingSalesInvoice);
  }

  onPreSharedPull(pulls: Pull[], prefix?: string) {
    const { m } = this;
    const { pullBuilder: p } = m;

    if (this.panelEnabled) {
      const id = this.scoped.id;

      pulls.push(
        p.SalesInvoice({
          objectId: id,
          name: prefix,
          select: {
            RepeatingSalesInvoiceWhereSource: {
              include: {
                Frequency: {},
                DayOfWeek: {},
              },
            },
          },
        })
      );
    }
  }

  onPostSharedPull(pullResult: IPullResult, prefix?: string) {
    this.enabled = this.enabler ? this.enabler() : true;

    this.objects = pullResult.collection<RepeatingSalesInvoice>(prefix) ?? [];
    this.description = `${this.objects.length} ${this.title}`;
  }

  toggle() {
    this.panelService.startEdit(this.panelId).subscribe();
  }
}
