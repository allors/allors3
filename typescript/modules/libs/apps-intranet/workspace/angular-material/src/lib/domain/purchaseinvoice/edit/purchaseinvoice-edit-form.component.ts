import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult, IObject } from '@allors/system/workspace/domain';
import {
  ContactMechanism,
  Currency,
  CustomerRelationship,
  InternalOrganisation,
  IrpfRegime,
  Organisation,
  OrganisationContactRelationship,
  Party,
  PartyContactMechanism,
  Person,
  PostalAddress,
  PurchaseInvoice,
  PurchaseInvoiceType,
  SupplierRelationship,
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
  selector: 'purchaseinvoice-edit-form',
  templateUrl: './purchaseinvoice-edit-form.component.html',
  providers: [ContextService],
})
export class PurchaseInvoiceEditFormComponent extends AllorsFormComponent<PurchaseInvoice> {
  readonly m: M;

  currencies: Currency[];
  vatRegimes: VatRegime[];
  irpfRegimes: IrpfRegime[];
  purchaseInvoiceTypes: PurchaseInvoiceType[];

  billedFromContacts: Person[] = [];
  billedFromContactMechanisms: ContactMechanism[] = [];
  billToContacts: Person[] = [];
  shipToCustomerAddresses: ContactMechanism[] = [];
  shipToCustomerContacts: Person[] = [];
  billToEndCustomerContactMechanisms: ContactMechanism[] = [];
  billToEndCustomerContacts: Person[] = [];
  shipToEndCustomerAddresses: ContactMechanism[] = [];
  shipToEndCustomerContacts: Person[] = [];

  addBilledFrom = false;
  addBilledFromContactMechanism = false;
  addBilledFromContactPerson = false;
  addBillToContactMechanism = false;
  addBillToContactPerson = false;
  addShipToCustomer = false;
  addShipToCustomerAddress = false;
  addShipToCustomerContactPerson = false;
  addBillToEndCustomer = false;
  addBillToEndCustomerContactMechanism = false;
  addBillToEndCustomerContactPerson = false;
  addShipToEndCustomer = false;
  addShipToEndCustomerAddress = false;
  addShipToEndCustomerContactPerson = false;

  private previousBilledFrom: Party;
  private previousShipToCustomer: Party;
  private previousBillToEndCustomer: Party;
  private previousShipToEndCustomer: Party;
  internalOrganisation: InternalOrganisation;

  customersFilter: SearchFactory;
  employeeFilter: SearchFactory;
  suppliersFilter: SearchFactory;
  showIrpf: boolean;

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
    public fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;

