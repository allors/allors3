import { Component, OnInit, Self, OnDestroy } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Subscription } from 'rxjs';
import { switchMap, filter } from 'rxjs/operators';

import { M } from '@allors/default/workspace/meta';
import {
  Person,
  Organisation,
  OrganisationContactRelationship,
  Party,
  Facility,
  InternalOrganisation,
  ContactMechanism,
  PartyContactMechanism,
  PostalAddress,
  SerialisedInventoryItemState,
  Currency,
  SalesOrderItem,
  ProductQuote,
  SalesOrder,
  SalesInvoice,
  VatRegime,
  IrpfRegime,
  VatClause,
  Store,
  BillingProcess,
  CustomerRelationship,
} from '@allors/default/workspace/domain';
import {
  OldPanelService,
  RefreshService,
  ErrorService,
  SearchFactory,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { IObject } from '@allors/system/workspace/domain';

import { FetcherService } from '../../../../services/fetcher/fetcher-service';
import { InternalOrganisationId } from '../../../../services/state/internal-organisation-id';
import { Filters } from '../../../../filters/filters';

@Component({
  selector: 'salesorder-overview-detail',
  templateUrl: './salesorder-overview-detail.component.html',
  providers: [ContextService, OldPanelService],
})
export class SalesOrderOverviewDetailComponent implements OnInit, OnDestroy {
  readonly m: M;

  order: SalesOrder;
  quote: ProductQuote;
  internalOrganisations: InternalOrganisation[];
  currencies: Currency[];
  billToContactMechanisms: ContactMechanism[] = [];
  billToEndCustomerContactMechanisms: ContactMechanism[] = [];
  shipFromAddresses: ContactMechanism[] = [];
  shipToAddresses: ContactMechanism[] = [];
  shipToEndCustomerAddresses: ContactMechanism[] = [];
  vatRegimes: VatRegime[];
  irpfRegimes: IrpfRegime[];
  vatClauses: VatClause[];
  billToContacts: Person[] = [];
  billToEndCustomerContacts: Person[] = [];
  shipToContacts: Person[] = [];
  shipToEndCustomerContacts: Person[] = [];
  stores: Store[];
  orderItems: SalesOrderItem[] = [];
  salesInvoice: SalesInvoice;
  billingProcesses: BillingProcess[];
  billingForOrderItems: BillingProcess;
  selectedSerialisedInventoryState: string;
  inventoryItemStates: SerialisedInventoryItemState[];
  internalOrganisation: InternalOrganisation;

  addShipFromAddress = false;

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
  facilities: Facility[];

  customersFilter: SearchFactory;
  showIrpf: boolean;

  get billToCustomerIsPerson(): boolean {
    return (
      !this.order.BillToCustomer ||
      this.order.BillToCustomer.strategy.cls === this.m.Person
    );
  }

  get shipToCustomerIsPerson(): boolean {
    return (
      !this.order.ShipToCustomer ||
      this.order.ShipToCustomer.strategy.cls === this.m.Person
    );
  }

  get billToEndCustomerIsPerson(): boolean {
    return (
      !this.order.BillToEndCustomer ||
      this.order.BillToEndCustomer.strategy.cls === this.m.Person
    );
  }

  get shipToEndCustomerIsPerson(): boolean {
    return (
      !this.order.ShipToEndCustomer ||
      this.order.ShipToEndCustomer.strategy.cls === this.m.Person
    );
  }

  constructor(
    @Self() public allors: ContextService,
    @Self() public panel: OldPanelService,
    public refreshService: RefreshService,
    private errorService: ErrorService,
    private fetcher: FetcherService,
    private snackBar: MatSnackBar,
    private internalOrganisationId: InternalOrganisationId
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;

    panel.name = 'detail';
    panel.title = 'Sales Order Details';
    panel.icon = 'business';
    panel.expandable = true;

    // Collapsed
    const salesOrderPullName = `${panel.name}_${this.m.SalesOrder.tag}`;
    const salesInvoicePullName = `${panel.name}_${this.m.SalesInvoice.tag}`;
    const goodPullName = `${panel.name}_${this.m.Good.tag}`;
    const billingProcessPullName = `${panel.name}_${this.m.BillingProcess.tag}`;
    const serialisedInventoryItemStatePullName = `${panel.name}_${this.m.SerialisedInventoryItemState.tag}`;

    panel.onPull = (pulls) => {
      if (this.panel.isCollapsed) {
        const m = this.m;
        const { pullBuilder: pull } = m;
        const x = {};

        pulls.push(
          pull.SalesOrder({
            name: salesOrderPullName,
            objectId: this.panel.manager.id,
            include: {
              SalesOrderItems: {
                Product: x,
                InvoiceItemType: x,
                SalesOrderItemState: x,
                SalesOrderItemShipmentState: x,
                SalesOrderItemPaymentState: x,
                SalesOrderItemInvoiceState: x,
              },
              SalesTerms: {
                TermType: x,
              },
              DerivedCurrency: x,
              BillToCustomer: x,
              BillToContactPerson: x,
              ShipToCustomer: x,
              ShipToContactPerson: x,
              ShipToEndCustomer: x,
              ShipToEndCustomerContactPerson: x,
              BillToEndCustomer: x,
              BillToEndCustomerContactPerson: x,
              SalesOrderState: x,
              SalesOrderShipmentState: x,
              SalesOrderInvoiceState: x,
              SalesOrderPaymentState: x,
              CreatedBy: x,
              LastModifiedBy: x,
              Quote: x,
              DerivedShipFromAddress: {
                Country: x,
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
              DerivedBillToContactMechanism: {
                PostalAddress_Country: x,
              },
            },
          }),
          pull.SalesOrder({
            name: salesInvoicePullName,
            objectId: this.panel.manager.id,
            select: { SalesInvoicesWhereSalesOrder: x },
          }),
          pull.BillingProcess({
            name: billingProcessPullName,
            sorting: [{ roleType: m.BillingProcess.Name }],
          }),
          pull.Currency({
            predicate: {
              kind: 'Equals',
              propertyType: m.Currency.IsActive,
              value: true,
            },
            sorting: [{ roleType: m.Currency.IsoCode }],
          }),
          pull.SerialisedInventoryItemState({
            name: serialisedInventoryItemStatePullName,
            predicate: {
              kind: 'Equals',
              propertyType: m.SerialisedInventoryItemState.IsActive,
              value: true,
            },
            sorting: [{ roleType: m.SerialisedInventoryItemState.Name }],
          })
        );
      }
    };

    panel.onPulled = (loaded) => {
      if (this.panel.isCollapsed) {
        this.order = loaded.object<SalesOrder>(salesOrderPullName);
        this.orderItems = loaded.collection<SalesOrderItem>(salesOrderPullName);
        this.salesInvoice = loaded.object<SalesInvoice>(salesInvoicePullName);
        this.currencies = loaded.collection<Currency>(this.m.Currency);
        this.billingProcesses = loaded.collection<BillingProcess>(
          billingProcessPullName
        );
        this.billingForOrderItems = this.billingProcesses?.find(
          (v: BillingProcess) =>
            v.UniqueId === 'ab01ccc2-6480-4fc0-b20e-265afd41fae2'
        );
        this.inventoryItemStates =
          loaded.collection<SerialisedInventoryItemState>(
            serialisedInventoryItemStatePullName
          );
      }
    };
  }

  public ngOnInit(): void {
    // Expanded
    this.subscription = this.panel.manager.on$
      .pipe(
        filter(() => {
          return this.panel.isExpanded;
        }),
        switchMap(() => {
          this.order = undefined;

          const m = this.m;
          const { pullBuilder: pull } = m;
          const x = {};
          const id = this.panel.manager.id;

          const pulls = [
            this.fetcher.internalOrganisation,
            this.fetcher.warehouses,
            pull.SalesOrder({
              objectId: id,
              include: {
                DerivedCurrency: x,
                Store: x,
                OriginFacility: x,
                ShipToCustomer: x,
                DerivedShipToAddress: x,
                ShipToContactPerson: x,
                SalesOrderState: x,
                DerivedBillToContactMechanism: x,
                BillToContactPerson: x,
                DerivedBillToEndCustomerContactMechanism: x,
                BillToEndCustomerContactPerson: x,
                ShipToEndCustomer: x,
                DerivedShipToEndCustomerAddress: x,
                ShipToEndCustomerContactPerson: x,
                DerivedVatClause: x,
                DerivedVatRegime: x,
                DerivedIrpfRegime: x,
                Quote: x,
              },
            }),
            pull.VatClause({ sorting: [{ roleType: m.VatClause.Name }] }),
            pull.Currency({ sorting: [{ roleType: m.Currency.Name }] }),
            pull.IrpfRegime({ sorting: [{ roleType: m.IrpfRegime.Name }] }),
            pull.Store({
              predicate: {
                kind: 'Equals',
                propertyType: m.Store.InternalOrganisation,
                object: this.internalOrganisation,
              },
              include: { BillingProcess: x },
              sorting: [{ roleType: m.Store.Name }],
            }),
            pull.Party({
              objectId: this.internalOrganisationId.value,
              select: {
                CurrentPartyContactMechanisms: {
                  include: {
                    ContactMechanism: {
                      PostalAddress_Country: x,
                    },
                  },
                },
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

        this.order = loaded.object<SalesOrder>(this.m.SalesOrder);
        this.internalOrganisation =
          this.fetcher.getInternalOrganisation(loaded);
        this.showIrpf = this.internalOrganisation.Country.IsoCode === 'ES';
        this.vatRegimes = this.internalOrganisation.Country.DerivedVatRegimes;
        this.facilities = this.fetcher.getWarehouses(loaded);
        this.irpfRegimes = loaded.collection<IrpfRegime>(this.m.IrpfRegime);
        this.vatClauses = loaded.collection<VatClause>(this.m.VatClause);
        this.stores = loaded.collection<Store>(this.m.Store);
        this.currencies = loaded.collection<Currency>(this.m.Currency);

        const partyContactMechanisms: PartyContactMechanism[] =
          loaded.collection<PartyContactMechanism>(
            this.m.Party.CurrentPartyContactMechanisms
          );
        this.shipFromAddresses = partyContactMechanisms
          ?.filter(
            (v: PartyContactMechanism) =>
              v.ContactMechanism.strategy.cls === this.m.PostalAddress
          )
          ?.map((v: PartyContactMechanism) => v.ContactMechanism);

        if (this.order.ShipToCustomer) {
          this.previousShipToCustomer = this.order.ShipToCustomer;
          this.updateShipToCustomer(this.order.ShipToCustomer);
        }

        if (this.order.BillToCustomer) {
          this.previousBillToCustomer = this.order.BillToCustomer;
          this.updateBillToCustomer(this.order.BillToCustomer);
        }

        if (this.order.BillToEndCustomer) {
          this.previousBillToEndCustomer = this.order.BillToEndCustomer;
          this.updateBillToEndCustomer(this.order.BillToEndCustomer);
        }

        if (this.order.ShipToEndCustomer) {
          this.previousShipToEndCustomer = this.order.ShipToEndCustomer;
          this.updateShipToEndCustomer(this.order.ShipToEndCustomer);
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
    }, this.errorService.errorHandler);
  }

  public shipToCustomerAdded(party: Party): void {
    const customerRelationship =
      this.allors.context.create<CustomerRelationship>(
        this.m.CustomerRelationship
      );
    customerRelationship.Customer = party;
    customerRelationship.InternalOrganisation = this.internalOrganisation;

    this.order.ShipToCustomer = party;
  }

  public billToCustomerAdded(party: Party): void {
    const customerRelationship =
      this.allors.context.create<CustomerRelationship>(
        this.m.CustomerRelationship
      );
    customerRelationship.Customer = party;
    customerRelationship.InternalOrganisation = this.internalOrganisation;

    this.order.BillToCustomer = party;
  }

  public shipToEndCustomerAdded(party: Party): void {
    const customerRelationship =
      this.allors.context.create<CustomerRelationship>(
        this.m.CustomerRelationship
      );
    customerRelationship.Customer = party;
    customerRelationship.InternalOrganisation = this.internalOrganisation;

    this.order.ShipToEndCustomer = party;
  }

  public billToEndCustomerAdded(party: Party): void {
    const customerRelationship =
      this.allors.context.create<CustomerRelationship>(
        this.m.CustomerRelationship
      );
    customerRelationship.Customer = party;
    customerRelationship.InternalOrganisation = this.internalOrganisation;

    this.order.BillToEndCustomer = party;
  }

  public billToContactPersonAdded(person: Person): void {
    const organisationContactRelationship =
      this.allors.context.create<OrganisationContactRelationship>(
        this.m.OrganisationContactRelationship
      );
    organisationContactRelationship.Organisation = this.order
      .BillToCustomer as Organisation;
    organisationContactRelationship.Contact = person;

    this.billToContacts.push(person);
    this.order.BillToContactPerson = person;
  }

  public billToEndCustomerContactPersonAdded(person: Person): void {
    const organisationContactRelationship =
      this.allors.context.create<OrganisationContactRelationship>(
        this.m.OrganisationContactRelationship
      );
    organisationContactRelationship.Organisation = this.order
      .BillToEndCustomer as Organisation;
    organisationContactRelationship.Contact = person;

    this.billToEndCustomerContacts.push(person);
    this.order.BillToEndCustomerContactPerson = person;
  }

  public shipToContactPersonAdded(person: Person): void {
    const organisationContactRelationship =
      this.allors.context.create<OrganisationContactRelationship>(
        this.m.OrganisationContactRelationship
      );
    organisationContactRelationship.Organisation = this.order
      .ShipToCustomer as Organisation;
    organisationContactRelationship.Contact = person;

    this.shipToContacts.push(person);
    this.order.ShipToContactPerson = person;
  }

  public shipToEndCustomerContactPersonAdded(person: Person): void {
    const organisationContactRelationship =
      this.allors.context.create<OrganisationContactRelationship>(
        this.m.OrganisationContactRelationship
      );
    organisationContactRelationship.Organisation = this.order
      .ShipToEndCustomer as Organisation;
    organisationContactRelationship.Contact = person;

    this.shipToEndCustomerContacts.push(person);
    this.order.ShipToEndCustomerContactPerson = person;
  }

  public billToCustomerSelected(party: IObject) {
    this.updateBillToCustomer(party as Party);
  }

  public billToEndCustomerSelected(party: IObject) {
    this.updateBillToEndCustomer(party as Party);
  }

  public shipToEndCustomerSelected(party: IObject) {
    this.updateShipToEndCustomer(party as Party);
  }

  public billToContactMechanismAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.billToContactMechanisms.push(partyContactMechanism.ContactMechanism);
    this.order.BillToCustomer.addPartyContactMechanism(partyContactMechanism);
    this.order.AssignedBillToContactMechanism =
      partyContactMechanism.ContactMechanism;
  }

  public billToEndCustomerContactMechanismAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.billToEndCustomerContactMechanisms.push(
      partyContactMechanism.ContactMechanism
    );
    this.order.BillToEndCustomer.addPartyContactMechanism(
      partyContactMechanism
    );
    this.order.AssignedBillToEndCustomerContactMechanism =
      partyContactMechanism.ContactMechanism;
  }

  public shipToAddressAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.shipToAddresses.push(partyContactMechanism.ContactMechanism);
    this.order.ShipToCustomer.addPartyContactMechanism(partyContactMechanism);
    this.order.AssignedShipToAddress =
      partyContactMechanism.ContactMechanism as PostalAddress;
  }

  public shipToEndCustomerAddressAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.shipToEndCustomerAddresses.push(
      partyContactMechanism.ContactMechanism
    );
    this.order.ShipToEndCustomer.addPartyContactMechanism(
      partyContactMechanism
    );
    this.order.AssignedShipToEndCustomerAddress =
      partyContactMechanism.ContactMechanism as PostalAddress;
  }

  public shipFromAddressAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.shipFromAddresses.push(partyContactMechanism.ContactMechanism);
    this.order.TakenBy.addPartyContactMechanism(partyContactMechanism);
    this.order.AssignedShipFromAddress =
      partyContactMechanism.ContactMechanism as PostalAddress;
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
      if (this.order.ShipToCustomer !== this.previousShipToCustomer) {
        this.order.AssignedShipToAddress = null;
        this.order.ShipToContactPerson = null;
        this.previousShipToCustomer = this.order.ShipToCustomer;
      }

      if (
        this.order.ShipToCustomer != null &&
        this.order.BillToCustomer == null
      ) {
        this.order.BillToCustomer = this.order.ShipToCustomer;
        this.updateBillToCustomer(this.order.ShipToCustomer);
      }

      const partyContactMechanisms: PartyContactMechanism[] =
        loaded.collection<PartyContactMechanism>(
          this.m.Party.CurrentPartyContactMechanisms
        );
      this.shipToAddresses = partyContactMechanisms
        ?.filter(
          (v: PartyContactMechanism) =>
            v.ContactMechanism.strategy.cls === m.PostalAddress
        )
        ?.map((v: PartyContactMechanism) => v.ContactMechanism);
      this.shipToContacts = loaded.collection<Person>(
        this.m.Party.CurrentContacts
      );
    });
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
      if (this.order.BillToCustomer !== this.previousBillToCustomer) {
        this.order.AssignedBillToContactMechanism = null;
        this.order.BillToContactPerson = null;
        this.previousBillToCustomer = this.order.BillToCustomer;
      }

      if (
        this.order.BillToCustomer != null &&
        this.order.ShipToCustomer == null
      ) {
        this.order.ShipToCustomer = this.order.BillToCustomer;
        this.updateShipToCustomer(this.order.ShipToCustomer);
      }

      const partyContactMechanisms: PartyContactMechanism[] =
        loaded.collection<PartyContactMechanism>(
          this.m.Party.CurrentPartyContactMechanisms
        );
      this.billToContactMechanisms = partyContactMechanisms?.map(
        (v: PartyContactMechanism) => v.ContactMechanism
      );
      this.billToContacts = loaded.collection<Person>(
        this.m.Party.CurrentContacts
      );
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
      if (this.order.BillToEndCustomer !== this.previousBillToEndCustomer) {
        this.order.AssignedBillToEndCustomerContactMechanism = null;
        this.order.BillToEndCustomerContactPerson = null;
        this.previousBillToEndCustomer = this.order.BillToEndCustomer;
      }

      if (
        this.order.BillToEndCustomer != null &&
        this.order.ShipToEndCustomer == null
      ) {
        this.order.ShipToEndCustomer = this.order.BillToEndCustomer;
        this.updateShipToEndCustomer(this.order.ShipToEndCustomer);
      }

      const partyContactMechanisms: PartyContactMechanism[] =
        loaded.collection<PartyContactMechanism>(
          this.m.Party.CurrentPartyContactMechanisms
        );
      this.billToEndCustomerContactMechanisms = partyContactMechanisms?.map(
        (v: PartyContactMechanism) => v.ContactMechanism
      );
      this.billToEndCustomerContacts = loaded.collection<Person>(
        this.m.Party.CurrentContacts
      );
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
      if (this.order.ShipToEndCustomer !== this.previousShipToEndCustomer) {
        this.order.AssignedShipToEndCustomerAddress = null;
        this.order.ShipToEndCustomerContactPerson = null;
        this.previousShipToEndCustomer = this.order.ShipToEndCustomer;
      }

      if (
        this.order.ShipToEndCustomer != null &&
        this.order.BillToEndCustomer == null
      ) {
        this.order.BillToEndCustomer = this.order.ShipToEndCustomer;
        this.updateBillToEndCustomer(this.order.BillToEndCustomer);
      }

      const partyContactMechanisms: PartyContactMechanism[] =
        loaded.collection<PartyContactMechanism>(
          this.m.Party.CurrentPartyContactMechanisms
        );
      this.shipToEndCustomerAddresses = partyContactMechanisms
        ?.filter(
          (v: PartyContactMechanism) =>
            v.ContactMechanism.strategy.cls === m.PostalAddress
        )
        ?.map((v: PartyContactMechanism) => v.ContactMechanism);
      this.shipToEndCustomerContacts = loaded.collection<Person>(
        this.m.Party.CurrentContacts
      );
    });
  }

  public shipToCustomerSelected(party: IObject) {
    if (party) {
      this.updateShipToCustomer(party as Party);
    }
  }

  public update(): void {
    this.allors.context.push().subscribe(() => {
      this.snackBar.open('Successfully saved.', 'close', { duration: 5000 });
      this.refreshService.refresh();
    }, this.errorService.errorHandler);
  }
}
