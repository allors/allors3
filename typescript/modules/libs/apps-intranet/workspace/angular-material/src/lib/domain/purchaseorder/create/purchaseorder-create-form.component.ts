import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult, IObject } from '@allors/system/workspace/domain';
import {
  ContactMechanism,
  Currency,
  Facility,
  InternalOrganisation,
  IrpfRegime,
  Organisation,
  OrganisationContactRelationship,
  Party,
  PartyContactMechanism,
  Person,
  PostalAddress,
  PurchaseOrder,
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
  templateUrl: './purchaseorder-create-form.component.html',
  providers: [ContextService],
})
export class PurchaseOrderCreateFormComponent extends AllorsFormComponent<PurchaseOrder> {
  readonly m: M;
  currencies: Currency[];
  takenViaContactMechanisms: ContactMechanism[] = [];
  takenViaContacts: Person[] = [];
  billToContactMechanisms: ContactMechanism[] = [];
  billToContacts: Person[] = [];
  shipToAddresses: ContactMechanism[] = [];
  shipToContacts: Person[] = [];
  vatRegimes: VatRegime[];
  internalOrganisation: InternalOrganisation;
  facilities: Facility[];
  addFacility = false;

  addSupplier = false;

  addTakenViaContactMechanism = false;
  addTakenViaContactPerson = false;

  addBillToContactMechanism = false;
  addBillToContactPerson = false;

  addShipToAddress = false;
  addShipToContactPerson = false;

  suppliersFilter: SearchFactory;
  irpfRegimes: IrpfRegime[];
  currencyInitialRole: Currency;
  shipToAddressInitialRole: PostalAddress;
  billToContactMechanismInitialRole: ContactMechanism;
  takenViaContactMechanismInitialRole: ContactMechanism;
  showIrpf: boolean;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;

