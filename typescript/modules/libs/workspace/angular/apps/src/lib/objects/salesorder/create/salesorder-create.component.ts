import { Component, OnDestroy, OnInit, Self, Optional, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import {
  Person,
  Organisation,
  OrganisationContactRelationship,
  Party,
  InternalOrganisation,
  ContactMechanism,
  PartyContactMechanism,
  PostalAddress,
  Currency,
  SalesOrder,
  VatRegime,
  IrpfRegime,
  Store,
} from '@allors/workspace/domain/default';
import { ObjectData, RefreshService, SaveService, SearchFactory, TestScope } from '@allors/workspace/angular/base';
import { SessionService } from '@allors/workspace/angular/core';
import { IObject } from '@allors/workspace/domain/system';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { FetcherService } from '../../../services/fetcher/fetcher-service';

@Component({
  templateUrl: './salesorder-create.component.html',
  providers: [SessionService],
})
export class SalesOrderCreateComponent extends TestScope implements OnInit, OnDestroy {
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
  internalOrganisation: Organisation;
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
    return !this.order.BillToCustomer || this.order.BillToCustomer.objectType.name === this.m.Person.name;
  }

  get shipToCustomerIsPerson(): boolean {
    return !this.order.ShipToCustomer || this.order.ShipToCustomer.objectType.name === this.m.Person.name;
  }

  get billToEndCustomerIsPerson(): boolean {
    return !this.order.BillToEndCustomer || this.order.BillToEndCustomer.objectType.name === this.m.Person.name;
  }

  get shipToEndCustomerIsPerson(): boolean {
    return !this.order.ShipToEndCustomer || this.order.ShipToEndCustomer.objectType.name === this.m.Person.name;
  }

  constructor(
    @Self() public allors: SessionService,
    @Optional() @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<SalesOrderCreateComponent>,

    private refreshService: RefreshService,
    private saveService: SaveService,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super();

    this.m = this.allors.workspace.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.allors.workspace.configuration.metaPopulation as M;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest([this.refreshService.refresh$, this.internalOrganisationId.observable$])
      .pipe(
        switchMap(([, internalOrganisationId]) => {
          const pulls = [
            this.fetcher.internalOrganisation,
            pull.Currency({
              predicate: { kind: 'Equals', propertyType: m.Currency.IsActive, value: true },
              sorting: [{ roleType: m.Currency.IsoCode }],
            }),
            pull.IrpfRegime({ sorting: [{ roleType: m.IrpfRegime.Name }] }),
            pull.Store({
              predicate: { kind: 'Equals', propertyType: m.Store.InternalOrganisation, object: internalOrganisationId },
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

          this.customersFilter = Filters.customersFilter(m, internalOrganisationId);

          return this.allors.client.pullReactive(this.allors.session, pulls);
        })
      )
      .subscribe((loaded) => {
        this.internalOrganisation = loaded.object<InternalOrganisation>(m.InternalOrganisation);
        this.showIrpf = this.internalOrganisation.Country.IsoCode === 'ES';
        this.vatRegimes = this.internalOrganisation.Country.DerivedVatRegimes;
        this.stores = loaded.collection<Store>(m.Store);
        this.currencies = loaded.collection<Currency>(m.Currency);
        this.irpfRegimes = loaded.collection<IrpfRegime>(m.IrpfRegime);

        this.order = this.allors.session.create<SalesOrder>(m.SalesOrder);
        this.order.TakenBy = this.internalOrganisation;

        const partyContactMechanisms: PartyContactMechanism[] = loaded.collection<PartyContactMechanism>(m.PartyContactMechanism);
        this.shipFromAddresses = partyContactMechanisms.filter((v: PartyContactMechanism) => v.ContactMechanism.objectType.name === 'PostalAddress').map((v: PartyContactMechanism) => v.ContactMechanism);

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

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.allors.context.save().subscribe(() => {
      const data: IObject = {
        id: this.order.id,
        objectType: this.order.objectType,
      };

      this.dialogRef.close(data);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }

  public shipToCustomerAdded(party: Party): void {
    const customerRelationship = this.allors.session.create<CustomerRelationship>(m.CustomerRelationship);
    customerRelationship.Customer = party;
    customerRelationship.InternalOrganisation = this.internalOrganisation;

    this.order.ShipToCustomer = party;
  }

  public billToCustomerAdded(party: Party): void {
    const customerRelationship = this.allors.session.create<CustomerRelationship>(m.CustomerRelationship);
    customerRelationship.Customer = party;
    customerRelationship.InternalOrganisation = this.internalOrganisation;

    this.order.BillToCustomer = party;
  }

  public shipToEndCustomerAdded(party: Party): void {
    const customerRelationship = this.allors.session.create<CustomerRelationship>(m.CustomerRelationship);
    customerRelationship.Customer = party;
    customerRelationship.InternalOrganisation = this.internalOrganisation;

    this.order.ShipToEndCustomer = party;
  }

  public billToEndCustomerAdded(party: Party): void {
    const customerRelationship = this.allors.session.create<CustomerRelationship>(m.CustomerRelationship);
    customerRelationship.Customer = party;
    customerRelationship.InternalOrganisation = this.internalOrganisation;

    this.order.BillToEndCustomer = party;
  }

  public billToContactPersonAdded(person: Person): void {
    const organisationContactRelationship = this.allors.session.create<OrganisationContactRelationship>(m.OrganisationContactRelationship);
    organisationContactRelationship.Organisation = this.order.BillToCustomer as Organisation;
    organisationContactRelationship.Contact = person;

    this.billToContacts.push(person);
    this.order.BillToContactPerson = person;
  }

  public billToEndCustomerContactPersonAdded(person: Person): void {
    const organisationContactRelationship = this.allors.session.create<OrganisationContactRelationship>(m.OrganisationContactRelationship);
    organisationContactRelationship.Organisation = this.order.BillToEndCustomer as Organisation;
    organisationContactRelationship.Contact = person;

    this.billToEndCustomerContacts.push(person);
    this.order.BillToEndCustomerContactPerson = person;
  }

  public shipToContactPersonAdded(person: Person): void {
    const organisationContactRelationship = this.allors.session.create<OrganisationContactRelationship>(m.OrganisationContactRelationship);
    organisationContactRelationship.Organisation = this.order.ShipToCustomer as Organisation;
    organisationContactRelationship.Contact = person;

    this.shipToContacts.push(person);
    this.order.ShipToContactPerson = person;
  }

  public shipToEndCustomerContactPersonAdded(person: Person): void {
    const organisationContactRelationship = this.allors.session.create<OrganisationContactRelationship>(m.OrganisationContactRelationship);
    organisationContactRelationship.Organisation = this.order.ShipToEndCustomer as Organisation;
    organisationContactRelationship.Contact = person;

    this.shipToEndCustomerContacts.push(person);
    this.order.ShipToEndCustomerContactPerson = person;
  }

  public billToContactMechanismAdded(partyContactMechanism: PartyContactMechanism): void {
    this.billToContactMechanisms.push(partyContactMechanism.ContactMechanism);
    this.order.BillToCustomer.addPartyContactMechanism(partyContactMechanism);
    this.order.AssignedBillToContactMechanism = partyContactMechanism.ContactMechanism;
  }

  public billToEndCustomerContactMechanismAdded(partyContactMechanism: PartyContactMechanism): void {
    this.billToEndCustomerContactMechanisms.push(partyContactMechanism.ContactMechanism);
    this.order.BillToEndCustomer.addPartyContactMechanism(partyContactMechanism);
    this.order.AssignedBillToEndCustomerContactMechanism = partyContactMechanism.ContactMechanism;
  }

  public shipToAddressAdded(partyContactMechanism: PartyContactMechanism): void {
    this.shipToAddresses.push(partyContactMechanism.ContactMechanism);
    this.order.ShipToCustomer.addPartyContactMechanism(partyContactMechanism);
    this.order.AssignedShipToAddress = partyContactMechanism.ContactMechanism as PostalAddress;
  }

  public shipToEndCustomerAddressAdded(partyContactMechanism: PartyContactMechanism): void {
    this.shipToEndCustomerAddresses.push(partyContactMechanism.ContactMechanism);
    this.order.ShipToEndCustomer.addPartyContactMechanism(partyContactMechanism);
    this.order.AssignedShipToEndCustomerAddress = partyContactMechanism.ContactMechanism as PostalAddress;
  }

  public shipFromAddressAdded(partyContactMechanism: PartyContactMechanism): void {
    this.shipFromAddresses.push(partyContactMechanism.ContactMechanism);
    this.order.TakenBy.addPartyContactMechanism(partyContactMechanism);
    this.order.AssignedShipFromAddress = partyContactMechanism.ContactMechanism as PostalAddress;
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

    this.allors.context.load(new PullRequest({ pulls })).subscribe((loaded) => {
      if (this.previousShipToCustomer && this.order.ShipToCustomer !== this.previousShipToCustomer) {
        this.order.ShipToContactPerson = null;
      }
      this.previousShipToCustomer = this.order.ShipToCustomer;

      if (this.order.ShipToCustomer !== null && this.order.BillToCustomer === null) {
        this.order.BillToCustomer = this.order.ShipToCustomer;
        this.updateBillToCustomer(this.order.ShipToCustomer);
      }

      const partyContactMechanisms: PartyContactMechanism[] = loaded.collection<PartyContactMechanism>(m.PartyContactMechanism);
      this.shipToAddresses = partyContactMechanisms.filter((v: PartyContactMechanism) => v.ContactMechanism.objectType.name === 'PostalAddress').map((v: PartyContactMechanism) => v.ContactMechanism);
      this.shipToContacts = loaded.collection<Person>(m.Person);

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

    this.allors.context.load(new PullRequest({ pulls })).subscribe((loaded) => {
      if (this.previousBillToCustomer && this.order.BillToCustomer !== this.previousBillToCustomer) {
        this.order.BillToContactPerson = null;
      }
      this.previousBillToCustomer = this.order.BillToCustomer;

      if (this.order.BillToCustomer !== null && this.order.ShipToCustomer === null) {
        this.order.ShipToCustomer = this.order.BillToCustomer;
        this.updateShipToCustomer(this.order.ShipToCustomer);
      }

      const partyContactMechanisms: PartyContactMechanism[] = loaded.collection<PartyContactMechanism>(m.PartyContactMechanism);
      this.billToContactMechanisms = partyContactMechanisms.map((v: PartyContactMechanism) => v.ContactMechanism);
      this.billToContacts = loaded.collection<Person>(m.Person);

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

    this.allors.context.load(new PullRequest({ pulls })).subscribe((loaded) => {
      if (this.previousBillToEndCustomer && this.order.BillToEndCustomer !== this.previousBillToEndCustomer) {
        this.order.BillToEndCustomerContactPerson = null;
      }
      this.previousBillToEndCustomer = this.order.BillToEndCustomer;

      if (this.order.BillToEndCustomer !== null && this.order.ShipToEndCustomer === null) {
        this.order.ShipToEndCustomer = this.order.BillToEndCustomer;
        this.updateShipToEndCustomer(this.order.ShipToEndCustomer);
      }

      const partyContactMechanisms: PartyContactMechanism[] = loaded.collection<PartyContactMechanism>(m.PartyContactMechanism);
      this.billToEndCustomerContactMechanisms = partyContactMechanisms.map((v: PartyContactMechanism) => v.ContactMechanism);
      this.billToEndCustomerContacts = loaded.collection<Person>(m.Person);

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

    this.allors.context.load(new PullRequest({ pulls })).subscribe((loaded) => {
      if (this.previousShipToEndCustomer && this.order.ShipToEndCustomer !== this.previousShipToEndCustomer) {
        this.order.ShipToEndCustomerContactPerson = null;
      }

      this.previousShipToEndCustomer = this.order.ShipToEndCustomer;

      if (this.order.ShipToEndCustomer !== null && this.order.BillToEndCustomer === null) {
        this.order.BillToEndCustomer = this.order.ShipToEndCustomer;
        this.updateBillToEndCustomer(this.order.BillToEndCustomer);
      }

      const partyContactMechanisms: PartyContactMechanism[] = loaded.collection<PartyContactMechanism>(m.PartyContactMechanism);
      this.shipToEndCustomerAddresses = partyContactMechanisms.filter((v: PartyContactMechanism) => v.ContactMechanism.objectType.name === 'PostalAddress').map((v: PartyContactMechanism) => v.ContactMechanism);
      this.shipToEndCustomerContacts = loaded.collection<Person>(m.Person);

      this.setDerivedInitialRoles();
    });
  }

  private setDerivedInitialRoles() {
    this.currencyInitialRole = this.order.BillToCustomer?.PreferredCurrency ?? this.order.BillToCustomer?.Locale?.Country?.Currency ?? this.order.TakenBy?.PreferredCurrency;
    this.takenByContactMechanismInitialRole = this.order.TakenBy?.OrderAddress ?? this.order.TakenBy?.BillingAddress ?? this.order.TakenBy?.GeneralCorrespondence;
    this.billToContactMechanismInitialRole = this.order.BillToCustomer?.BillingAddress ?? this.order.BillToCustomer?.ShippingAddress ?? this.order.BillToCustomer?.GeneralCorrespondence;
    this.billToEndCustomerContactMechanismInitialRole = this.order.BillToEndCustomer?.BillingAddress ?? this.order.BillToEndCustomer?.ShippingAddress ?? this.order.BillToEndCustomer?.GeneralCorrespondence;
    this.shipToEndCustomerAddressInitialRole = this.order.ShipToEndCustomer?.ShippingAddress ?? this.order.ShipToEndCustomer?.GeneralCorrespondence;
    this.shipFromAddressInitialRole = this.order.TakenBy?.ShippingAddress;
    this.shipToAddressInitialRole = this.order.ShipToCustomer?.ShippingAddress;
  }
}
