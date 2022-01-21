import { Component, OnInit, Self, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { switchMap, filter } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import {
  Person,
  Organisation,
  OrganisationContactRelationship,
  Party,
  InternalOrganisation,
  Good,
  ContactMechanism,
  PartyContactMechanism,
  PostalAddress,
  Currency,
  SalesInvoice,
  VatRegime,
  IrpfRegime,
  VatClause,
  CustomerRelationship,
} from '@allors/workspace/domain/default';
import { PanelService, RefreshService, SaveService, SearchFactory } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';
import { IObject } from '@allors/workspace/domain/system';

import { FetcherService } from '../../../../services/fetcher/fetcher-service';
import { InternalOrganisationId } from '../../../../services/state/internal-organisation-id';
import { Filters } from '../../../../filters/filters';

@Component({
  selector: 'salesinvoice-overview-detail',
  templateUrl: './salesinvoice-overview-detail.component.html',
  providers: [ContextService, PanelService],
})
export class SalesInvoiceOverviewDetailComponent implements OnInit, OnDestroy {
  readonly m: M;

  invoice: SalesInvoice;
  internalOrganisation: InternalOrganisation;
  currencies: Currency[];
  vatRegimes: VatRegime[];
  irpfRegimes: IrpfRegime[];
  vatClauses: VatClause[];
  billToContactMechanisms: ContactMechanism[] = [];
  billToContacts: Person[] = [];
  billToEndCustomerContactMechanisms: ContactMechanism[] = [];
  billToEndCustomerContacts: Person[] = [];
  shipToAddresses: ContactMechanism[] = [];
  shipToContacts: Person[] = [];
  shipToEndCustomerAddresses: ContactMechanism[] = [];
  shipToEndCustomerContacts: Person[] = [];

  addShipToCustomer = false;
  addShipToAddress = false;
  addShipToContactPerson = false;

  addBillToCustomer = false;
  addBillToContactMechanism = false;
  addBillToContactPerson = false;

  addShipToEndCustomer = false;
  addShipToEndCustomerAddress = false;
  addShipToEndCustomerContactPerson = false;

  addBillToEndCustomer = false;
  addBillToEndCustomerContactMechanism = false;
  addBillToEndCustomerContactPerson = false;

  private previousShipToCustomer: Party;
  private previousShipToEndCustomer: Party;
  private previousBillToCustomer: Party;
  private previousBillToEndCustomer: Party;

  private subscription: Subscription;

  customersFilter: SearchFactory;
  showIrpf: boolean;

  get billToCustomerIsPerson(): boolean {
    return !this.invoice.BillToCustomer || this.invoice.BillToCustomer.strategy.cls === this.m.Person;
  }

  get shipToCustomerIsPerson(): boolean {
    return !this.invoice.ShipToCustomer || this.invoice.ShipToCustomer.strategy.cls === this.m.Person;
  }

  get billToEndCustomerIsPerson(): boolean {
    return !this.invoice.BillToEndCustomer || this.invoice.BillToEndCustomer.strategy.cls === this.m.Person;
  }

  get shipToEndCustomerIsPerson(): boolean {
    return !this.invoice.ShipToEndCustomer || this.invoice.ShipToEndCustomer.strategy.cls === this.m.Person;
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
    panel.title = 'Sales Invoice Details';
    panel.icon = 'business';
    panel.expandable = true;

    // Collapsed
    const salesInvoicePullName = `${panel.name}_${this.m.SalesInvoice.tag}`;
    const goodPullName = `${panel.name}_${this.m.Good.tag}`;

    panel.onPull = (pulls) => {
      if (this.panel.isCollapsed) {
        const m = this.m;
        const { pullBuilder: pull } = m;
        const x = {};

        pulls.push(
          pull.SalesInvoice({
            name: salesInvoicePullName,
            objectId: this.panel.manager.id,
            include: {
              SalesInvoiceItems: {
                Product: x,
                InvoiceItemType: x,
              },
              SalesTerms: {
                TermType: x,
              },
              DerivedVatClause: x,
              DerivedCurrency: x,
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
        );
      }
    };

    panel.onPulled = (loaded) => {
      if (this.panel.isCollapsed) {
        this.invoice = loaded.object<SalesInvoice>(this.m.SalesInvoice);
      }
    };
  }

  public ngOnInit(): void {
    const m = this.m;

    // Expanded
    this.subscription = this.panel.manager.on$
      .pipe(
        filter(() => {
          return this.panel.isExpanded;
        }),
        switchMap(() => {
          this.invoice = undefined;

          const m = this.m;
          const { pullBuilder: pull } = m;
          const x = {};
          const id = this.panel.manager.id;

          const pulls = [
            this.fetcher.internalOrganisation,
            pull.SalesInvoice({
              objectId: id,
              include: {
                BillToCustomer: x,
                DerivedBillToContactMechanism: x,
                BillToContactPerson: x,
                ShipToCustomer: x,
                DerivedShipToAddress: x,
                ShipToContactPerson: x,
                BillToEndCustomer: x,
                DerivedBillToEndCustomerContactMechanism: x,
                BillToEndCustomerContactPerson: x,
                ShipToEndCustomer: x,
                DerivedShipToEndCustomerAddress: x,
                ShipToEndCustomerContactPerson: x,
                SalesInvoiceState: x,
                DerivedCurrency: x,
                DerivedVatClause: x,
              },
            }),
            pull.IrpfRegime({ sorting: [{ roleType: m.IrpfRegime.Name }] }),
            pull.VatClause({ sorting: [{ roleType: m.VatClause.Name }] }),
            pull.Currency({
              predicate: { kind: 'Equals', propertyType: m.Currency.IsActive, value: true },
              sorting: [{ roleType: m.Currency.IsoCode }],
            }),
            pull.Organisation({
              predicate: { kind: 'Equals', propertyType: m.Organisation.IsInternalOrganisation, value: true },
              sorting: [{ roleType: m.Organisation.DisplayName }],
            }),
          ];

          this.customersFilter = Filters.customersFilter(m, this.internalOrganisationId.value);

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        this.internalOrganisation = this.fetcher.getInternalOrganisation(loaded);
        this.showIrpf = this.internalOrganisation.Country.IsoCode === 'ES';
        this.vatRegimes = this.internalOrganisation.Country.DerivedVatRegimes;
        this.irpfRegimes = loaded.collection<IrpfRegime>(m.IrpfRegime);
        this.vatClauses = loaded.collection<VatClause>(m.VatClause);
        this.currencies = loaded.collection<Currency>(m.Currency);

        this.invoice = loaded.object<SalesInvoice>(m.SalesInvoice);

        if (this.invoice.BillToCustomer) {
          this.previousBillToCustomer = this.invoice.BillToCustomer;
          this.updateBillToCustomer(this.invoice.BillToCustomer);
        }

        if (this.invoice.BillToEndCustomer) {
          this.previousBillToEndCustomer = this.invoice.BillToEndCustomer;
          this.updateBillToEndCustomer(this.invoice.BillToEndCustomer);
        }

        if (this.invoice.ShipToCustomer) {
          this.previousShipToCustomer = this.invoice.ShipToCustomer;
          this.updateShipToCustomer(this.invoice.ShipToCustomer);
        }

        if (this.invoice.ShipToEndCustomer) {
          this.previousShipToEndCustomer = this.invoice.ShipToEndCustomer;
          this.updateShipToEndCustomer(this.invoice.ShipToEndCustomer);
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

  public shipToCustomerAdded(party: Party): void {
    const customerRelationship = this.allors.context.create<CustomerRelationship>(this.m.CustomerRelationship);
    customerRelationship.Customer = party;
    customerRelationship.InternalOrganisation = this.internalOrganisation;

    this.invoice.ShipToCustomer = party;
  }

  public billToCustomerAdded(party: Party): void {
    const customerRelationship = this.allors.context.create<CustomerRelationship>(this.m.CustomerRelationship);
    customerRelationship.Customer = party;
    customerRelationship.InternalOrganisation = this.internalOrganisation;

    this.invoice.BillToCustomer = party;
  }

  public shipToEndCustomerAdded(party: Party): void {
    const customerRelationship = this.allors.context.create<CustomerRelationship>(this.m.CustomerRelationship);
    customerRelationship.Customer = party;
    customerRelationship.InternalOrganisation = this.internalOrganisation;

    this.invoice.ShipToEndCustomer = party;
  }

  public billToEndCustomerAdded(party: Party): void {
    const customerRelationship = this.allors.context.create<CustomerRelationship>(this.m.CustomerRelationship);
    customerRelationship.Customer = party;
    customerRelationship.InternalOrganisation = this.internalOrganisation;

    this.invoice.BillToEndCustomer = party;
  }

  public billToContactPersonAdded(person: Person): void {
    const organisationContactRelationship = this.allors.context.create<OrganisationContactRelationship>(this.m.OrganisationContactRelationship);
    organisationContactRelationship.Organisation = this.invoice.BillToCustomer as Organisation;
    organisationContactRelationship.Contact = person;

    this.billToContacts.push(person);
    this.invoice.BillToContactPerson = person;
  }

  public billToEndCustomerContactPersonAdded(person: Person): void {
    const organisationContactRelationship = this.allors.context.create<OrganisationContactRelationship>(this.m.OrganisationContactRelationship);
    organisationContactRelationship.Organisation = this.invoice.BillToEndCustomer as Organisation;
    organisationContactRelationship.Contact = person;

    this.billToEndCustomerContacts.push(person);
    this.invoice.BillToEndCustomerContactPerson = person;
  }

  public shipToContactPersonAdded(person: Person): void {
    const organisationContactRelationship = this.allors.context.create<OrganisationContactRelationship>(this.m.OrganisationContactRelationship);
    organisationContactRelationship.Organisation = this.invoice.ShipToCustomer as Organisation;
    organisationContactRelationship.Contact = person;

    this.shipToContacts.push(person);
    this.invoice.ShipToContactPerson = person;
  }

  public shipToEndCustomerContactPersonAdded(person: Person): void {
    const organisationContactRelationship = this.allors.context.create<OrganisationContactRelationship>(this.m.OrganisationContactRelationship);
    organisationContactRelationship.Organisation = this.invoice.ShipToEndCustomer as Organisation;
    organisationContactRelationship.Contact = person;

    this.shipToEndCustomerContacts.push(person);
    this.invoice.ShipToEndCustomerContactPerson = person;
  }

  public billToContactMechanismAdded(partyContactMechanism: PartyContactMechanism): void {
    this.billToContactMechanisms.push(partyContactMechanism.ContactMechanism);
    this.invoice.BillToCustomer.addPartyContactMechanism(partyContactMechanism);
    this.invoice.AssignedBillToContactMechanism = partyContactMechanism.ContactMechanism;
  }

  public billToEndCustomerContactMechanismAdded(partyContactMechanism: PartyContactMechanism): void {
    this.billToEndCustomerContactMechanisms.push(partyContactMechanism.ContactMechanism);
    this.invoice.BillToEndCustomer.addPartyContactMechanism(partyContactMechanism);
    this.invoice.AssignedBillToEndCustomerContactMechanism = partyContactMechanism.ContactMechanism;
  }

  public shipToAddressAdded(partyContactMechanism: PartyContactMechanism): void {
    this.shipToAddresses.push(partyContactMechanism.ContactMechanism);
    this.invoice.ShipToCustomer.addPartyContactMechanism(partyContactMechanism);
    this.invoice.AssignedShipToAddress = partyContactMechanism.ContactMechanism as PostalAddress;
  }

  public shipToEndCustomerAddressAdded(partyContactMechanism: PartyContactMechanism): void {
    this.shipToEndCustomerAddresses.push(partyContactMechanism.ContactMechanism);
    this.invoice.ShipToEndCustomer.addPartyContactMechanism(partyContactMechanism);
    this.invoice.AssignedShipToEndCustomerAddress = partyContactMechanism.ContactMechanism as PostalAddress;
  }

  public billToCustomerSelected(party: IObject) {
    if (party) {
      this.updateBillToCustomer(party as Party);
    }
  }

  public billToEndCustomerSelected(party: IObject) {
    if (party) {
      this.updateBillToEndCustomer(party as Party);
    }
  }

  public shipToCustomerSelected(party: IObject) {
    if (party) {
      this.updateShipToCustomer(party as Party);
    }
  }

  public shipToEndCustomerSelected(party: IObject) {
    if (party) {
      this.updateShipToEndCustomer(party as Party);
    }
  }

  private updateBillToCustomer(party: Party) {
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
      pull.Party({
        object: party,
        include: {
          PreferredCurrency: x,
        },
      }),
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
      if (this.invoice.BillToCustomer !== this.previousBillToCustomer) {
        this.invoice.AssignedBillToContactMechanism = null;
        this.invoice.BillToContactPerson = null;
        this.previousBillToCustomer = this.invoice.BillToCustomer;
      }

      if (this.invoice.BillToCustomer != null && this.invoice.ShipToCustomer == null) {
        this.invoice.ShipToCustomer = this.invoice.BillToCustomer;
        this.updateShipToCustomer(this.invoice.ShipToCustomer);
      }

      const partyContactMechanisms: PartyContactMechanism[] = loaded.collection<PartyContactMechanism>(m.Party.CurrentPartyContactMechanisms);
      this.billToContactMechanisms = partyContactMechanisms?.map((v: PartyContactMechanism) => v.ContactMechanism);
      this.billToContacts = loaded.collection<Person>(m.Party.CurrentContacts);
    });
  }

  private updateShipToCustomer(party: Party): void {
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
      pull.Party({
        object: party,
        include: {
          PreferredCurrency: x,
        },
      }),
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
      if (this.invoice.ShipToCustomer !== this.previousShipToCustomer) {
        this.invoice.AssignedShipToAddress = null;
        this.invoice.ShipToContactPerson = null;
        this.previousShipToCustomer = this.invoice.ShipToCustomer;
      }

      if (this.invoice.ShipToCustomer != null && this.invoice.BillToCustomer == null) {
        this.invoice.BillToCustomer = this.invoice.ShipToCustomer;
        this.updateBillToCustomer(this.invoice.ShipToCustomer);
      }

      const partyContactMechanisms: PartyContactMechanism[] = loaded.collection<PartyContactMechanism>(m.Party.CurrentPartyContactMechanisms);
      this.shipToAddresses = partyContactMechanisms?.filter((v: PartyContactMechanism) => v.ContactMechanism.strategy.cls === m.PostalAddress)?.map((v: PartyContactMechanism) => v.ContactMechanism);
      this.shipToContacts = loaded.collection<Person>(m.Party.CurrentContacts);
    });
  }

  private updateBillToEndCustomer(party: Party) {
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
      if (this.invoice.BillToEndCustomer !== this.previousBillToEndCustomer) {
        this.invoice.AssignedBillToEndCustomerContactMechanism = null;
        this.invoice.BillToEndCustomerContactPerson = null;
        this.previousBillToEndCustomer = this.invoice.BillToEndCustomer;
      }

      if (this.invoice.BillToEndCustomer != null && this.invoice.ShipToEndCustomer == null) {
        this.invoice.ShipToEndCustomer = this.invoice.BillToEndCustomer;
        this.updateShipToEndCustomer(this.invoice.ShipToEndCustomer);
      }

      const partyContactMechanisms: PartyContactMechanism[] = loaded.collection<PartyContactMechanism>(m.Party.CurrentPartyContactMechanisms);
      this.billToEndCustomerContactMechanisms = partyContactMechanisms?.map((v: PartyContactMechanism) => v.ContactMechanism);
      this.billToEndCustomerContacts = loaded.collection<Person>(m.Party.CurrentContacts);
    });
  }

  private updateShipToEndCustomer(party: Party) {
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
      if (this.invoice.ShipToEndCustomer !== this.previousShipToEndCustomer) {
        this.invoice.AssignedShipToEndCustomerAddress = null;
        this.invoice.ShipToEndCustomerContactPerson = null;
        this.previousShipToEndCustomer = this.invoice.ShipToEndCustomer;
      }

      if (this.invoice.ShipToEndCustomer != null && this.invoice.BillToEndCustomer == null) {
        this.invoice.BillToEndCustomer = this.invoice.ShipToEndCustomer;
        this.updateBillToEndCustomer(this.invoice.BillToEndCustomer);
      }

      const partyContactMechanisms: PartyContactMechanism[] = loaded.collection<PartyContactMechanism>(m.Party.CurrentPartyContactMechanisms);
      this.shipToEndCustomerAddresses = partyContactMechanisms?.filter((v: PartyContactMechanism) => v.ContactMechanism.strategy.cls === m.PostalAddress)?.map((v: PartyContactMechanism) => v.ContactMechanism);
      this.shipToEndCustomerContacts = loaded.collection<Person>(m.Party.CurrentContacts);
    });
  }
}
