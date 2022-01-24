import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { M } from '@allors/default/workspace/meta';
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
  SalesInvoice,
  VatRegime,
  IrpfRegime,
  SalesInvoiceType,
  CustomerRelationship,
} from '@allors/default/workspace/domain';
import {
  ObjectData,
  RefreshService,
  SaveService,
  SearchFactory,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { IObject } from '@allors/system/workspace/domain';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { Filters } from '../../../filters/filters';

@Component({
  templateUrl: './salesinvoice-create.component.html',
  providers: [ContextService],
})
export class SalesInvoiceCreateComponent implements OnInit, OnDestroy {
  readonly m: M;
  public title = 'Add Sales Invoice';

  invoice: SalesInvoice;
  billToContactMechanisms: ContactMechanism[] = [];
  billToContacts: Person[] = [];
  billToEndCustomerContactMechanisms: ContactMechanism[] = [];
  billToEndCustomerContacts: Person[] = [];
  shipToAddresses: ContactMechanism[] = [];
  shipToContacts: Person[] = [];
  shipToEndCustomerAddresses: ContactMechanism[] = [];
  shipToEndCustomerContacts: Person[] = [];
  internalOrganisation: InternalOrganisation;
  currencies: Currency[];
  vatRegimes: VatRegime[];
  irpfRegimes: IrpfRegime[];

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
  salesInvoiceTypes: SalesInvoiceType[];

  customersFilter: SearchFactory;
  currencyInitialRole: Currency;
  billToContactMechanismInitialRole: ContactMechanism;
  billToEndCustomerContactMechanismInitialRole: ContactMechanism;
  shipToEndCustomerAddressInitialRole: ContactMechanism;
  shipToAddressInitialRole: PostalAddress;
  showIrpf: boolean;

  get billToCustomerIsPerson(): boolean {
    return (
      !this.invoice.BillToCustomer ||
      this.invoice.BillToCustomer.strategy.cls === this.m.Person
    );
  }

  get shipToCustomerIsPerson(): boolean {
    return (
      !this.invoice.ShipToCustomer ||
      this.invoice.ShipToCustomer.strategy.cls === this.m.Person
    );
  }

  get billToEndCustomerIsPerson(): boolean {
    return (
      !this.invoice.BillToEndCustomer ||
      this.invoice.BillToEndCustomer.strategy.cls === this.m.Person
    );
  }

  get shipToEndCustomerIsPerson(): boolean {
    return (
      !this.invoice.ShipToEndCustomer ||
      this.invoice.ShipToEndCustomer.strategy.cls === this.m.Person
    );
  }

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<SalesInvoiceCreateComponent>,
    private saveService: SaveService,
    public refreshService: RefreshService,
    public internalOrganisationId: InternalOrganisationId,
    private fetcher: FetcherService
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;

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
            pull.SalesInvoiceType({
              predicate: {
                kind: 'Equals',
                propertyType: m.SalesInvoiceType.IsActive,
                value: true,
              },
              sorting: [{ roleType: m.SalesInvoiceType.Name }],
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
        this.salesInvoiceTypes = loaded.collection<SalesInvoiceType>(
          m.SalesInvoiceType
        );
        this.currencies = loaded.collection<Currency>(m.Currency);
        this.irpfRegimes = loaded.collection<IrpfRegime>(m.IrpfRegime);

        this.invoice = this.allors.context.create<SalesInvoice>(m.SalesInvoice);
        this.invoice.BilledFrom = this.internalOrganisation;
        this.invoice.AdvancePayment = '0';

        if (this.invoice.BillToCustomer) {
          this.updateBillToCustomer(this.invoice.BillToCustomer);
        }

        if (this.invoice.BillToEndCustomer) {
          this.updateBillToEndCustomer(this.invoice.BillToEndCustomer);
        }

        if (this.invoice.ShipToCustomer) {
          this.updateShipToCustomer(this.invoice.ShipToCustomer);
        }

        if (this.invoice.ShipToEndCustomer) {
          this.updateShipToEndCustomer(this.invoice.ShipToEndCustomer);
        }

        this.previousShipToCustomer = this.invoice.ShipToCustomer;
        this.previousShipToEndCustomer = this.invoice.ShipToEndCustomer;
        this.previousBillToCustomer = this.invoice.BillToCustomer;
        this.previousBillToEndCustomer = this.invoice.BillToEndCustomer;
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.allors.context.push().subscribe(() => {
      this.dialogRef.close(this.invoice);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }

  public shipToCustomerAdded(party: Party): void {
    const customerRelationship =
      this.allors.context.create<CustomerRelationship>(
        this.m.CustomerRelationship
      );
    customerRelationship.Customer = party;
    customerRelationship.InternalOrganisation = this.internalOrganisation;

    this.invoice.ShipToCustomer = party;
  }

  public billToCustomerAdded(party: Party): void {
    const customerRelationship =
      this.allors.context.create<CustomerRelationship>(
        this.m.CustomerRelationship
      );
    customerRelationship.Customer = party;
    customerRelationship.InternalOrganisation = this.internalOrganisation;

    this.invoice.BillToCustomer = party;
  }

  public shipToEndCustomerAdded(party: Party): void {
    const customerRelationship =
      this.allors.context.create<CustomerRelationship>(
        this.m.CustomerRelationship
      );
    customerRelationship.Customer = party;
    customerRelationship.InternalOrganisation = this.internalOrganisation;

    this.invoice.ShipToEndCustomer = party;
  }

  public billToEndCustomerAdded(party: Party): void {
    const customerRelationship =
      this.allors.context.create<CustomerRelationship>(
        this.m.CustomerRelationship
      );
    customerRelationship.Customer = party;
    customerRelationship.InternalOrganisation = this.internalOrganisation;

    this.invoice.BillToEndCustomer = party;
  }

  public billToContactPersonAdded(person: Person): void {
    const organisationContactRelationship =
      this.allors.context.create<OrganisationContactRelationship>(
        this.m.OrganisationContactRelationship
      );
    organisationContactRelationship.Organisation = this.invoice
      .BillToCustomer as Organisation;
    organisationContactRelationship.Contact = person;

    this.billToContacts.push(person);
    this.invoice.BillToContactPerson = person;
  }

  public billToEndCustomerContactPersonAdded(person: Person): void {
    const organisationContactRelationship =
      this.allors.context.create<OrganisationContactRelationship>(
        this.m.OrganisationContactRelationship
      );
    organisationContactRelationship.Organisation = this.invoice
      .BillToEndCustomer as Organisation;
    organisationContactRelationship.Contact = person;

    this.billToEndCustomerContacts.push(person);
    this.invoice.BillToEndCustomerContactPerson = person;
  }

  public shipToContactPersonAdded(person: Person): void {
    const organisationContactRelationship =
      this.allors.context.create<OrganisationContactRelationship>(
        this.m.OrganisationContactRelationship
      );
    organisationContactRelationship.Organisation = this.invoice
      .ShipToCustomer as Organisation;
    organisationContactRelationship.Contact = person;

    this.shipToContacts.push(person);
    this.invoice.ShipToContactPerson = person;
  }

  public shipToEndCustomerContactPersonAdded(person: Person): void {
    const organisationContactRelationship =
      this.allors.context.create<OrganisationContactRelationship>(
        this.m.OrganisationContactRelationship
      );
    organisationContactRelationship.Organisation = this.invoice
      .ShipToEndCustomer as Organisation;
    organisationContactRelationship.Contact = person;

    this.shipToEndCustomerContacts.push(person);
    this.invoice.ShipToEndCustomerContactPerson = person;
  }

  public billToContactMechanismAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.billToContactMechanisms.push(partyContactMechanism.ContactMechanism);
    this.invoice.BillToCustomer.addPartyContactMechanism(partyContactMechanism);
    this.invoice.AssignedBillToContactMechanism =
      partyContactMechanism.ContactMechanism;
  }

  public billToEndCustomerContactMechanismAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.billToEndCustomerContactMechanisms.push(
      partyContactMechanism.ContactMechanism
    );
    this.invoice.BillToEndCustomer.addPartyContactMechanism(
      partyContactMechanism
    );
    this.invoice.AssignedBillToEndCustomerContactMechanism =
      partyContactMechanism.ContactMechanism;
  }

  public shipToAddressAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.shipToAddresses.push(partyContactMechanism.ContactMechanism);
    this.invoice.ShipToCustomer.addPartyContactMechanism(partyContactMechanism);
    this.invoice.AssignedShipToAddress =
      partyContactMechanism.ContactMechanism as PostalAddress;
  }

  public shipToEndCustomerAddressAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.shipToEndCustomerAddresses.push(
      partyContactMechanism.ContactMechanism
    );
    this.invoice.ShipToEndCustomer.addPartyContactMechanism(
      partyContactMechanism
    );
    this.invoice.AssignedShipToEndCustomerAddress =
      partyContactMechanism.ContactMechanism as PostalAddress;
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
        this.invoice.ShipToCustomer !== this.previousShipToCustomer
      ) {
        this.invoice.ShipToContactPerson = null;
      }

      this.previousShipToCustomer = this.invoice.ShipToCustomer;

      if (
        this.invoice.ShipToCustomer != null &&
        this.invoice.BillToCustomer == null
      ) {
        this.invoice.BillToCustomer = this.invoice.ShipToCustomer;
        this.updateBillToCustomer(this.invoice.ShipToCustomer);
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
        this.invoice.BillToCustomer !== this.previousBillToCustomer
      ) {
        this.invoice.BillToContactPerson = null;
      }

      this.previousBillToCustomer = this.invoice.BillToCustomer;

      if (
        this.invoice.BillToCustomer != null &&
        this.invoice.ShipToCustomer == null
      ) {
        this.invoice.ShipToCustomer = this.invoice.BillToCustomer;
        this.updateShipToCustomer(this.invoice.ShipToCustomer);
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
        this.invoice.BillToEndCustomer !== this.previousBillToEndCustomer
      ) {
        this.invoice.BillToEndCustomerContactPerson = null;
      }
      this.previousBillToEndCustomer = this.invoice.BillToEndCustomer;

      if (
        this.invoice.BillToEndCustomer != null &&
        this.invoice.ShipToEndCustomer == null
      ) {
        this.invoice.ShipToEndCustomer = this.invoice.BillToEndCustomer;
        this.updateShipToEndCustomer(this.invoice.ShipToEndCustomer);
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
        this.invoice.ShipToEndCustomer !== this.previousShipToEndCustomer
      ) {
        this.invoice.ShipToEndCustomerContactPerson = null;
      }

      this.previousShipToEndCustomer = this.invoice.ShipToEndCustomer;

      if (
        this.invoice.ShipToEndCustomer != null &&
        this.invoice.BillToEndCustomer == null
      ) {
        this.invoice.BillToEndCustomer = this.invoice.ShipToEndCustomer;
        this.updateBillToEndCustomer(this.invoice.BillToEndCustomer);
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
      this.invoice.BillToCustomer?.PreferredCurrency ??
      this.invoice.BillToCustomer?.Locale?.Country?.Currency ??
      this.invoice.BilledFrom?.PreferredCurrency;
    this.billToContactMechanismInitialRole =
      this.invoice.BillToCustomer?.BillingAddress ??
      this.invoice.BillToCustomer?.ShippingAddress ??
      this.invoice.BillToCustomer?.GeneralCorrespondence;
    this.billToEndCustomerContactMechanismInitialRole =
      this.invoice.BillToEndCustomer?.BillingAddress ??
      this.invoice.BillToEndCustomer?.ShippingAddress ??
      this.invoice.BillToEndCustomer?.GeneralCorrespondence;
    this.shipToEndCustomerAddressInitialRole =
      this.invoice.ShipToEndCustomer?.ShippingAddress ??
      this.invoice.ShipToEndCustomer?.GeneralCorrespondence;
    this.shipToAddressInitialRole =
      this.invoice.ShipToCustomer?.ShippingAddress;
  }
}
