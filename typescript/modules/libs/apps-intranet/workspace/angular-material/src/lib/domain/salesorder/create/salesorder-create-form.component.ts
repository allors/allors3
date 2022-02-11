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
  SalesOrder,
  Store,
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
  templateUrl: './salesorder-create-form.component.html',
  providers: [ContextService],
})
export class SalesOrderCreateFormComponent extends AllorsFormComponent<SalesOrder> {
  readonly m: M;
  billToContactMechanisms: ContactMechanism[] = [];
  billToEndCustomerContactMechanisms: ContactMechanism[] = [];
  shipFromAddresses: ContactMechanism[] = [];
  shipToAddresses: ContactMechanism[] = [];
  shipToEndCustomerAddresses: ContactMechanism[] = [];
  billToContacts: Person[] = [];
  billToEndCustomerContacts: Person[] = [];
  shipToContacts: Person[] = [];
  shipToEndCustomerContacts: Person[] = [];
  stores: Store[];
  internalOrganisation: InternalOrganisation;
  currencies: Currency[];
  vatRegimes: VatRegime[];
  irpfRegimes: IrpfRegime[];

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

  customersFilter: SearchFactory;
  currencyInitialRole: Currency;
  takenByContactMechanismInitialRole: ContactMechanism;
  billToContactMechanismInitialRole: ContactMechanism;
  billToEndCustomerContactMechanismInitialRole: ContactMechanism;
  shipToEndCustomerAddressInitialRole: ContactMechanism;
  shipFromAddressInitialRole: PostalAddress;
  shipToAddressInitialRole: PostalAddress;
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
      p.Currency({
        predicate: {
          kind: 'Equals',
          propertyType: m.Currency.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.Currency.IsoCode }],
      }),
      p.IrpfRegime({ sorting: [{ roleType: m.IrpfRegime.Name }] }),
      p.Store({
        predicate: {
          kind: 'Equals',
          propertyType: m.Store.InternalOrganisation,
          value: this.internalOrganisationId.value,
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
      })
    );

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.onPostPullInitialize(pullResult);

    this.internalOrganisation =
      this.fetcher.getInternalOrganisation(pullResult);
    this.showIrpf = this.internalOrganisation.Country.IsoCode === 'ES';
    this.vatRegimes = this.internalOrganisation.Country.DerivedVatRegimes;
    this.stores = pullResult.collection<Store>(this.m.Store);
    this.currencies = pullResult.collection<Currency>(this.m.Currency);
    this.irpfRegimes = pullResult.collection<IrpfRegime>(this.m.IrpfRegime);

    this.object.TakenBy = this.internalOrganisation;

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

    if (this.stores.length === 1) {
      this.object.Store = this.stores[0];
    }

    if (this.object.ShipToCustomer) {
      this.updateShipToCustomer(this.object.ShipToCustomer);
    }

    if (this.object.BillToCustomer) {
      this.updateBillToCustomer(this.object.BillToCustomer);
    }

    if (this.object.BillToEndCustomer) {
      this.updateBillToEndCustomer(this.object.BillToEndCustomer);
    }

    if (this.object.ShipToEndCustomer) {
      this.updateShipToEndCustomer(this.object.ShipToEndCustomer);
    }

    this.previousShipToCustomer = this.object.ShipToCustomer;
    this.previousShipToEndCustomer = this.object.ShipToEndCustomer;
    this.previousBillToCustomer = this.object.BillToCustomer;
    this.previousBillToEndCustomer = this.object.BillToEndCustomer;
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

  public billToContactMechanismAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.billToContactMechanisms.push(partyContactMechanism.ContactMechanism);
    this.object.BillToCustomer.addPartyContactMechanism(partyContactMechanism);
    this.object.AssignedBillToContactMechanism =
      partyContactMechanism.ContactMechanism;
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

  public shipToAddressAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.shipToAddresses.push(partyContactMechanism.ContactMechanism);
    this.object.ShipToCustomer.addPartyContactMechanism(partyContactMechanism);
    this.object.AssignedShipToAddress =
      partyContactMechanism.ContactMechanism as PostalAddress;
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

  public shipFromAddressAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.shipFromAddresses.push(partyContactMechanism.ContactMechanism);
    this.object.TakenBy.addPartyContactMechanism(partyContactMechanism);
    this.object.AssignedShipFromAddress =
      partyContactMechanism.ContactMechanism as PostalAddress;
  }

  public shipToCustomerSelected(party: IObject) {
    if (party) {
      this.updateShipToCustomer(party as Party);
    }
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
          Locale: {
            Country: {
              Currency: x,
            },
          },
          OrderAddress: x,
          BillingAddress: x,
          ShippingAddress: x,
          GeneralCorrespondence: x,
        },
      }),
    ];

    this.allors.context.pull(pulls).subscribe((pullResult) => {
      if (
        this.previousShipToCustomer &&
        this.object.ShipToCustomer !== this.previousShipToCustomer
      ) {
        this.object.ShipToContactPerson = null;
      }
      this.previousShipToCustomer = this.object.ShipToCustomer;

      if (
        this.object.ShipToCustomer != null &&
        this.object.BillToCustomer == null
      ) {
        this.object.BillToCustomer = this.object.ShipToCustomer;
        this.updateBillToCustomer(this.object.ShipToCustomer);
      }

      const partyContactMechanisms: PartyContactMechanism[] =
        pullResult.collection<PartyContactMechanism>(
          m.Party.CurrentPartyContactMechanisms
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

      this.setDerivedInitialRoles();
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
          Locale: {
            Country: {
              Currency: x,
            },
          },
          OrderAddress: x,
          BillingAddress: x,
          ShippingAddress: x,
          GeneralCorrespondence: x,
        },
      }),
    ];

    this.allors.context.pull(pulls).subscribe((pullResult) => {
      if (
        this.previousBillToCustomer &&
        this.object.BillToCustomer !== this.previousBillToCustomer
      ) {
        this.object.BillToContactPerson = null;
      }
      this.previousBillToCustomer = this.object.BillToCustomer;

      if (
        this.object.BillToCustomer != null &&
        this.object.ShipToCustomer == null
      ) {
        this.object.ShipToCustomer = this.object.BillToCustomer;
        this.updateShipToCustomer(this.object.ShipToCustomer);
      }

      const partyContactMechanisms: PartyContactMechanism[] =
        pullResult.collection<PartyContactMechanism>(
          m.Party.CurrentPartyContactMechanisms
        );
      this.billToContactMechanisms = partyContactMechanisms?.map(
        (v: PartyContactMechanism) => v.ContactMechanism
      );
      this.billToContacts = pullResult.collection<Person>(
        this.m.Party.CurrentContacts
      );

      this.setDerivedInitialRoles();
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
      pull.Party({
        object: party,
        include: {
          PreferredCurrency: x,
          Locale: {
            Country: {
              Currency: x,
            },
          },
          OrderAddress: x,
          BillingAddress: x,
          ShippingAddress: x,
          GeneralCorrespondence: x,
        },
      }),
    ];

    this.allors.context.pull(pulls).subscribe((pullResult) => {
      if (
        this.previousBillToEndCustomer &&
        this.object.BillToEndCustomer !== this.previousBillToEndCustomer
      ) {
        this.object.BillToEndCustomerContactPerson = null;
      }
      this.previousBillToEndCustomer = this.object.BillToEndCustomer;

      if (
        this.object.BillToEndCustomer != null &&
        this.object.ShipToEndCustomer == null
      ) {
        this.object.ShipToEndCustomer = this.object.BillToEndCustomer;
        this.updateShipToEndCustomer(this.object.ShipToEndCustomer);
      }

      const partyContactMechanisms: PartyContactMechanism[] =
        pullResult.collection<PartyContactMechanism>(
          m.Party.CurrentPartyContactMechanisms
        );
      this.billToEndCustomerContactMechanisms = partyContactMechanisms?.map(
        (v: PartyContactMechanism) => v.ContactMechanism
      );
      this.billToEndCustomerContacts = pullResult.collection<Person>(
        m.Party.CurrentContacts
      );

      this.setDerivedInitialRoles();
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
      pull.Party({
        object: party,
        include: {
          PreferredCurrency: x,
          Locale: {
            Country: {
              Currency: x,
            },
          },
          OrderAddress: x,
          BillingAddress: x,
          ShippingAddress: x,
          GeneralCorrespondence: x,
        },
      }),
    ];

    this.allors.context.pull(pulls).subscribe((pullResult) => {
      if (
        this.previousShipToEndCustomer &&
        this.object.ShipToEndCustomer !== this.previousShipToEndCustomer
      ) {
        this.object.ShipToEndCustomerContactPerson = null;
      }

      this.previousShipToEndCustomer = this.object.ShipToEndCustomer;

      if (
        this.object.ShipToEndCustomer != null &&
        this.object.BillToEndCustomer == null
      ) {
        this.object.BillToEndCustomer = this.object.ShipToEndCustomer;
        this.updateBillToEndCustomer(this.object.BillToEndCustomer);
      }

      const partyContactMechanisms: PartyContactMechanism[] =
        pullResult.collection<PartyContactMechanism>(
          m.Party.CurrentPartyContactMechanisms
        );
      this.shipToEndCustomerAddresses = partyContactMechanisms
        ?.filter(
          (v: PartyContactMechanism) =>
            v.ContactMechanism.strategy.cls === m.PostalAddress
        )
        ?.map((v: PartyContactMechanism) => v.ContactMechanism);
      this.shipToEndCustomerContacts = pullResult.collection<Person>(
        m.Party.CurrentContacts
      );

      this.setDerivedInitialRoles();
    });
  }

  private setDerivedInitialRoles() {
    this.currencyInitialRole =
      this.object.BillToCustomer?.PreferredCurrency ??
      this.object.BillToCustomer?.Locale?.Country?.Currency ??
      this.object.TakenBy?.PreferredCurrency;
    this.takenByContactMechanismInitialRole =
      this.object.TakenBy?.OrderAddress ??
      this.object.TakenBy?.BillingAddress ??
      this.object.TakenBy?.GeneralCorrespondence;
    this.billToContactMechanismInitialRole =
      this.object.BillToCustomer?.BillingAddress ??
      this.object.BillToCustomer?.ShippingAddress ??
      this.object.BillToCustomer?.GeneralCorrespondence;
    this.billToEndCustomerContactMechanismInitialRole =
      this.object.BillToEndCustomer?.BillingAddress ??
      this.object.BillToEndCustomer?.ShippingAddress ??
      this.object.BillToEndCustomer?.GeneralCorrespondence;
    this.shipToEndCustomerAddressInitialRole =
      this.object.ShipToEndCustomer?.ShippingAddress ??
      this.object.ShipToEndCustomer?.GeneralCorrespondence;
    this.shipFromAddressInitialRole = this.object.TakenBy?.ShippingAddress;
    this.shipToAddressInitialRole = this.object.ShipToCustomer?.ShippingAddress;
  }
}
