import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import {
  EditIncludeHandler,
  Node,
  CreateOrEditPullHandler,
  Pull,
  IPullResult,
  PostCreatePullHandler,
} from '@allors/system/workspace/domain';
import {
  BasePrice,
  InternalOrganisation,
  SalesOrder,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { Filters } from '../../../filters/filters';

@Component({
  templateUrl: './salesorder-create-form.component.html',
  providers: [ContextService],
})
export class SalesOrderCreateFormComponent
  extends AllorsFormComponent<SalesOrder>
  implements CreateOrEditPullHandler, EditIncludeHandler, PostCreatePullHandler
{
  readonly m: M;

  title = 'Add Sales Order';

  order: SalesOrder;
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
  private subscription: Subscription;

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
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest([
      this.refreshService.refresh$,
      this.internalOrganisationId.observable$,
    ])
      .pipe(
        switchMap(([, internalOrganisationId]) => {
          const pulls = [
            this.fetcher.internalOrganisation,
            pull.Currency({
              predicate: {
                kind: 'Equals',
                propertyType: m.Currency.IsActive,
                value: true,
              },
              sorting: [{ roleType: m.Currency.IsoCode }],
            }),
            pull.IrpfRegime({ sorting: [{ roleType: m.IrpfRegime.Name }] }),
            pull.Store({
              predicate: {
                kind: 'Equals',
                propertyType: m.Store.InternalOrganisation,
                value: internalOrganisationId,
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
            internalOrganisationId
          );

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.internalOrganisation =
          this.fetcher.getInternalOrganisation(loaded);
        this.showIrpf = this.internalOrganisation.Country.IsoCode === 'ES';
        this.vatRegimes = this.internalOrganisation.Country.DerivedVatRegimes;
        this.stores = loaded.collection<Store>(m.Store);
        this.currencies = loaded.collection<Currency>(m.Currency);
        this.irpfRegimes = loaded.collection<IrpfRegime>(m.IrpfRegime);

        this.order = this.allors.context.create<SalesOrder>(m.SalesOrder);
        this.order.TakenBy = this.internalOrganisation;

        const partyContactMechanisms: PartyContactMechanism[] =
          loaded.collection<PartyContactMechanism>(
            m.Party.CurrentPartyContactMechanisms
          );
        this.shipFromAddresses = partyContactMechanisms
          ?.filter(
            (v: PartyContactMechanism) =>
              v.ContactMechanism.strategy.cls === m.PostalAddress
          )
          ?.map((v: PartyContactMechanism) => v.ContactMechanism);

        if (this.stores.length === 1) {
          this.order.Store = this.stores[0];
        }

        if (this.order.ShipToCustomer) {
          this.updateShipToCustomer(this.order.ShipToCustomer);
        }

        if (this.order.BillToCustomer) {
          this.updateBillToCustomer(this.order.BillToCustomer);
        }

        if (this.order.BillToEndCustomer) {
          this.updateBillToEndCustomer(this.order.BillToEndCustomer);
        }

        if (this.order.ShipToEndCustomer) {
          this.updateShipToEndCustomer(this.order.ShipToEndCustomer);
        }

        this.previousShipToCustomer = this.order.ShipToCustomer;
        this.previousShipToEndCustomer = this.order.ShipToEndCustomer;
        this.previousBillToCustomer = this.order.BillToCustomer;
        this.previousBillToEndCustomer = this.order.BillToEndCustomer;
      });
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

    this.allors.context.pull(pulls).subscribe((loaded) => {
      if (
        this.previousShipToCustomer &&
        this.order.ShipToCustomer !== this.previousShipToCustomer
      ) {
        this.order.ShipToContactPerson = null;
      }
      this.previousShipToCustomer = this.order.ShipToCustomer;

      if (
        this.order.ShipToCustomer != null &&
        this.order.BillToCustomer == null
      ) {
        this.order.BillToCustomer = this.order.ShipToCustomer;
        this.updateBillToCustomer(this.order.ShipToCustomer);
      }

      const partyContactMechanisms: PartyContactMechanism[] =
        loaded.collection<PartyContactMechanism>(
          m.Party.CurrentPartyContactMechanisms
        );
      this.shipToAddresses = partyContactMechanisms
        ?.filter(
          (v: PartyContactMechanism) =>
            v.ContactMechanism.strategy.cls === m.PostalAddress
        )
        ?.map((v: PartyContactMechanism) => v.ContactMechanism);
      this.shipToContacts = loaded.collection<Person>(m.Party.CurrentContacts);

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

    this.allors.context.pull(pulls).subscribe((loaded) => {
      if (
        this.previousBillToCustomer &&
        this.order.BillToCustomer !== this.previousBillToCustomer
      ) {
        this.order.BillToContactPerson = null;
      }
      this.previousBillToCustomer = this.order.BillToCustomer;

      if (
        this.order.BillToCustomer != null &&
        this.order.ShipToCustomer == null
      ) {
        this.order.ShipToCustomer = this.order.BillToCustomer;
        this.updateShipToCustomer(this.order.ShipToCustomer);
      }

      const partyContactMechanisms: PartyContactMechanism[] =
        loaded.collection<PartyContactMechanism>(
          m.Party.CurrentPartyContactMechanisms
        );
      this.billToContactMechanisms = partyContactMechanisms?.map(
        (v: PartyContactMechanism) => v.ContactMechanism
      );
      this.billToContacts = loaded.collection<Person>(m.Party.CurrentContacts);

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

    this.allors.context.pull(pulls).subscribe((loaded) => {
      if (
        this.previousBillToEndCustomer &&
        this.order.BillToEndCustomer !== this.previousBillToEndCustomer
      ) {
        this.order.BillToEndCustomerContactPerson = null;
      }
      this.previousBillToEndCustomer = this.order.BillToEndCustomer;

      if (
        this.order.BillToEndCustomer != null &&
        this.order.ShipToEndCustomer == null
      ) {
        this.order.ShipToEndCustomer = this.order.BillToEndCustomer;
        this.updateShipToEndCustomer(this.order.ShipToEndCustomer);
      }

      const partyContactMechanisms: PartyContactMechanism[] =
        loaded.collection<PartyContactMechanism>(
          m.Party.CurrentPartyContactMechanisms
        );
      this.billToEndCustomerContactMechanisms = partyContactMechanisms?.map(
        (v: PartyContactMechanism) => v.ContactMechanism
      );
      this.billToEndCustomerContacts = loaded.collection<Person>(
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

    this.allors.context.pull(pulls).subscribe((loaded) => {
      if (
        this.previousShipToEndCustomer &&
        this.order.ShipToEndCustomer !== this.previousShipToEndCustomer
      ) {
        this.order.ShipToEndCustomerContactPerson = null;
      }

      this.previousShipToEndCustomer = this.order.ShipToEndCustomer;

      if (
        this.order.ShipToEndCustomer != null &&
        this.order.BillToEndCustomer == null
      ) {
        this.order.BillToEndCustomer = this.order.ShipToEndCustomer;
        this.updateBillToEndCustomer(this.order.BillToEndCustomer);
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

      this.setDerivedInitialRoles();
    });
  }

  private setDerivedInitialRoles() {
    this.currencyInitialRole =
      this.order.BillToCustomer?.PreferredCurrency ??
      this.order.BillToCustomer?.Locale?.Country?.Currency ??
      this.order.TakenBy?.PreferredCurrency;
    this.takenByContactMechanismInitialRole =
      this.order.TakenBy?.OrderAddress ??
      this.order.TakenBy?.BillingAddress ??
      this.order.TakenBy?.GeneralCorrespondence;
    this.billToContactMechanismInitialRole =
      this.order.BillToCustomer?.BillingAddress ??
      this.order.BillToCustomer?.ShippingAddress ??
      this.order.BillToCustomer?.GeneralCorrespondence;
    this.billToEndCustomerContactMechanismInitialRole =
      this.order.BillToEndCustomer?.BillingAddress ??
      this.order.BillToEndCustomer?.ShippingAddress ??
      this.order.BillToEndCustomer?.GeneralCorrespondence;
    this.shipToEndCustomerAddressInitialRole =
      this.order.ShipToEndCustomer?.ShippingAddress ??
      this.order.ShipToEndCustomer?.GeneralCorrespondence;
    this.shipFromAddressInitialRole = this.order.TakenBy?.ShippingAddress;
    this.shipToAddressInitialRole = this.order.ShipToCustomer?.ShippingAddress;
  }
}
