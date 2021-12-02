import { Component, Self, AfterViewInit, OnDestroy, Injector } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { Organisation, SupplierOffering } from '@allors/workspace/domain/default';
import { NavigationService, RefreshService, PanelManagerService, NavigationActivatedRoute } from '@allors/workspace/angular/base';
import { ContextService, WorkspaceService } from '@allors/workspace/angular/core';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './organisation-overview.component.html',
  providers: [PanelManagerService, ContextService],
})
export class OrganisationOverviewComponent implements AfterViewInit, OnDestroy {
  title = 'Organisation';

  organisation: Organisation;

  subscription: Subscription;
  supplierOfferings: SupplierOffering[];
  m: M;

  constructor(
    @Self() public allors: ContextService,
    @Self() public panelManager: PanelManagerService,
    public workspaceService: WorkspaceService,
    public refreshService: RefreshService,
    public navigation: NavigationService,
    private route: ActivatedRoute,
    private internalOrganisationId: InternalOrganisationId,
    public injector: Injector,
    titleService: Title
  ) {
    titleService.setTitle(this.title);

    this.allors.context.name = this.constructor.name;
    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;
  }

  public ngAfterViewInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest(this.route.url, this.route.queryParams, this.refreshService.refresh$, this.internalOrganisationId.observable$)
      .pipe(
        switchMap(([, ,]) => {
          const navRoute = new NavigationActivatedRoute(this.route);
          this.panelManager.objectType = m.Organisation;
          this.panelManager.id = navRoute.id();
          this.panelManager.expanded = navRoute.panel();

          this.panelManager.on();

          const pulls = [
            pull.Organisation({
              objectId: this.panelManager.id,
            }),
            pull.Organisation({
              objectId: this.panelManager.id,
              select: {
                SupplierOfferingsWhereSupplier: x,
              },
            }),
          ];

          this.panelManager.onPull(pulls);

          return this.panelManager.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.panelManager.context.reset();
        this.panelManager.onPulled(loaded);

        this.organisation = loaded.object<Organisation>(m.Organisation);
        this.supplierOfferings = loaded.collection<SupplierOffering>(m.Organisation.SupplierOfferingsWhereSupplier);
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
