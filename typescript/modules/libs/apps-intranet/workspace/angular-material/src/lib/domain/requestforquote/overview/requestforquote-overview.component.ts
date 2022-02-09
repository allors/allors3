import {
  Component,
  Self,
  AfterViewInit,
  OnDestroy,
  Injector,
} from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { M } from '@allors/default/workspace/meta';
import { RequestForQuote, Quote } from '@allors/default/workspace/domain';
import {
  NavigationActivatedRoute,
  NavigationService,
  OldPanelManagerService,
  RefreshService,
} from '@allors/base/workspace/angular/foundation';
import {
  ContextService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './requestforquote-overview.component.html',
  providers: [OldPanelManagerService, ContextService],
})
export class RequestForQuoteOverviewComponent
  implements AfterViewInit, OnDestroy
{
  title = 'Request For Quote';

  public requestForQuote: RequestForQuote;
  public quote: Quote;

  subscription: Subscription;
  m: M;

  constructor(
    @Self() public allors: ContextService,
    @Self() public panelManager: OldPanelManagerService,
    public workspaceService: WorkspaceService,
    public refreshService: RefreshService,
    public navigation: NavigationService,
    private route: ActivatedRoute,
    public injector: Injector,
    private internalOrganisationId: InternalOrganisationId,
    titleService: Title
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;

    titleService.setTitle(this.title);
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest(
      this.route.url,
      this.route.queryParams,
      this.refreshService.refresh$,
      this.internalOrganisationId.observable$
    )
      .pipe(
        switchMap(() => {
          const navRoute = new NavigationActivatedRoute(this.route);
          this.panelManager.id = navRoute.id();
          this.panelManager.objectType = m.RequestForQuote;
          this.panelManager.expanded = navRoute.panel();

          this.panelManager.on();

          const pulls = [
            pull.RequestForQuote({
              objectId: this.panelManager.id,
              include: {
                FullfillContactMechanism: {
                  PostalAddress_Country: x,
                },
                RequestItems: {
                  Product: x,
                },
                Originator: x,
                ContactPerson: x,
                RequestState: x,
                DerivedCurrency: x,
                CreatedBy: x,
                LastModifiedBy: x,
              },
            }),
            pull.RequestForQuote({
              objectId: this.panelManager.id,
              select: {
                QuoteWhereRequest: x,
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

        this.requestForQuote = loaded.object<RequestForQuote>(
          this.m.RequestForQuote
        );
        this.quote = loaded.object<Quote>(
          this.m.RequestForQuote.QuoteWhereRequest
        );
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}