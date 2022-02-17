import { combineLatest, delay, map, switchMap } from 'rxjs';
import { Component, Self } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CustomerShipment } from '@allors/default/workspace/domain';
import {
  RefreshService,
  SharedPullService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import {
  NavigationService,
  NavigationActivatedRoute,
  PanelService,
  ScopedService,
  AllorsOverviewPageComponent,
} from '@allors/base/workspace/angular/application';
import { IPullResult, Pull } from '@allors/system/workspace/domain';
import { AllorsMaterialPanelService } from '@allors/base/workspace/angular-material/application';
import { M } from '@allors/default/workspace/meta';

@Component({
  templateUrl: './customershipment-overview.component.html',
  providers: [
    ScopedService,
    {
      provide: PanelService,
      useClass: AllorsMaterialPanelService,
    },
  ],
})
export class CustomerShipmentOverviewComponent extends AllorsOverviewPageComponent {
  m: M;
  shipment: CustomerShipment;

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
      p.Shipment({
        name: prefix,
        objectId: id,
        include: {
          ShipmentItems: {
            Good: {},
          },
          ShipFromParty: {},
          ShipFromAddress: {},
          ShipToParty: {},
          ShipToContactPerson: {},
          ShipmentState: {},
          CreatedBy: {},
          LastModifiedBy: {},
          ShipToAddress: {
            Country: {},
          },
        },
      })
    );
  }

  onPostSharedPull(loaded: IPullResult, prefix?: string) {
    this.shipment = loaded.object<CustomerShipment>(prefix);
  }
}
