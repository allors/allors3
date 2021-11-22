import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
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
  SupplierRelationship,
  ContactMechanism,
  PartyContactMechanism,
  PostalAddress,
  Currency,
  VatRegime,
  IrpfRegime,
  PurchaseInvoice,
  PurchaseInvoiceType,
  CustomerRelationship,
} from '@allors/workspace/domain/default';
import { ObjectData, RefreshService, SaveService, SearchFactory, TestScope } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';
import { IObject } from '@allors/workspace/domain/system';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { Filters } from '../../../filters/filters';

@Component({
  templateUrl: './purchaseinvoice-create.component.html',
  providers: [ContextService],
})
export class PurchaseInvoiceCreateComponent extends TestScope implements OnInit, OnDestroy {
  public m: M;

  title = 'Add Purchase Invoice';

  invoice: PurchaseInvoice;
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

  private subscription: Subscription;

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
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<PurchaseInvoiceCreateComponent>,

    public refreshService: RefreshService,
    private saveService: SaveService,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super();

    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;

    this.subscription = combineLatest([this.refreshService.refresh$, this.internalOrganisationId.observable$])
      .pipe(
        switchMap(() => {
          const pulls = [
            this.fetcher.internalOrganisation,
            pull.IrpfRegime({ sorting: [{ roleType: m.IrpfRegime.Name }] }),
            pull.Currency({
              predicate: { kind: 'Equals', propertyType: m.Currency.IsActive, value: true },
              sorting: [{ roleType: m.Currency.IsoCode }],
            }),
            pull.PurchaseInvoiceType({
              predicate: { kind: 'Equals', propertyType: m.PurchaseInvoiceType.IsActive, value: true },
              sorting: [{ roleType: m.PurchaseInvoiceType.Name }],
            }),
          ];

          this.customersFilter = Filters.customersFilter(m, this.internalOrganisationId.value);
          this.employeeFilter = Filters.employeeFilter(m, this.internalOrganisationId.value);
          this.suppliersFilter = Filters.suppliersFilter(m, this.internalOrganisationId.value);

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.internalOrganisation = this.fetcher.getInternalOrganisation(loaded);
        this.showIrpf = this.internalOrganisation.Country.IsoCode === 'ES';
        this.vatRegimes = this.internalOrganisation.Country.DerivedVatRegimes;
        this.irpfRegimes = loaded.collection<IrpfRegime>(m.IrpfRegime);
        this.currencies = loaded.collection<Currency>(m.Currency);
        this.purchaseInvoiceTypes = loaded.collection<PurchaseInvoiceType>(m.PurchaseInvoiceType);

        this.invoice = loaded.object<PurchaseInvoice>(m.PurchaseInvoice);

        this.invoice = this.allors.context.create<PurchaseInvoice>(m.PurchaseInvoice);
        this.invoice.BilledTo = this.internalOrganisation;

        if (this.invoice.BilledFrom) {
          this.updateBilledFrom(this.invoice.BilledFrom);
        }

        if (this.invoice.ShipToCustomer) {
          this.updateShipToCustomer(this.invoice.ShipToCustomer);
        }

        if (this.invoice.BillToEndCustomer) {
          this.updateBillToEndCustomer(this.invoice.BillToEndCustomer);
        }

        if (this.invoice.ShipToEndCustomer) {
          this.updateShipToEndCustomer(this.invoice.ShipToEndCustomer);
        }

        this.previousBilledFrom = this.invoice.BilledFrom;
        this.previousShipToCustomer = this.invoice.ShipToCustomer;
        this.previousBillToEndCustomer = this.invoice.BillToEndCustomer;
        this.previousShipToEndCustomer = this.invoice.ShipToEndCustomer;

        this.currencyInitialRole = this.internalOrganisation.PreferredCurrency;
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

  public billedFromAdded(organisation: Organisation): void {
    const supplierRelationship = this.allors.context.create<SupplierRelationship>(this.m.SupplierRelationship);
    supplierRelationship.Supplier = organisation;
    supplierRelationship.InternalOrganisation = this.internalOrganisation;

    this.invoice.BilledFrom = organisation;
    this.billedFromSelected(organisation);
  }

  public shipToCustomerAdded(party: Party): void {
    const customerRelationship = this.allors.context.create<CustomerRelationship>(this.m.CustomerRelationship);
    customerRelationship.Customer = party;
    customerRelationship.InternalOrganisation = this.internalOrganisation;

    this.invoice.ShipToCustomer = party;
  }

  public billToEndCustomerAdded(party: Party): void {
    const customerRelationship = this.allors.context.create<CustomerRelationship>(this.m.CustomerRelationship);
    customerRelationship.Customer = party;
    customerRelationship.InternalOrganisation = this.internalOrganisation;

    this.invoice.BillToEndCustomer = party;
  }

  public shipToEndCustomerAdded(party: Party): void {
    const customerRelationship = this.allors.context.create<CustomerRelationship>(this.m.CustomerRelationship);
    customerRelationship.Customer = party;
    customerRelationship.InternalOrganisation = this.internalOrganisation;

    this.invoice.ShipToEndCustomer = party;
  }

  public billedFromContactPersonAdded(person: Person): void {
    const organisationContactRelationship = this.allors.context.create<OrganisationContactRelationship>(this.m.OrganisationContactRelationship);
    organisationContactRelationship.Organisation = this.invoice.BilledFrom as Organisation;
    organisationContactRelationship.Contact = person;

    this.billedFromContacts.push(person);
    this.invoice.BilledFromContactPerson = person;
  }

  public shipToCustomerContactPersonAdded(person: Person): void {
    const organisationContactRelationship = this.allors.context.create<OrganisationContactRelationship>(this.m.OrganisationContactRelationship);
    organisationContactRelationship.Organisation = this.invoice.BilledFrom as Organisation;
    organisationContactRelationship.Contact = person;

    this.shipToCustomerContacts.push(person);
    this.invoice.ShipToCustomerContactPerson = person;
  }

  public billToEndCustomerContactPersonAdded(person: Person): void {
    const organisationContactRelationship = this.allors.context.create<OrganisationContactRelationship>(this.m.OrganisationContactRelationship);
    organisationContactRelationship.Organisation = this.invoice.ShipToEndCustomer as Organisation;
    organisationContactRelationship.Contact = person;

    this.billToEndCustomerContacts.push(person);
    this.invoice.BillToEndCustomerContactPerson = person;
  }

  public shipToEndCustomerContactPersonAdded(person: Person): void {
    const organisationContactRelationship = this.allors.context.create<OrganisationContactRelationship>(this.m.OrganisationContactRelationship);
    organisationContactRelationship.Organisation = this.invoice.ShipToEndCustomer as Organisation;
    organisationContactRelationship.Contact = person;

    this.shipToEndCustomerContacts.push(person);
    this.invoice.ShipToEndCustomerContactPerson = person;
  }

  public billedFromContactMechanismAdded(partyContactMechanism: PartyContactMechanism): void {
    this.billedFromContactMechanisms.push(partyContactMechanism.ContactMechanism);
    this.invoice.BilledFrom.addPartyContactMechanism(partyContactMechanism);
    this.invoice.AssignedBilledFromContactMechanism = partyContactMechanism.ContactMechanism;
  }

  public shipToCustomerAddressAdded(partyContactMechanism: PartyContactMechanism): void {
    this.shipToCustomerAddresses.push(partyContactMechanism.ContactMechanism);
    this.invoice.ShipToCustomer.addPartyContactMechanism(partyContactMechanism);
    this.invoice.AssignedShipToCustomerAddress = partyContactMechanism.ContactMechanism as PostalAddress;
  }

  public billToEndCustomerContactMechanismAdded(partyContactMechanism: PartyContactMechanism): void {
    this.billToEndCustomerContactMechanisms.push(partyContactMechanism.ContactMechanism);
    this.invoice.BillToEndCustomer.addPartyContactMechanism(partyContactMechanism);
    this.invoice.AssignedBillToEndCustomerContactMechanism = partyContactMechanism.ContactMechanism;
  }

  public shipToEndCustomerAddressAdded(partyContactMechanism: PartyContactMechanism): void {
    this.shipToEndCustomerAddresses.push(partyContactMechanism.ContactMechanism);
    this.invoice.ShipToEndCustomer.addPartyContactMechanism(partyContactMechanism);
    this.invoice.AssignedShipToEndCustomerAddress = partyContactMechanism.ContactMechanism as PostalAddress;
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
      pull.Organisation({
        object: party,
        name: 'selectedSupplier',
        include: {
          OrderAddress: x,
        },
      }),
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
      if (this.invoice.BilledFrom !== this.previousBilledFrom) {
        this.invoice.AssignedBilledFromContactMechanism = null;
        this.invoice.BilledFromContactPerson = null;
        this.previousBilledFrom = this.invoice.BilledFrom;
      }

      const partyContactMechanisms: PartyContactMechanism[] = loaded.collection<PartyContactMechanism>(m.Party.CurrentPartyContactMechanisms);
      this.billedFromContactMechanisms = partyContactMechanisms?.filter((v: PartyContactMechanism) => v.ContactMechanism.strategy.cls === m.PostalAddress)?.map((v: PartyContactMechanism) => v.ContactMechanism);
      this.billedFromContacts = loaded.collection<Person>(m.Party.CurrentContacts);

      const selectedSupplier = loaded.object<Organisation>('selectedSupplier');
      this.billedFromContactMechanismInitialRole = selectedSupplier.OrderAddress;
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
        name: 'selectedParty',
        include: {
          PreferredCurrency: x,
          BillingAddress: x,
          GeneralCorrespondence: x,
        },
      }),
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
      if (this.invoice.ShipToCustomer !== this.previousShipToCustomer) {
        this.invoice.AssignedShipToEndCustomerAddress = null;
        this.invoice.ShipToCustomerContactPerson = null;
        this.previousShipToCustomer = this.invoice.ShipToCustomer;
      }

      const partyContactMechanisms: PartyContactMechanism[] = loaded.collection<PartyContactMechanism>(m.Party.CurrentPartyContactMechanisms);
      this.shipToCustomerAddresses = partyContactMechanisms?.map((v: PartyContactMechanism) => v.ContactMechanism);
      this.shipToCustomerContacts = loaded.collection<Person>(m.Party.CurrentContacts);

      const selectedparty = loaded.object<Party>('selectedParty');
      this.shipToCustomerAddressInitialRole = selectedparty.BillingAddress ?? selectedparty.GeneralCorrespondence;
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
        name: 'selectedParty',
        include: {
          PreferredCurrency: x,
          BillingAddress: x,
          GeneralCorrespondence: x,
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
      this.billToEndCustomerContactMechanisms = partyContactMechanisms?.filter((v: PartyContactMechanism) => v.ContactMechanism.strategy.cls === m.PostalAddress)?.map((v: PartyContactMechanism) => v.ContactMechanism);
      this.billToEndCustomerContacts = loaded.collection<Person>(m.Party.CurrentContacts);

      const selectedparty = loaded.object<Party>('selectedParty');
      this.billToEndCustomerContactMechanismInitialRole = selectedparty.BillingAddress ?? selectedparty.GeneralCorrespondence;
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
        name: 'selectedParty',
        include: {
          PreferredCurrency: x,
          BillingAddress: x,
          GeneralCorrespondence: x,
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

      const selectedparty = loaded.object<Party>('selectedParty');
      this.shipToEndCustomerAddressInitialRole = selectedparty.BillingAddress ?? selectedparty.GeneralCorrespondence;
    });
  }
}
