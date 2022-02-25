import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { IObject, IPullResult, Pull } from '@allors/system/workspace/domain';
import {
  BillingProcess,
  ContactMechanism,
  Currency,
  CustomerRelationship,
  Facility,
  InternalOrganisation,
  IrpfRegime,
  Organisation,
  OrganisationContactRelationship,
  Party,
  PartyContactMechanism,
  Person,
  PostalAddress,
  ProductQuote,
  SalesInvoice,
  SalesOrder,
  SalesOrderItem,
  SerialisedInventoryItemState,
  Store,
  VatClause,
  VatRegime,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
  SearchFactory,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { Filters } from '../../../filters/filters';

@Component({
  selector: 'salesorder-edit-form',
  templateUrl: './salesorder-edit-form.component.html',
  providers: [ContextService],
})
export class SalesOrderEditFormComponent extends AllorsFormComponent<SalesOrder> {
  readonly m: M;

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
  facilities: Facility[];

  customersFilter: SearchFactory;
  showIrpf: boolean;

  get billToCustomerIsPerson(): boolean {
    return (
      !this.object.BillToCustomer ||
      this.object.BillToCustomer.strategy.cls === this.m.Person
    );
  }

  get shipToCustomerIsPerson(): boolean {
    return (
      !this.object.ShipToCustomer ||
      this.object.ShipToCustomer.strategy.cls === this.m.Person
    );
  }

  get billToEndCustomerIsPerson(): boolean {
    return (
      !this.object.BillToEndCustomer ||
      this.object.BillToEndCustomer.strategy.cls === this.m.Person
    );
  }

  get shipToEndCustomerIsPerson(): boolean {
    return (
      !this.object.ShipToEndCustomer ||
      this.object.ShipToEndCustomer.strategy.cls === this.m.Person
    );
  }

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;

    this.customersFilter = Filters.customersFilter(
      this.m,
      internalOrganisationId.value
    );
  }
  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      this.fetcher.internalOrganisation,
      this.fetcher.warehouses,
      p.SalesOrder({
        name: '_object',
        objectId: this.editRequest.objectId,
        include: {
          DerivedCurrency: {},
          Store: {},
          OriginFacility: {},
          ShipToCustomer: {},
          ShipToContactPerson: {},
          SalesOrderState: {},
          BillToContactPerson: {},
          BillToEndCustomerContactPerson: {},
          ShipToEndCustomer: {},
          ShipToEndCustomerContactPerson: {},
          DerivedVatClause: {},
          DerivedVatRegime: {},
          DerivedIrpfRegime: {},
          Quote: {},
          SalesOrderItems: {
            Product: {},
            InvoiceItemType: {},
            SalesOrderItemState: {},
            SalesOrderItemShipmentState: {},
            SalesOrderItemPaymentState: {},
            SalesOrderItemInvoiceState: {},
          },
          SalesTerms: {
            TermType: {},
          },
          BillToCustomer: {},
          BillToEndCustomer: {},
          SalesOrderShipmentState: {},
          SalesOrderInvoiceState: {},
          SalesOrderPaymentState: {},
          CreatedBy: {},
          LastModifiedBy: {},
          DerivedShipFromAddress: {
            Country: {},
          },
          DerivedShipToAddress: {
            Country: {},
          },
          DerivedBillToEndCustomerContactMechanism: {
            PostalAddress_Country: {},
          },
          DerivedShipToEndCustomerAddress: {
            Country: {},
          },
          DerivedBillToContactMechanism: {
            PostalAddress_Country: {},
          },
        },
      }),
      p.VatClause({ sorting: [{ roleType: m.VatClause.Name }] }),
      p.IrpfRegime({ sorting: [{ roleType: m.IrpfRegime.Name }] }),
      p.Store({
        predicate: {
          kind: 'Equals',
          propertyType: m.Store.InternalOrganisation,
          object: this.internalOrganisation,
        },
        include: { BillingProcess: {} },
        sorting: [{ roleType: m.Store.Name }],
      }),
      p.Party({
        objectId: this.internalOrganisationId.value,
        select: {
          CurrentPartyContactMechanisms: {
            include: {
              ContactMechanism: {
                PostalAddress_Country: {},
              },
            },
          },
        },
      }),
      p.SalesOrder({
        objectId: this.editRequest.objectId,
        select: { SalesInvoicesWhereSalesOrder: {} },
      }),
      p.BillingProcess({
        sorting: [{ roleType: m.BillingProcess.Name }],
      }),
      p.Currency({
        predicate: {
          kind: 'Equals',
          propertyType: m.Currency.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.Currency.IsoCode }],
      }),
      p.SerialisedInventoryItemState({
        predicate: {
          kind: 'Equals',
          propertyType: m.SerialisedInventoryItemState.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.SerialisedInventoryItemState.Name }],
      })
    );

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.onPostPullInitialize(pullResult);

    this.salesInvoice = pullResult.object<SalesInvoice>(
      this.m.SalesOrder.SalesInvoicesWhereSalesOrder
    );
    this.currencies = pullResult.collection<Currency>(this.m.Currency);
    this.billingProcesses = pullResult.collection<BillingProcess>(
      this.m.BillingProcess
    );
    this.billingForOrderItems = this.billingProcesses?.find(
      (v: BillingProcess) =>
        v.UniqueId === 'ab01ccc2-6480-4fc0-b20e-265afd41fae2'
    );
    this.inventoryItemStates =
      pullResult.collection<SerialisedInventoryItemState>(
        this.m.SerialisedInventoryItemState
      );

    this.internalOrganisation =
      this.fetcher.getInternalOrganisation(pullResult);
    this.showIrpf = this.internalOrganisation.Country.IsoCode === 'ES';
    this.vatRegimes = this.internalOrganisation.Country.DerivedVatRegimes;
    this.facilities = this.fetcher.getWarehouses(pullResult);
    this.irpfRegimes = pullResult.collection<IrpfRegime>(this.m.IrpfRegime);
    this.vatClauses = pullResult.collection<VatClause>(this.m.VatClause);
    this.stores = pullResult.collection<Store>(this.m.Store);
    this.currencies = pullResult.collection<Currency>(this.m.Currency);

    const partyContactMechanisms: PartyContactMechanism[] =
      pullResult.collection<PartyContactMechanism>(
        this.m.Party.CurrentPartyContactMechanisms
      );
    this.shipFromAddresses = partyContactMechanisms
      ?.filter(
        (v: PartyContactMechanism) =>
          v.ContactMechanism.strategy.cls === this.m.PostalAddress
      )
      ?.map((v: PartyContactMechanism) => v.ContactMechanism);

    if (this.object.ShipToCustomer) {
      this.previousShipToCustomer = this.object.ShipToCustomer;
      this.updateShipToCustomer(this.object.ShipToCustomer);
    }

    if (this.object.BillToCustomer) {
      this.previousBillToCustomer = this.object.BillToCustomer;
      this.updateBillToCustomer(this.object.BillToCustomer);
    }

    if (this.object.BillToEndCustomer) {
      this.previousBillToEndCustomer = this.object.BillToEndCustomer;
      this.updateBillToEndCustomer(this.object.BillToEndCustomer);
    }

    if (this.object.ShipToEndCustomer) {
      this.previousShipToEndCustomer = this.object.ShipToEndCustomer;
      this.updateShipToEndCustomer(this.object.ShipToEndCustomer);
    }
  }

  public shipToCustomerAdded(party: Party): void {
    const customerRelationship =
      this.allors.context.create<CustomerRelationship>(
        this.m.CustomerRelationship
      );
    customerRelationship.Customer = party;
    customerRelationship.InternalOrganisation = this.internalOrganisation;

    this.object.ShipToCustomer = party;
  }

  public billToCustomerAdded(party: Party): void {
    const customerRelationship =
      this.allors.context.create<CustomerRelationship>(
        this.m.CustomerRelationship
      );
    customerRelationship.Customer = party;
    customerRelationship.InternalOrganisation = this.internalOrganisation;

    this.object.BillToCustomer = party;
  }

  public shipToEndCustomerAdded(party: Party): void {
    const customerRelationship =
      this.allors.context.create<CustomerRelationship>(
        this.m.CustomerRelationship
      );
    customerRelationship.Customer = party;
    customerRelationship.InternalOrganisation = this.internalOrganisation;

    this.object.ShipToEndCustomer = party;
  }

  public billToEndCustomerAdded(party: Party): void {
    const customerRelationship =
      this.allors.context.create<CustomerRelationship>(
        this.m.CustomerRelationship
      );
    customerRelationship.Customer = party;
    customerRelationship.InternalOrganisation = this.internalOrganisation;

    this.object.BillToEndCustomer = party;
  }

  public billToContactPersonAdded(person: Person): void {
    const organisationContactRelationship =
      this.allors.context.create<OrganisationContactRelationship>(
        this.m.OrganisationContactRelationship
      );
    organisationContactRelationship.Organisation = this.object
      .BillToCustomer as Organisation;
    organisationContactRelationship.Contact = person;

    this.billToContacts.push(person);
    this.object.BillToContactPerson = person;
  }

  public billToEndCustomerContactPersonAdded(person: Person): void {
    const organisationContactRelationship =
      this.allors.context.create<OrganisationContactRelationship>(
        this.m.OrganisationContactRelationship
      );
    organisationContactRelationship.Organisation = this.object
      .BillToEndCustomer as Organisation;
    organisationContactRelationship.Contact = person;

    this.billToEndCustomerContacts.push(person);
    this.object.BillToEndCustomerContactPerson = person;
  }

  public shipToContactPersonAdded(person: Person): void {
    const organisationContactRelationship =
      this.allors.context.create<OrganisationContactRelationship>(
        this.m.OrganisationContactRelationship
      );
    organisationContactRelationship.Organisation = this.object
      .ShipToCustomer as Organisation;
    organisationContactRelationship.Contact = person;

    this.shipToContacts.push(person);
    this.object.ShipToContactPerson = person;
  }

  public shipToEndCustomerContactPersonAdded(person: Person): void {
    const organisationContactRelationship =
      this.allors.context.create<OrganisationContactRelationship>(
        this.m.OrganisationContactRelationship
      );
    organisationContactRelationship.Organisation = this.object
      .ShipToEndCustomer as Organisation;
    organisationContactRelationship.Contact = person;

    this.shipToEndCustomerContacts.push(person);
    this.object.ShipToEndCustomerContactPerson = person;
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
    partyContactMechanism.Party = this.object.BillToCustomer;
    this.object.AssignedBillToContactMechanism =
      partyContactMechanism.ContactMechanism;
  }

  public billToEndCustomerContactMechanismAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.billToEndCustomerContactMechanisms.push(
      partyContactMechanism.ContactMechanism
    );
    partyContactMechanism.Party = this.object.BillToEndCustomer;
    this.object.AssignedBillToEndCustomerContactMechanism =
      partyContactMechanism.ContactMechanism;
  }

  public shipToAddressAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.shipToAddresses.push(partyContactMechanism.ContactMechanism);
    partyContactMechanism.Party = this.object.ShipToCustomer;
    this.object.AssignedShipToAddress =
      partyContactMechanism.ContactMechanism as PostalAddress;
  }

  public shipToEndCustomerAddressAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.shipToEndCustomerAddresses.push(
      partyContactMechanism.ContactMechanism
    );
    partyContactMechanism.Party = this.object.ShipToEndCustomer;
    this.object.AssignedShipToEndCustomerAddress =
      partyContactMechanism.ContactMechanism as PostalAddress;
  }

  public shipFromAddressAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.shipFromAddresses.push(partyContactMechanism.ContactMechanism);
    partyContactMechanism.Party = this.object.TakenBy;
    this.object.AssignedShipFromAddress =
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
          PartyContactMechanismsWhereParty: x,
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

    this.allors.context.pull(pulls).subscribe((pullResult) => {
      if (this.object.ShipToCustomer !== this.previousShipToCustomer) {
        this.object.AssignedShipToAddress = null;
        this.object.ShipToContactPerson = null;
        this.previousShipToCustomer = this.object.ShipToCustomer;
      }

      if (
        this.object.ShipToCustomer != null &&
        this.object.BillToCustomer == null
      ) {
        this.object.BillToCustomer = this.object.ShipToCustomer;
        this.updateBillToCustomer(this.object.ShipToCustomer);
      }

      const partyContactMechanisms: PartyContactMechanism[] =
        pullResult.collection<PartyContactMechanism>(
          this.m.Party.CurrentPartyContactMechanisms
        );
      this.shipToAddresses = partyContactMechanisms
        ?.filter(
          (v: PartyContactMechanism) =>
            v.ContactMechanism.strategy.cls === m.PostalAddress
        )
        ?.map((v: PartyContactMechanism) => v.ContactMechanism);
      this.shipToContacts = pullResult.collection<Person>(
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
          PartyContactMechanismsWhereParty: x,
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

    this.allors.context.pull(pulls).subscribe((pullResult) => {
      if (this.object.BillToCustomer !== this.previousBillToCustomer) {
        this.object.AssignedBillToContactMechanism = null;
        this.object.BillToContactPerson = null;
        this.previousBillToCustomer = this.object.BillToCustomer;
      }

      if (
        this.object.BillToCustomer != null &&
        this.object.ShipToCustomer == null
      ) {
        this.object.ShipToCustomer = this.object.BillToCustomer;
        this.updateShipToCustomer(this.object.ShipToCustomer);
      }

      const partyContactMechanisms: PartyContactMechanism[] =
        pullResult.collection<PartyContactMechanism>(
          this.m.Party.CurrentPartyContactMechanisms
        );
      this.billToContactMechanisms = partyContactMechanisms?.map(
        (v: PartyContactMechanism) => v.ContactMechanism
      );
      this.billToContacts = pullResult.collection<Person>(
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
          PartyContactMechanismsWhereParty: x,
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

    this.allors.context.pull(pulls).subscribe((pullResult) => {
      if (this.object.BillToEndCustomer !== this.previousBillToEndCustomer) {
        this.object.AssignedBillToEndCustomerContactMechanism = null;
        this.object.BillToEndCustomerContactPerson = null;
        this.previousBillToEndCustomer = this.object.BillToEndCustomer;
      }

      if (
        this.object.BillToEndCustomer != null &&
        this.object.ShipToEndCustomer == null
      ) {
        this.object.ShipToEndCustomer = this.object.BillToEndCustomer;
        this.updateShipToEndCustomer(this.object.ShipToEndCustomer);
      }

      const partyContactMechanisms: PartyContactMechanism[] =
        pullResult.collection<PartyContactMechanism>(
          this.m.Party.CurrentPartyContactMechanisms
        );
      this.billToEndCustomerContactMechanisms = partyContactMechanisms?.map(
        (v: PartyContactMechanism) => v.ContactMechanism
      );
      this.billToEndCustomerContacts = pullResult.collection<Person>(
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
          PartyContactMechanismsWhereParty: x,
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

    this.allors.context.pull(pulls).subscribe((pullResult) => {
      if (this.object.ShipToEndCustomer !== this.previousShipToEndCustomer) {
        this.object.AssignedShipToEndCustomerAddress = null;
        this.object.ShipToEndCustomerContactPerson = null;
        this.previousShipToEndCustomer = this.object.ShipToEndCustomer;
      }

      if (
        this.object.ShipToEndCustomer != null &&
        this.object.BillToEndCustomer == null
      ) {
        this.object.BillToEndCustomer = this.object.ShipToEndCustomer;
        this.updateBillToEndCustomer(this.object.BillToEndCustomer);
      }

      const partyContactMechanisms: PartyContactMechanism[] =
        pullResult.collection<PartyContactMechanism>(
          this.m.Party.CurrentPartyContactMechanisms
        );
      this.shipToEndCustomerAddresses = partyContactMechanisms
        ?.filter(
          (v: PartyContactMechanism) =>
            v.ContactMechanism.strategy.cls === m.PostalAddress
        )
        ?.map((v: PartyContactMechanism) => v.ContactMechanism);
      this.shipToEndCustomerContacts = pullResult.collection<Person>(
        this.m.Party.CurrentContacts
      );
    });
  }

  public shipToCustomerSelected(party: IObject) {
    if (party) {
      this.updateShipToCustomer(party as Party);
    }
  }
}