    this.suppliersFilter = Filters.suppliersFilter(
      this.m,
      internalOrganisationId.value
    );
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      this.fetcher.internalOrganisation,
      p.IrpfRegime({}),
      p.Currency({
        predicate: {
          kind: 'Equals',
          propertyType: m.Currency.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.Currency.IsoCode }],
      }),
      p.Facility({ sorting: [{ roleType: m.Facility.Name }] }),
      p.Organisation({
        predicate: {
          kind: 'Equals',
          propertyType: m.Organisation.IsInternalOrganisation,
          value: true,
        },
        sorting: [{ roleType: m.Organisation.DisplayName }],
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
    this.facilities = pullResult.collection<Facility>(this.m.Facility);

    this.object.OrderedBy = this.internalOrganisation;

    if (this.object.OrderedBy) {
      this.updateOrderedBy(this.object.OrderedBy);
    }
  }

  public supplierAdded(supplier: Organisation): void {
    const supplierRelationship =
      this.allors.context.create<SupplierRelationship>(
        this.m.SupplierRelationship
      );
    supplierRelationship.Supplier = supplier;
    supplierRelationship.InternalOrganisation = this.internalOrganisation;

    this.object.TakenViaSupplier = supplier;
    
    this.takenViaContactMechanisms = [];
    this.takenViaContacts = [];
    this.takenViaContactMechanismInitialRole = null;
  }

  public takenViaContactPersonAdded(person: Person): void {
    const organisationContactRelationship =
      this.allors.context.create<OrganisationContactRelationship>(
        this.m.OrganisationContactRelationship
      );
    organisationContactRelationship.Organisation = this.object
      .TakenViaSupplier as Organisation;
    organisationContactRelationship.Contact = person;

    this.takenViaContacts.push(person);
    this.object.TakenViaContactPerson = person;
  }

  public takenViaContactMechanismAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.takenViaContactMechanisms.push(partyContactMechanism.ContactMechanism);
    partyContactMechanism.Party = this.object.TakenViaSupplier;
    this.object.AssignedTakenViaContactMechanism =
      partyContactMechanism.ContactMechanism;
  }

  public billToContactPersonAdded(person: Person): void {
    const organisationContactRelationship =
      this.allors.context.create<OrganisationContactRelationship>(
        this.m.OrganisationContactRelationship
      );
    organisationContactRelationship.Organisation = this.object
      .OrderedBy as Organisation;
    organisationContactRelationship.Contact = person;

    this.billToContacts.push(person);
    this.object.BillToContactPerson = person;
  }

  public billToContactMechanismAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.billToContactMechanisms.push(partyContactMechanism.ContactMechanism);
    partyContactMechanism.Party = this.object.OrderedBy;
    this.object.AssignedBillToContactMechanism =
      partyContactMechanism.ContactMechanism;
  }

  public shipToContactPersonAdded(person: Person): void {
    const organisationContactRelationship =
      this.allors.context.create<OrganisationContactRelationship>(
        this.m.OrganisationContactRelationship
      );
    organisationContactRelationship.Organisation = this.object
      .OrderedBy as Organisation;
    organisationContactRelationship.Contact = person;

    this.shipToContacts.push(person);
    this.object.ShipToContactPerson = person;
  }

  public shipToAddressAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.shipToAddresses.push(partyContactMechanism.ContactMechanism);
    partyContactMechanism.Party = this.object.OrderedBy;
    this.object.AssignedShipToAddress =
      partyContactMechanism.ContactMechanism as PostalAddress;
  }

  public supplierSelected(supplier: IObject) {
    this.updateSupplier(supplier as Party);
  }

  public facilityAdded(facility: Facility): void {
    this.facilities.push(facility);
    // TODO: Martien
    this.object.StoredInFacility = facility;
  }

  private updateSupplier(supplier: Party): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const pulls = [
      pull.Organisation({
        object: supplier,
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
        object: supplier,
        select: {
          CurrentContacts: x,
        },
      }),
      pull.Organisation({
        object: supplier,
        name: 'selectedSupplier',
        include: {
          OrderAddress: x,
        },
      }),
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
      const partyContactMechanisms: PartyContactMechanism[] =
        loaded.collection<PartyContactMechanism>(
          m.Party.CurrentPartyContactMechanisms
        );
      this.takenViaContactMechanisms = partyContactMechanisms?.map(
        (v: PartyContactMechanism) => v.ContactMechanism
      ) ?? [];
      this.takenViaContacts = loaded.collection<Person>(
        m.Party.CurrentContacts
      ) ?? [];

      const selectedSupplier = loaded.object<Organisation>('selectedSupplier');
      this.takenViaContactMechanismInitialRole = selectedSupplier.OrderAddress;
    });
  }

  private updateOrderedBy(organisation: Party): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const pulls = [
      pull.Organisation({
        object: organisation,
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
        object: organisation,
        select: {
          CurrentContacts: x,
        },
      }),
      pull.Organisation({
        object: organisation,
        name: 'selectedOrganisation',
        include: {
          PreferredCurrency: x,
          ShippingAddress: x,
          BillingAddress: x,
          GeneralCorrespondence: x,
        },
      }),
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
      const partyContactMechanisms = loaded.collection<PartyContactMechanism>(
        m.Party.CurrentPartyContactMechanisms
      );
      this.billToContactMechanisms = partyContactMechanisms?.map(
        (v: PartyContactMechanism) => v.ContactMechanism
      );
      this.shipToAddresses = partyContactMechanisms
        ?.filter(
          (v: PartyContactMechanism) =>
            v.ContactMechanism.strategy.cls === m.PostalAddress
        )
        ?.map((v: PartyContactMechanism) => v.ContactMechanism);
      this.billToContacts = loaded.collection<Person>(m.Party.CurrentContacts);
      this.shipToContacts = this.billToContacts;

      const selectedOrganisation = loaded.object<Organisation>(
        'selectedOrganisation'
      );
      this.currencyInitialRole = selectedOrganisation.PreferredCurrency;
      this.shipToAddressInitialRole = selectedOrganisation.ShippingAddress;
      this.billToContactMechanismInitialRole =
        selectedOrganisation.BillingAddress ??
        selectedOrganisation.GeneralCorrespondence;
    });
  }
}
