import { Component, Self } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { SerialisedItem } from '@allors/default/workspace/domain';
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
import { IPullResult, Path, Pull } from '@allors/system/workspace/domain';
import { AllorsMaterialPanelService } from '@allors/base/workspace/angular-material/application';
import { M } from '@allors/default/workspace/meta';

@Component({
  templateUrl: './serialiseditem-overview-page.component.html',
  providers: [
    ScopedService,
    {
      provide: PanelService,
      useClass: AllorsMaterialPanelService,
    },
  ],
})
export class SerialisedItemOverviewPageComponent extends AllorsOverviewPageComponent {
  readonly m: M;

  serialisedItem: SerialisedItem;

  workEffortTarget: Path;
  workRequirementTarget: Path;
  salesInvoiceTarget: Path;
  purchaseOrderTarget: Path;
  salesOrderTarget: Path;
  quoteTarget: Path;
  requestTarget: Path;

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
    const { pathBuilder: p } = this.m;

    this.workEffortTarget = p.FixedAsset({
      WorkEffortFixedAssetAssignmentsWhereFixedAsset: {
        Assignment: {},
      },
    });

    this.workRequirementTarget = p.FixedAsset({
      WorkRequirementsWhereFixedAsset: {},
    });

    this.salesInvoiceTarget = p.SerialisedItem({
      SalesInvoiceItemsWhereSerialisedItem: {
        SalesInvoiceWhereSalesInvoiceItem: {},
      },
    });

    this.purchaseOrderTarget = p.SerialisedItem({
      PurchaseOrderItemsWhereSerialisedItem: {
        PurchaseOrderWherePurchaseOrderItem: {},
      },
    });

    this.salesOrderTarget = p.SerialisedItem({
      SalesOrderItemsWhereSerialisedItem: {
        SalesOrderWhereSalesOrderItem: {},
      },
    });

    this.quoteTarget = p.SerialisedItem({
      QuoteItemsWhereSerialisedItem: {
        QuoteWhereQuoteItem: {},
      },
    });

    this.requestTarget = p.SerialisedItem({
      RequestItemsWhereSerialisedItem: {
        RequestWhereRequestItem: {},
      },
    });
  }

  onPreSharedPull(pulls: Pull[], prefix?: string) {
    const {
      m: { pullBuilder: p },
    } = this;

    const id = this.scoped.id;

    pulls.push(
      p.SerialisedItem({
        name: prefix,
        objectId: id,
        include: {
          OwnedBy: {},
        },
      })
    );
  }

  onPostSharedPull(loaded: IPullResult, prefix?: string) {
    this.serialisedItem = loaded.object<SerialisedItem>(prefix);
  }
}
