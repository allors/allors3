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
  PurchaseInvoice,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { FetcherService } from '../../../../services/fetcher/fetcher-service';
import { InternalOrganisationId } from '../../../../services/state/internal-organisation-id';
import { Filters } from '../../../../filters/filters';

@Component({
  selector: 'purchaseinvoice-edit-form',
  templateUrl: './purchaseinvoice-edit-form.component.html',
  providers: [ContextService, OldPanelService],
})
export class PurchaseInvoiceEditFormComponent
  extends AllorsFormComponent<PurchaseInvoice>
  implements CreateOrEditPullHandler, EditIncludeHandler, PostCreatePullHandler
{
  readonly m: M;

  invoice: PurchaseInvoice;

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

  private subscription: Subscription;
  internalOrganisation: InternalOrganisation;

  customersFilter: SearchFactory;
  employeeFilter: SearchFactory;
  suppliersFilter: SearchFactory;
  showIrpf: boolean;

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
    errorService: ErrorService,
    form: NgForm,
    public fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;

    // Normal
    const purchaseInvoicePullName = `${panel.name}_${this.m.PurchaseInvoice.tag}`;

    panel.onPull = (pulls) => {
      const m = this.m;
      const { pullBuilder: pull } = m;
      const x = {};

      const { id } = this.panel.manager;

      pulls.push(
        pull.PurchaseInvoice({
          name: purchaseInvoicePullName,
          objectId: id,
          include: {
            PurchaseInvoiceItems: {
              InvoiceItemType: x,
            },
            BilledFrom: x,
            DerivedBilledFromContactMechanism: {
              PostalAddress_Country: {},
            },
            BilledFromContactPerson: x,
            BillToEndCustomer: x,
            BillToEndCustomerContactPerson: x,
            ShipToEndCustomer: x,
            ShipToEndCustomerContactPerson: x,
            ElectronicDocuments: x,
            PurchaseInvoiceState: x,
            CreatedBy: x,
            LastModifiedBy: x,
            DerivedVatRegime: {
              VatRates: x,
            },
            DerivedBillToEndCustomerContactMechanism: {
              PostalAddress_Country: {},
            },
            DerivedShipToEndCustomerAddress: {
              Country: x,
            },
          },
        })
      );
    };

    panel.onPulled = (loaded) => {
      if (this.panel.isCollapsed) {
        this.invoice = loaded.object<PurchaseInvoice>(purchaseInvoicePullName);
      }
    };
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    // Expanded
    this.subscription = this.panel.manager.on$
      .pipe(
        filter(() => {
          return this.panel.isExpanded;
        }),
        switchMap(() => {
          this.invoice = undefined;

          const id = this.panel.manager.id;

          const pulls = [
            this.fetcher.internalOrganisation,
            pull.PurchaseInvoice({
              objectId: id,
              include: {
                BilledFrom: x,
                BilledFromContactPerson: x,
                DerivedBilledFromContactMechanism: x,
                ShipToCustomer: x,
                BillToEndCustomer: x,
                DerivedBillToEndCustomerContactMechanism: x,
                BillToEndCustomerContactPerson: x,
                ShipToEndCustomer: x,
                DerivedShipToEndCustomerAddress: x,
                ShipToEndCustomerContactPerson: x,
                PurchaseInvoiceState: x,
                AssignedVatRegime: x,
                DerivedVatRegime: x,
              },
            }),
            pull.IrpfRegime({ sorting: [{ roleType: m.IrpfRegime.Name }] }),
            pull.Currency({
              predicate: {
                kind: 'Equals',
                propertyType: m.Currency.IsActive,
                value: true,
              },
              sorting: [{ roleType: m.Currency.IsoCode }],
            }),
            pull.PurchaseInvoiceType({
              predicate: {
                kind: 'Equals',
                propertyType: m.PurchaseInvoiceType.IsActive,
                value: true,
              },
              sorting: [{ roleType: m.PurchaseInvoiceType.Name }],
            }),
          ];

          this.customersFilter = Filters.customersFilter(
            m,
            this.internalOrganisationId.value
          );
          this.employeeFilter = Filters.employeeFilter(
            m,
            this.internalOrganisationId.value
          );
          this.suppliersFilter = Filters.suppliersFilter(
            m,
            this.internalOrganisationId.value
          );

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        this.internalOrganisation =
          this.fetcher.getInternalOrganisation(loaded);
        this.showIrpf = this.internalOrganisation.Country.IsoCode === 'ES';
        this.vatRegimes = this.internalOrganisation.Country.DerivedVatRegimes;
        this.irpfRegimes = loaded.collection<IrpfRegime>(m.IrpfRegime);
        this.currencies = loaded.collection<Currency>(m.Currency);
        this.purchaseInvoiceTypes = loaded.collection<PurchaseInvoiceType>(
          m.PurchaseInvoiceType
        );

        this.invoice = loaded.object<PurchaseInvoice>(m.PurchaseInvoice);

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
      });
  }

  public billedFromAdded(organisation: Organisation): void {
    const supplierRelationship =
      this.allors.context.create<SupplierRelationship>(
        this.m.SupplierRelationship
      );
    supplierRelationship.Supplier = organisation;
    supplierRelationship.InternalOrganisation = this.internalOrganisation;

    this.invoice.BilledFrom = organisation;
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

  public billToEndCustomerAdded(party: Party): void {
    const customerRelationship =
      this.allors.context.create<CustomerRelationship>(
        this.m.CustomerRelationship
      );
    customerRelationship.Customer = party;
    customerRelationship.InternalOrganisation = this.internalOrganisation;

    this.invoice.BillToEndCustomer = party;
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

  public billedFromContactPersonAdded(person: Person): void {
    const organisationContactRelationship =
      this.allors.context.create<OrganisationContactRelationship>(
        this.m.OrganisationContactRelationship
      );
    organisationContactRelationship.Organisation = this.invoice
      .BilledFrom as Organisation;
    organisationContactRelationship.Contact = person;

    this.billedFromContacts.push(person);
    this.invoice.BilledFromContactPerson = person;
  }

  public shipToCustomerContactPersonAdded(person: Person): void {
    const organisationContactRelationship =
      this.allors.context.create<OrganisationContactRelationship>(
        this.m.OrganisationContactRelationship
      );
    organisationContactRelationship.Organisation = this.invoice
      .BilledFrom as Organisation;
    organisationContactRelationship.Contact = person;

    this.shipToCustomerContacts.push(person);
    this.invoice.ShipToCustomerContactPerson = person;
  }

  public billToEndCustomerContactPersonAdded(person: Person): void {
    const organisationContactRelationship =
      this.allors.context.create<OrganisationContactRelationship>(
        this.m.OrganisationContactRelationship
      );
    organisationContactRelationship.Organisation = this.invoice
      .ShipToEndCustomer as Organisation;
    organisationContactRelationship.Contact = person;

    this.billToEndCustomerContacts.push(person);
    this.invoice.BillToEndCustomerContactPerson = person;
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

  public billedFromContactMechanismAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.billedFromContactMechanisms.push(
      partyContactMechanism.ContactMechanism
    );
    this.invoice.BilledFrom.addPartyContactMechanism(partyContactMechanism);
    this.invoice.AssignedBilledFromContactMechanism =
      partyContactMechanism.ContactMechanism;
  }

  public shipToCustomerAddressAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.shipToCustomerAddresses.push(partyContactMechanism.ContactMechanism);
    this.invoice.ShipToCustomer.addPartyContactMechanism(partyContactMechanism);
    this.invoice.AssignedShipToCustomerAddress =
      partyContactMechanism.ContactMechanism as PostalAddress;
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
      if (this.invoice.BilledFrom !== this.previousBilledFrom) {
        this.invoice.AssignedBilledFromContactMechanism = null;
        this.invoice.BilledFromContactPerson = null;
        this.previousBilledFrom = this.invoice.BilledFrom;
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
      if (this.invoice.ShipToCustomer !== this.previousShipToCustomer) {
        this.invoice.AssignedShipToEndCustomerAddress = null;
        this.invoice.ShipToCustomerContactPerson = null;
        this.previousShipToCustomer = this.invoice.ShipToCustomer;
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
      if (this.invoice.BillToEndCustomer !== this.previousBillToEndCustomer) {
        this.invoice.AssignedBillToEndCustomerContactMechanism = null;
        this.invoice.BillToEndCustomerContactPerson = null;
        this.previousBillToEndCustomer = this.invoice.BillToEndCustomer;
      }

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
      if (this.invoice.ShipToEndCustomer !== this.previousShipToEndCustomer) {
        this.invoice.AssignedShipToEndCustomerAddress = null;
        this.invoice.ShipToEndCustomerContactPerson = null;
        this.previousShipToEndCustomer = this.invoice.ShipToEndCustomer;
      }

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
    });
  }
}