import { Component, Self, AfterViewInit, OnDestroy, Injector } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { MetaService, RefreshService,  NavigationService, PanelManagerService, SessionService } from '@allors/angular/services/core';
import { Good, ProductQuote, SalesOrder } from '@allors/domain/generated';
import { ActivatedRoute } from '@angular/router';
import { InternalOrganisationId } from '@allors/angular/base';
import { PullRequest } from '@allors/protocol/system';
import { NavigationActivatedRoute, TestScope } from '@allors/angular/core';

@Component({
  templateUrl: './productquote-overview.component.html',
  providers: [PanelManagerService, SessionService]
})
export class ProductQuoteOverviewComponent extends TestScope implements AfterViewInit, OnDestroy {

  title = 'Quote';

  public productQuote: ProductQuote;
  public goods: Good[] = [];
  public salesOrder: SalesOrder;

  subscription: Subscription;

  constructor(
    @Self() public panelManager: PanelManagerService,
    
    public refreshService: RefreshService,
    public navigation: NavigationService,
    private route: ActivatedRoute,
    private internalOrganisationId: InternalOrganisationId,
    public injector: Injector,
    titleService: Title,
  ) {
    super();

    titleService.setTitle(this.title);
  }

  public ngAfterViewInit(): void {

    this.subscription = combineLatest(this.route.url, this.route.queryParams, this.refreshService.refresh$, this.internalOrganisationId.observable$)
      .pipe(
        switchMap(() => {

          const { m, pull, x } = this.metaService;

          const navRoute = new NavigationActivatedRoute(this.route);
          this.panelManager.id = navRoute.id();
          this.panelManager.objectType = m.ProductQuote;
          this.panelManager.expanded = navRoute.panel();

          this.panelManager.on();

          const pulls = [
            pull.ProductQuote(
              {
                object: this.panelManager.id,
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
                    PostalAddress_Country: x
                  }
                }
              }),
            pull.ProductQuote(
              {
                object: this.panelManager.id,
                select: {
                  SalesOrderWhereQuote: x,
                }
              }
            )
          ];

          this.panelManager.onPull(pulls);

          return this.panelManager.context
            .load(new PullRequest({ pulls }));
        })
      )
      .subscribe((loaded) => {

        this.panelManager.context.session.reset();

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
