import { Component, OnInit, Self, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { switchMap, filter } from 'rxjs/operators';

import { M } from '@allors/default/workspace/meta';
import {
  Person,
  Organisation,
  OrganisationContactRelationship,
  Party,
  InternalOrganisation,
  ContactMechanism,
  PartyContactMechanism,
  Currency,
  RequestForQuote,
  ProductQuote,
  SalesOrder,
  VatRegime,
  IrpfRegime,
  CustomerRelationship,
} from '@allors/default/workspace/domain';
import {
  PanelService,
  RefreshService,
  SaveService,
  SearchFactory,
} from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';
import { IObject } from '@allors/system/workspace/domain';

import { FetcherService } from '../../../../services/fetcher/fetcher-service';
import { InternalOrganisationId } from '../../../../services/state/internal-organisation-id';
import { Filters } from '../../../../filters/filters';

@Component({
  selector: 'productquote-overview-detail',
  templateUrl: './productquote-overview-detail.component.html',
  providers: [ContextService, PanelService],
})
export class ProductQuoteOverviewDetailComponent implements OnInit, OnDestroy {
  readonly m: M;

  productQuote: ProductQuote;
  salesOrder: SalesOrder;
  request: RequestForQuote;
  currencies: Currency[];
  contactMechanisms: ContactMechanism[];
  contacts: Person[];
  internalOrganisation: InternalOrganisation;

  addContactPerson = false;
  addContactMechanism = false;
  addReceiver = false;

  private previousReceiver: Party;
  private subscription: Subscription;
  vatRegimes: VatRegime[];
  irpfRegimes: IrpfRegime[];

  customersFilter: SearchFactory;
  showIrpf: boolean;

  get receiverIsPerson(): boolean {
    return (
      !this.productQuote.Receiver ||
      this.productQuote.Receiver.strategy.cls === this.m.Person
    );
  }

  constructor(
    @Self() public allors: ContextService,
    @Self() public panel: PanelService,
    public refreshService: RefreshService,
    private saveService: SaveService,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;

    panel.name = 'detail';
    panel.title = 'ProductQuote Details';
    panel.icon = 'business';
    panel.expandable = true;

    // Collapsed
    const productQuotePullName = `${panel.name}_${this.m.ProductQuote.tag}`;
    const salesOrderPullName = `${panel.name}_${this.m.SalesOrder.tag}`;

    panel.onPull = (pulls) => {
      if (this.panel.isCollapsed) {
        const m = this.m;
        const { pullBuilder: pull } = m;
        const x = {};

        pulls.push(
          pull.ProductQuote({
            name: productQuotePullName,
            objectId: this.panel.manager.id,
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
            name: salesOrderPullName,
            objectId: this.panel.manager.id,
            select: {
              SalesOrderWhereQuote: x,
            },
          })
        );
      }
    };

    panel.onPulled = (loaded) => {
      if (this.panel.isCollapsed) {
        this.productQuote = loaded.object<ProductQuote>(this.m.ProductQuote);
        this.salesOrder = loaded.object<SalesOrder>(
          this.m.ProductQuote.SalesOrderWhereQuote
        );
      }
    };
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    // Expanded
    this.subscription = this.panel.manager.on$
      .pipe(
        filter(() => {
          return this.panel.isExpanded;
        }),
        switchMap(() => {
          this.productQuote = undefined;

          const id = this.panel.manager.id;

          const pulls = [
            this.fetcher.internalOrganisation,
            pull.Currency({ sorting: [{ roleType: m.Currency.Name }] }),
            pull.IrpfRegime({ sorting: [{ roleType: m.IrpfRegime.Name }] }),
            pull.ProductQuote({
              objectId: id,
              include: {
                AssignedCurrency: x,
                DerivedCurrency: x,
                Receiver: x,
                FullfillContactMechanism: x,
                QuoteState: x,
                Request: x,
                DerivedVatRegime: x,
                DerivedIrpfRegime: x,
                ContactPerson: x,
              },
            }),
          ];

          this.customersFilter = Filters.customersFilter(
            m,
            this.internalOrganisationId.value
          );

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        this.internalOrganisation =
          this.fetcher.getInternalOrganisation(loaded);
        this.showIrpf = this.internalOrganisation.Country.IsoCode === 'ES';
        this.vatRegimes = this.internalOrganisation.Country.DerivedVatRegimes;
        this.irpfRegimes = loaded.collection<IrpfRegime>(m.IrpfRegime);
        this.productQuote = loaded.object<ProductQuote>(m.ProductQuote);
        this.currencies = loaded.collection<Currency>(m.Currency);

        if (this.productQuote.Receiver) {
          this.previousReceiver = this.productQuote.Receiver;
          this.update(this.productQuote.Receiver);
        }
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.allors.context.push().subscribe(() => {
      this.refreshService.refresh();
      this.panel.toggle();
    }, this.saveService.errorHandler);
  }

  public personAdded(person: Person): void {
    const organisationContactRelationship =
      this.allors.context.create<OrganisationContactRelationship>(
        this.m.OrganisationContactRelationship
      );
    organisationContactRelationship.Organisation = this.productQuote
      .Receiver as Organisation;
    organisationContactRelationship.Contact = person;

    this.contacts.push(person);
    this.productQuote.ContactPerson = person;
  }

  public partyContactMechanismAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.contactMechanisms.push(partyContactMechanism.ContactMechanism);
    this.productQuote.Receiver.addPartyContactMechanism(partyContactMechanism);
    this.productQuote.FullfillContactMechanism =
      partyContactMechanism.ContactMechanism;
  }

  public receiverSelected(party: IObject): void {
    if (party) {
      this.update(party as Party);
    }
  }

  public receiverAdded(party: Party): void {
    const customerRelationship =
      this.allors.context.create<CustomerRelationship>(
        this.m.CustomerRelationship
      );
    customerRelationship.Customer = party;
    customerRelationship.InternalOrganisation = this.internalOrganisation;

    this.request.Originator = party;
  }

  private update(party: Party) {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const pulls = [
      pull.Party({
        object: party,
        select: {
          PartyContactMechanisms: x,
          CurrentPartyContactMechanisms: {
            include: {
              ContactMechanism: {
                PostalAddress_Country: x,
              },
            },
          },
        },
      }),
      pull.Party({
        object: party,
        select: {
          CurrentContacts: x,
        },
      }),
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
      if (this.productQuote.Receiver !== this.previousReceiver) {
        this.productQuote.ContactPerson = null;
        this.productQuote.FullfillContactMechanism = null;

        this.previousReceiver = this.productQuote.Receiver;
      }

      const partyContactMechanisms: PartyContactMechanism[] =
        loaded.collection<PartyContactMechanism>(
          m.Party.CurrentPartyContactMechanisms
        );
      this.contactMechanisms = partyContactMechanisms?.map(
        (v: PartyContactMechanism) => v.ContactMechanism
      );
      this.contacts = loaded.collection<Person>(m.Party.CurrentContacts);
    });
  }
}