    this.customersFilter = Filters.customersFilter(
      this.m,
      this.internalOrganisationId.value
    );
    this.employeeFilter = Filters.employeeFilter(
      this.m,
      this.internalOrganisationId.value
    );
    this.suppliersFilter = Filters.suppliersFilter(
      this.m,
      this.internalOrganisationId.value
    );
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      this.fetcher.internalOrganisation,
      p.PurchaseInvoice({
        objectId: this.editRequest.objectId,
        include: {
          BilledFrom: {},
          AssignedVatRegime: {},
          PurchaseInvoiceItems: {
            InvoiceItemType: {},
          },
          DerivedBilledFromContactMechanism: {
            PostalAddress_Country: {},
          },
          BilledFromContactPerson: {},
          BillToEndCustomer: {},
          BillToEndCustomerContactPerson: {},
          ShipToEndCustomer: {},
          ShipToEndCustomerContactPerson: {},
          ElectronicDocuments: {},
          PurchaseInvoiceState: {},
          CreatedBy: {},
          LastModifiedBy: {},
          DerivedVatRegime: {
            VatRates: {},
          },
          DerivedBillToEndCustomerContactMechanism: {
            PostalAddress_Country: {},
          },
          DerivedShipToEndCustomerAddress: {
            Country: {},
          },
        },
      }),
      p.IrpfRegime({ sorting: [{ roleType: m.IrpfRegime.Name }] }),
      p.Currency({
        predicate: {
          kind: 'Equals',
          propertyType: m.Currency.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.Currency.IsoCode }],
      }),
      p.PurchaseInvoiceType({
        predicate: {
          kind: 'Equals',
          propertyType: m.PurchaseInvoiceType.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.PurchaseInvoiceType.Name }],
      })
    );

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    this.object = pullResult.object('_object');

    this.onPostPullInitialize(pullResult);

    this.internalOrganisation =
      this.fetcher.getInternalOrganisation(pullResult);
    this.showIrpf = this.internalOrganisation.Country.IsoCode === 'ES';
    this.vatRegimes = this.internalOrganisation.Country.DerivedVatRegimes;
    this.irpfRegimes = pullResult.collection<IrpfRegime>(this.m.IrpfRegime);
    this.currencies = pullResult.collection<Currency>(this.m.Currency);
    this.purchaseInvoiceTypes = pullResult.collection<PurchaseInvoiceType>(
      this.m.PurchaseInvoiceType
    );

    if (this.object.BilledFrom) {
      this.updateBilledFrom(this.object.BilledFrom);
    }

    if (this.object.ShipToCustomer) {
      this.updateShipToCustomer(this.object.ShipToCustomer);
    }

    if (this.object.BillToEndCustomer) {
      this.updateBillToEndCustomer(this.object.BillToEndCustomer);
    }

    if (this.object.ShipToEndCustomer) {
      this.updateShipToEndCustomer(this.object.ShipToEndCustomer);
    }

    this.previousBilledFrom = this.object.BilledFrom;
    this.previousShipToCustomer = this.object.ShipToCustomer;
    this.previousBillToEndCustomer = this.object.BillToEndCustomer;
    this.previousShipToEndCustomer = this.object.ShipToEndCustomer;
  }

  public billedFromAdded(organisation: Organisation): void {
    const supplierRelationship =
      this.allors.context.create<SupplierRelationship>(
        this.m.SupplierRelationship
      );
    supplierRelationship.Supplier = organisation;
    supplierRelationship.InternalOrganisation = this.internalOrganisation;

    this.object.BilledFrom = organisation;
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

  public billToEndCustomerAdded(party: Party): void {
    const customerRelationship =
      this.allors.context.create<CustomerRelationship>(
        this.m.CustomerRelationship
      );
    customerRelationship.Customer = party;
    customerRelationship.InternalOrganisation = this.internalOrganisation;

    this.object.BillToEndCustomer = party;
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

  public billedFromContactPersonAdded(person: Person): void {
    const organisationContactRelationship =
      this.allors.context.create<OrganisationContactRelationship>(
        this.m.OrganisationContactRelationship
      );
    organisationContactRelationship.Organisation = this.object
      .BilledFrom as Organisation;
    organisationContactRelationship.Contact = person;

    this.billedFromContacts.push(person);
    this.object.BilledFromContactPerson = person;
  }

  public shipToCustomerContactPersonAdded(person: Person): void {
    const organisationContactRelationship =
      this.allors.context.create<OrganisationContactRelationship>(
        this.m.OrganisationContactRelationship
      );
    organisationContactRelationship.Organisation = this.object
      .BilledFrom as Organisation;
    organisationContactRelationship.Contact = person;

    this.shipToCustomerContacts.push(person);
    this.object.ShipToCustomerContactPerson = person;
  }

  public billToEndCustomerContactPersonAdded(person: Person): void {
    const organisationContactRelationship =
      this.allors.context.create<OrganisationContactRelationship>(
        this.m.OrganisationContactRelationship
      );
    organisationContactRelationship.Organisation = this.object
      .ShipToEndCustomer as Organisation;
    organisationContactRelationship.Contact = person;

    this.billToEndCustomerContacts.push(person);
    this.object.BillToEndCustomerContactPerson = person;
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

  public billedFromContactMechanismAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.billedFromContactMechanisms.push(
      partyContactMechanism.ContactMechanism
    );
    this.object.BilledFrom.addPartyContactMechanism(partyContactMechanism);
    this.object.AssignedBilledFromContactMechanism =
      partyContactMechanism.ContactMechanism;
  }

  public shipToCustomerAddressAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.shipToCustomerAddresses.push(partyContactMechanism.ContactMechanism);
    this.object.ShipToCustomer.addPartyContactMechanism(partyContactMechanism);
    this.object.AssignedShipToCustomerAddress =
      partyContactMechanism.ContactMechanism as PostalAddress;
  }

  public billToEndCustomerContactMechanismAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.billToEndCustomerContactMechanisms.push(
      partyContactMechanism.ContactMechanism
    );
    this.object.BillToEndCustomer.addPartyContactMechanism(
      partyContactMechanism
    );
    this.object.AssignedBillToEndCustomerContactMechanism =
      partyContactMechanism.ContactMechanism;
  }

  public shipToEndCustomerAddressAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.shipToEndCustomerAddresses.push(
      partyContactMechanism.ContactMechanism
    );
    this.object.ShipToEndCustomer.addPartyContactMechanism(
      partyContactMechanism
    );
    this.object.AssignedShipToEndCustomerAddress =
      partyContactMechanism.ContactMechanism as PostalAddress;
  }

  public billedFromSelected(organisation: IObject) {
    if (organisation) {
      this.updateBilledFrom(organisation as Organisation);
    }
  }

  public shipToCustomerSelected(party: IObject) {
    if (party) {
      this.updateShipToCustomer(party as Party);
    }
  }

  public billToEndCustomerSelected(party: IObject) {
    if (party) {
      this.updateBillToEndCustomer(party as Party);
    }
  }

  public shipToEndCustomerSelected(party: IObject) {
    if (party) {
      this.updateShipToEndCustomer(party as Party);
    }
  }

  private updateBilledFrom(party: Party): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const pulls = [
      pull.Organisation({
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
      pull.Organisation({
        object: party,
        select: {
          CurrentContacts: x,
        },
      }),
      pull.PurchaseOrder({
        predicate: {
          kind: 'Equals',
          propertyType: m.PurchaseOrder.TakenViaSupplier,
          object: party,
        },
        sorting: [{ roleType: m.PurchaseOrder.OrderNumber }],
      }),
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
      if (this.object.BilledFrom !== this.previousBilledFrom) {
        this.object.AssignedBilledFromContactMechanism = null;
        this.object.BilledFromContactPerson = null;
        this.previousBilledFrom = this.object.BilledFrom;
      }

      const partyContactMechanisms: PartyContactMechanism[] =
        loaded.collection<PartyContactMechanism>(
          m.Party.CurrentPartyContactMechanisms
        );
      this.billedFromContactMechanisms = partyContactMechanisms
        ?.filter(
          (v: PartyContactMechanism) =>
            v.ContactMechanism.strategy.cls === m.PostalAddress
        )
        ?.map((v: PartyContactMechanism) => v.ContactMechanism);
      this.billedFromContacts = loaded.collection<Person>(
        m.Party.CurrentContacts
      );
    });
  }

  private updateShipToCustomer(party: Party) {
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
      if (this.object.ShipToCustomer !== this.previousShipToCustomer) {
        this.object.AssignedShipToEndCustomerAddress = null;
        this.object.ShipToCustomerContactPerson = null;
        this.previousShipToCustomer = this.object.ShipToCustomer;
      }

      const partyContactMechanisms: PartyContactMechanism[] =
        loaded.collection<PartyContactMechanism>(
          m.Party.CurrentPartyContactMechanisms
        );
      this.shipToCustomerAddresses = partyContactMechanisms?.map(
        (v: PartyContactMechanism) => v.ContactMechanism
      );
      this.shipToCustomerContacts = loaded.collection<Person>(
        m.Party.CurrentContacts
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
        loaded.collection<PartyContactMechanism>(
          m.Party.CurrentPartyContactMechanisms
        );
      this.billToEndCustomerContactMechanisms = partyContactMechanisms
        ?.filter(
          (v: PartyContactMechanism) =>
            v.ContactMechanism.strategy.cls === m.PostalAddress
        )
        ?.map((v: PartyContactMechanism) => v.ContactMechanism);
      this.billToEndCustomerContacts = loaded.collection<Person>(
        m.Party.CurrentContacts
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
        loaded.collection<PartyContactMechanism>(
          m.Party.CurrentPartyContactMechanisms
        );
      this.shipToEndCustomerAddresses = partyContactMechanisms
        ?.filter(
          (v: PartyContactMechanism) =>
            v.ContactMechanism.strategy.cls === m.PostalAddress
        )
        ?.map((v: PartyContactMechanism) => v.ContactMechanism);
      this.shipToEndCustomerContacts = loaded.collection<Person>(
        m.Party.CurrentContacts
      );
    });
  }
}
