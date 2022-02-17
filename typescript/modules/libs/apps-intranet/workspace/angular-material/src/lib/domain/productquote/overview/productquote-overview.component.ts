import { Component, Self } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import {
  RefreshService,
  SharedPullService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import {
  NavigationService,
  PanelService,
  ScopedService,
  AllorsOverviewPageComponent,
} from '@allors/base/workspace/angular/application';
import { IPullResult, Pull } from '@allors/system/workspace/domain';
import { AllorsMaterialPanelService } from '@allors/base/workspace/angular-material/application';
import { M } from '@allors/default/workspace/meta';
import { ProductQuote, SalesOrder } from '@allors/default/workspace/domain';

@Component({
  templateUrl: './productquote-overview.component.html',
  providers: [
    ScopedService,
    {
      provide: PanelService,
      useClass: AllorsMaterialPanelService,
    },
  ],
})
export class ProductQuoteOverviewComponent extends AllorsOverviewPageComponent {
  m: M;
  productQuote: ProductQuote;
  salesOrder: SalesOrder;

  constructor(
    @Self() scopedService: ScopedService,
    @Self() panelService: PanelService,
    public navigation: NavigationService,
    sharedPullService: SharedPullService,
    refreshService: RefreshService,
    route: ActivatedRoute,
    workspaceService: WorkspaceService
  ) {
    super(
      scopedService,
      panelService,
      sharedPullService,
      refreshService,
      route,
      workspaceService
    );
    this.m = workspaceService.workspace.configuration.metaPopulation as M;
  }

  onPreSharedPull(pulls: Pull[], prefix?: string) {
    const {
      m: { pullBuilder: p },
    } = this;

    const id = this.scoped.id;

    pulls.push(
      p.ProductQuote({
        name: prefix,
        objectId: id,
        include: {
          QuoteItems: {
            Product: {},
            QuoteItemState: {},
          },
          Receiver: {},
          ContactPerson: {},
          QuoteState: {},
          CreatedBy: {},
          LastModifiedBy: {},
          Request: {},
          FullfillContactMechanism: {
            PostalAddress_Country: {},
          },
        },
      }),
      p.ProductQuote({
        name: `${prefix}_salesOrder`,
        objectId: id,
        select: {
          SalesOrderWhereQuote: {},
        },
      })
    );
  }

  onPostSharedPull(loaded: IPullResult, prefix?: string) {
    this.productQuote = loaded.object<ProductQuote>(prefix);
    this.salesOrder = loaded.object<SalesOrder>(`${prefix}_salesOrder`);
  }
}
