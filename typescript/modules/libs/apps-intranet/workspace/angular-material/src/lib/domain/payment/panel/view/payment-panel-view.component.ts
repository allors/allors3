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
import { Invoice, Payment } from '@allors/default/workspace/domain';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'payment-panel-view',
  templateUrl: './payment-panel-view.component.html',
})
export class PaymentPanelViewComponent
  extends AllorsCustomViewExtentPanelComponent
  implements SharedPullHandler, OnInit
{
  override readonly panelKind = 'Extent';

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
    return null;
    // TODO: Martien
    // return { propertyType: this.init, id: this.scoped.id };
  }

  title = 'Payments';

  m: M;

  objects: Payment[];

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
    this.display = this.displayService.primary(this.m.Payment);
  }

  onPreSharedPull(pulls: Pull[], prefix?: string) {
    const { m } = this;
    const { pullBuilder: p } = m;

    if (this.panelEnabled) {
      const id = this.scoped.id;

      pulls.push(
        p.PaymentApplication({
          name: `${prefix} invoice`,
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
          name: `${prefix} order`,
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
        })
      );
    }
  }

  onPostSharedPull(pullResult: IPullResult, prefix?: string) {
    this.enabled = this.enabler ? this.enabler() : true;

    this.objects =
      pullResult.collection<Payment>(`${prefix} invoice`) ??
      pullResult.collection<Payment>(`${prefix} order`) ??
      [];

    this.description = `${this.objects.length} payments`;
  }

  toggle() {
    this.panelService.startEdit(this.panelId).subscribe();
  }
}
