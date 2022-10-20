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
  VatRate,
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
  selector: 'purchaseorder-edit-form',
  templateUrl: './purchaseorder-edit-form.component.html',
  providers: [ContextService],
})
export class PurchaseOrderEditFormComponent extends AllorsFormComponent<PurchaseOrder> {
  readonly m: M;
  currencies: Currency[];
  takenViaContactMechanisms: ContactMechanism[] = [];
  takenViaContacts: Person[] = [];
  billToContactMechanisms: ContactMechanism[] = [];
  billToContacts: Person[] = [];
  shipToAddresses: ContactMechanism[] = [];
  shipToContacts: Person[] = [];
  vatRates: VatRate[];
  vatRegimes: VatRegime[];
  internalOrganisation: InternalOrganisation;
  ownWarehouses: Facility[];
  facilities: Facility[];
  addFacility = false;

  addSupplier = false;
  addTakenViaContactMechanism = false;
  addTakenViaContactPerson = false;

  addBillToContactMechanism = false;
  addBillToContactPerson = false;

  addShipToAddress = false;
  addShipToContactPerson = false;

  private previousSupplier: Party;

  private takenVia: Party;

  suppliersFilter: SearchFactory;
  irpfRegimes: IrpfRegime[];
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
      this.internalOrganisationId.value
    );
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      this.fetcher.internalOrganisation,
      this.fetcher.ownWarehousesAndStorageLocations,
      p.PurchaseOrder({
        name: '_object',
        objectId: this.editRequest.objectId,
        include: {
          OrderedBy: {},
          StoredInFacility: {},
          TakenViaSupplier: {},
          DerivedTakenViaContactMechanism: {},
          TakenViaContactPerson: {},
          BillToContactPerson: {},
          PurchaseOrderState: {},
          PurchaseOrderShipmentState: {},
          CreatedBy: {},
          LastModifiedBy: {},
          ElectronicDocuments: {},
          DerivedShipToAddress: {
            Country: {},
          },
          DerivedBillToContactMechanism: {
            PostalAddress_Country: {},
          },
          DerivedVatRegime: {
            VatRates: {},
          },
        },
      }),
      p.IrpfRegime({}),
      p.Facility({ sorting: [{ roleType: m.Facility.Name }] }),
      p.Currency({
        predicate: {
          kind: 'Equals',
          propertyType: m.Currency.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.Currency.IsoCode }],
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
    this.ownWarehouses =
      this.fetcher.getOwnWarehousesAndStorageLocations(pullResult);
    this.facilities = pullResult.collection<Facility>(this.m.Facility);
    this.currencies = pullResult.collection<Currency>(this.m.Currency);

    if (this.object.TakenViaSupplier) {
      this.takenVia = this.object.TakenViaSupplier;
      this.updateSupplier(this.takenVia);
    }

    if (this.object.OrderedBy) {
      this.updateOrderedBy(this.object.OrderedBy);
    }

    this.previousSupplier = this.object.TakenViaSupplier;
  }

  public facilityAdded(facility: Facility): void {
    this.facilities.push(facility);
    this.object.StoredInFacility = facility;
  }

  public supplierAdded(organisation: Organisation): void {
    const supplierRelationship =
      this.allors.context.create<SupplierRelationship>(
        this.m.SupplierRelationship
      );
    supplierRelationship.Supplier = organisation;
    supplierRelationship.InternalOrganisation = this.internalOrganisation;

    this.object.TakenViaSupplier = organisation;
    this.takenVia = organisation;
  }

  public takenViaContactPersonAdded(person: Person): void {
    const organisationContactRelationship =
      this.allors.context.create<OrganisationContactRelationship>(
        this.m.OrganisationContactRelationship
      );
    organisationContactRelationship.Organisation = this
      .takenVia as Organisation;
    organisationContactRelationship.Contact = person;

    this.takenViaContacts.push(person);
    this.object.TakenViaContactPerson = person;
  }

  public takenViaContactMechanismAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.takenViaContactMechanisms.push(partyContactMechanism.ContactMechanism);
    partyContactMechanism.Party = this.takenVia;
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

  private updateSupplier(supplier: Party): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const pulls = [
      pull.Party({
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
      pull.Party({
        object: supplier,
        select: {
          CurrentContacts: x,
        },
      }),
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
      if (this.object.TakenViaSupplier !== this.previousSupplier) {
        this.object.AssignedTakenViaContactMechanism = null;
        this.object.TakenViaContactPerson = null;
        this.previousSupplier = this.object.TakenViaSupplier;
      }

      const partyContactMechanisms: PartyContactMechanism[] =
        loaded.collection<PartyContactMechanism>(
          m.Party.CurrentPartyContactMechanisms
        );
      this.takenViaContactMechanisms =
        partyContactMechanisms?.map(
          (v: PartyContactMechanism) => v.ContactMechanism
        ) ?? [];
      this.takenViaContacts =
        loaded.collection<Person>(m.Party.CurrentContacts) ?? [];
    });
  }

  private updateOrderedBy(organisation: Party): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const pulls = [
      pull.Party({
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
      pull.Party({
        object: organisation,
        select: {
          CurrentContacts: x,
        },
      }),
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
      const partyContactMechanisms: PartyContactMechanism[] =
        loaded.collection<PartyContactMechanism>(
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
    });
  }
}
