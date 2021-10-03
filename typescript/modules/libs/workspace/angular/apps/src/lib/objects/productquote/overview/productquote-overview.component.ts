import { Component, Self, AfterViewInit, OnDestroy, Injector } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { Good, ProductQuote, SalesOrder } from '@allors/workspace/domain/default';
import { NavigationActivatedRoute, NavigationService, PanelManagerService, RefreshService, TestScope } from '@allors/workspace/angular/base';
import { ContextService, WorkspaceService } from '@allors/workspace/angular/core';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './productquote-overview.component.html',
  providers: [PanelManagerService, ContextService],
})
export class ProductQuoteOverviewComponent extends TestScope implements AfterViewInit, OnDestroy {
  title = 'Quote';

  public productQuote: ProductQuote;
  public goods: Good[] = [];
  public salesOrder: SalesOrder;

  subscription: Subscription;
  m: M;

  constructor(
    @Self() public panelManager: PanelManagerService,
    public workspaceService: WorkspaceService,
    public refreshService: RefreshService,
    public navigation: NavigationService,
    private route: ActivatedRoute,
    private internalOrganisationId: InternalOrganisationId,
    public injector: Injector,
    titleService: Title
  ) {
    super();

    titleService.setTitle(this.title);

    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;
  }

  public ngAfterViewInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest(this.route.url, this.route.queryParams, this.refreshService.refresh$, this.internalOrganisationId.observable$)
      .pipe(
        switchMap(() => {
          const navRoute = new NavigationActivatedRoute(this.route);
          this.panelManager.id = navRoute.id();
          this.panelManager.objectType = m.ProductQuote;
          this.panelManager.expanded = navRoute.panel();

          this.panelManager.on();

          const pulls = [
            pull.ProductQuote({
              objectId: this.panelManager.id,
              include: {
                QuoteItems: {
                  Product: x,
                  QuoteItemState: x,
                },
                Receiver: x,
                ContactPerson: x,
                QuoteState: x,
                CreatedBy: x,
                LastModifiedBy: x,
                Request: x,
                FullfillContactMechanism: {
                  PostalAddress_Country: x,
                },
              },
            }),
            pull.ProductQuote({
              objectId: this.panelManager.id,
              select: {
                SalesOrderWhereQuote: x,
              },
            }),
          ];

          this.panelManager.onPull(pulls);

          return this.panelManager.context.pull( pulls);
        })
      )
      .subscribe((loaded) => {
        this.panelManager.context.reset();

        this.panelManager.onPulled(loaded);

        this.productQuote = loaded.object<ProductQuote>(m.ProductQuote);
        this.goods = loaded.collection<Good>(m.Good);
        this.salesOrder = loaded.object<SalesOrder>(m.SalesOrder);
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
