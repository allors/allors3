import { Component, Self } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Good, UnifiedGood } from '@allors/default/workspace/domain';
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
  templateUrl: './unifiedgood-overview-page.component.html',
  providers: [
    ScopedService,
    {
      provide: PanelService,
      useClass: AllorsMaterialPanelService,
    },
  ],
})
export class UnifiedGoodOverviewPageComponent extends AllorsOverviewPageComponent {
  m: M;

  good: UnifiedGood;

  nonSerialisedInventoryItemTarget: Path;
  serialisedInventoryItemTarget: Path;

  serialised: () => boolean;
  nonSerialised: () => boolean;

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
    const { m } = this;
    const { pathBuilder: p } = this.m;

    this.nonSerialisedInventoryItemTarget = p.NonUnifiedPart({
      InventoryItemsWherePart: {},
      ofType: m.NonSerialisedInventoryItem,
    });

    this.serialisedInventoryItemTarget = p.NonUnifiedPart({
      InventoryItemsWherePart: {},
      ofType: m.SerialisedInventoryItem,
    });

    this.serialised = () =>
      this.good.InventoryItemKind.UniqueId ===
      '2596e2dd-3f5d-4588-a4a2-167d6fbe3fae';

    this.nonSerialised = () => !this.serialised();
  }

  onPreSharedPull(pulls: Pull[], prefix?: string) {
    const {
      m: { pullBuilder: p },
    } = this;

    const id = this.scoped.id;

    pulls.push(
      p.UnifiedGood({
        name: prefix,
        objectId: id,
        include: {
          InventoryItemKind: {},
        },
      })
    );
  }

  onPostSharedPull(loaded: IPullResult, prefix?: string) {
    this.good = loaded.object<UnifiedGood>(prefix);
  }
}
