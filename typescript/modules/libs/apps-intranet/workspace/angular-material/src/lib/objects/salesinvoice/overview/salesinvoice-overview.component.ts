import {
  Component,
  Self,
  AfterViewInit,
  OnDestroy,
  Injector,
} from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { ActivatedRoute } from '@angular/router';

import { M } from '@allors/default/workspace/meta';
import {
  SalesInvoice,
  RepeatingSalesInvoice,
} from '@allors/default/workspace/domain';
import {
  NavigationActivatedRoute,
  NavigationService,
  PanelManagerService,
  RefreshService,
} from '@allors/workspace/angular/base';
import {
  ContextService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './salesinvoice-overview.component.html',
  providers: [PanelManagerService, ContextService],
})
export class SalesInvoiceOverviewComponent implements AfterViewInit, OnDestroy {
  title = 'Sales Invoice';

  public invoice: SalesInvoice;
  public repeatingInvoices: RepeatingSalesInvoice[];
  public repeatingInvoice: RepeatingSalesInvoice;

  subscription: Subscription;
  m: M;

  constructor(
    @Self() public allors: ContextService,
    @Self() public panelManager: PanelManagerService,
    public workspaceService: WorkspaceService,
    public refreshService: RefreshService,
    public navigation: NavigationService,
    private route: ActivatedRoute,
    public injector: Injector,
    private internalOrganisationId: InternalOrganisationId,
    titleService: Title
  ) {
    this.allors.context.name = this.constructor.name;
    titleService.setTitle(this.title);

    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;
  }

  public ngAfterViewInit(): void {
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
          this.panelManager.objectType = m.SalesInvoice;
          this.panelManager.expanded = navRoute.panel();

          const { id } = this.panelManager;

          this.panelManager.on();

          const pulls = [
            pull.SalesInvoice({
              objectId: id,
              include: {
                SalesInvoiceItems: {
                  Product: x,
                  InvoiceItemType: x,
                },
                SalesTerms: {
                  TermType: x,
                },
                BillToCustomer: x,
                BillToContactPerson: x,
                ShipToCustomer: x,
                ShipToContactPerson: x,
                ShipToEndCustomer: x,
                ShipToEndCustomerContactPerson: x,
                SalesInvoiceState: x,
                CreatedBy: x,
                LastModifiedBy: x,
                DerivedBillToContactMechanism: {
                  PostalAddress_Country: x,
                },
                DerivedShipToAddress: {
                  Country: x,
                },
                DerivedBillToEndCustomerContactMechanism: {
                  PostalAddress_Country: x,
                },
                DerivedShipToEndCustomerAddress: {
                  Country: x,
                },
              },
            }),
            pull.RepeatingSalesInvoice({
              predicate: {
                kind: 'Equals',
                propertyType: m.RepeatingSalesInvoice.Source,
                value: id,
              },
              include: {
                Frequency: x,
                DayOfWeek: x,
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

        this.invoice = loaded.object<SalesInvoice>(m.SalesInvoice);
        this.repeatingInvoices = loaded.collection<RepeatingSalesInvoice>(
          m.RepeatingSalesInvoice
        );
        if (this.repeatingInvoices?.length > 0) {
          this.repeatingInvoice = this.repeatingInvoices[0];
        } else {
          this.repeatingInvoice = undefined;
        }
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
