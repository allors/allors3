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
import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { Filters } from '../../../filters/filters';

@Component({
  templateUrl: './purchaseinvoice-create-form.component.html',
  providers: [ContextService],
})
export class PurchaseInvoiceCreateFormComponent extends AllorsFormComponent<PurchaseInvoice> {
  public m: M;
  currencies: Currency[];
  vatRegimes: VatRegime[];
  irpfRegimes: IrpfRegime[];
  purchaseInvoiceTypes: PurchaseInvoiceType[];
  internalOrganisation: InternalOrganisation;

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

  customersFilter: SearchFactory;
  employeeFilter: SearchFactory;
  suppliersFilter: SearchFactory;
  billedFromContactMechanismInitialRole: ContactMechanism;
  shipToCustomerAddressInitialRole: ContactMechanism;
  billToEndCustomerContactMechanismInitialRole: ContactMechanism;
  shipToEndCustomerAddressInitialRole: ContactMechanism;
  currencyInitialRole: Currency;
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
    private fetcher: FetcherService,
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
    this.object = this.context.create(this.createRequest.objectType);

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

    this.object.BilledTo = this.internalOrganisation;

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

    this.currencyInitialRole = this.internalOrganisation.PreferredCurrency;
  }

  public billedFromAdded(organisation: Organisation): void {
    const supplierRelationship =
      this.allors.context.create<SupplierRelationship>(
        this.m.SupplierRelationship
      );
    supplierRelationship.Supplier = organisation;
    supplierRelationship.InternalOrganisation = this.internalOrganisation;

    this.object.BilledFrom = organisation;
    this.billedFromSelected(organisation);
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
    partyContactMechanism.Party = this.object.BilledFrom;
    this.object.AssignedBilledFromContactMechanism =
      partyContactMechanism.ContactMechanism;
  }

  public shipToCustomerAddressAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.shipToCustomerAddresses.push(partyContactMechanism.ContactMechanism);
    partyContactMechanism.Party = this.object.ShipToCustomer;
    this.object.AssignedShipToCustomerAddress =
      partyContactMechanism.ContactMechanism as PostalAddress;
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
      pull.Organisation({
        object: party,
        select: {
          CurrentContacts: x,
        },
      }),
      pull.Organisation({
        object: party,
        name: 'selectedSupplier',
        include: {
          OrderAddress: x,
        },
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

      const selectedSupplier = loaded.object<Organisation>('selectedSupplier');
      this.billedFromContactMechanismInitialRole =
        selectedSupplier?.OrderAddress;
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
        name: 'selectedParty',
        include: {
          PreferredCurrency: x,
          BillingAddress: x,
          GeneralCorrespondence: x,
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

      const selectedparty = loaded.object<Party>('selectedParty');
      this.shipToCustomerAddressInitialRole =
        selectedparty?.BillingAddress ?? selectedparty.GeneralCorrespondence;
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
      pull.Party({
        object: party,
        name: 'selectedParty',
        include: {
          PreferredCurrency: x,
          BillingAddress: x,
          GeneralCorrespondence: x,
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

      const selectedparty = loaded.object<Party>('selectedParty');
      this.billToEndCustomerContactMechanismInitialRole =
        selectedparty?.BillingAddress ?? selectedparty.GeneralCorrespondence;
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
      pull.Party({
        object: party,
        name: 'selectedParty',
        include: {
          PreferredCurrency: x,
          BillingAddress: x,
          GeneralCorrespondence: x,
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

      const selectedparty = loaded.object<Party>('selectedParty');
      this.shipToEndCustomerAddressInitialRole =
        selectedparty?.BillingAddress ?? selectedparty.GeneralCorrespondence;
    });
  }
}
