import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult, IObject } from '@allors/system/workspace/domain';
import {
  ContactMechanism,
  Facility,
  InternalOrganisation,
  Organisation,
  OrganisationContactRelationship,
  Party,
  PartyContactMechanism,
  Person,
  PostalAddress,
  PurchaseShipment,
  SupplierRelationship,
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
  templateUrl: './purchaseshipment-create-form.component.html',
  providers: [ContextService],
})
export class PurchaseShipmentCreateFormComponent extends AllorsFormComponent<PurchaseShipment> {
  readonly m: M;

  shipFromContacts: Person[] = [];
  shipToAddresses: ContactMechanism[] = [];
  shipToContacts: Person[] = [];
  internalOrganisation: InternalOrganisation;

  facilities: Facility[];
  selectedFacility: Facility;
  addFacility = false;

  addSupplier = false;
  addShipFromContactPerson = false;

  addShipToAddress = false;
  addShipToContactPerson = false;

  suppliersFilter: SearchFactory;

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
    this.facilities = pullResult.collection<Facility>(this.m.Facility);

    this.object.ShipToParty = this.internalOrganisation;

    if (this.object.ShipFromParty) {
      this.updateSupplier(this.object.ShipFromParty);
    }

    if (this.object.ShipToParty) {
      this.updateOrderedBy(this.object.ShipToParty);
    }
  }

  public override save(): void {
    this.object.ShipToFacility = this.selectedFacility;

    super.save();
  }

  public facilityAdded(facility: Facility): void {
    this.facilities.push(facility);
    this.selectedFacility = facility;
  }

  public supplierAdded(organisation: Organisation): void {
    const supplierRelationship =
      this.allors.context.create<SupplierRelationship>(
        this.m.SupplierRelationship
      );
    supplierRelationship.Supplier = organisation;
    supplierRelationship.InternalOrganisation = this.internalOrganisation;

    this.object.ShipFromParty = organisation;
  }

  public shipFromContactPersonAdded(person: Person): void {
    const organisationContactRelationship =
      this.allors.context.create<OrganisationContactRelationship>(
        this.m.OrganisationContactRelationship
      );
    organisationContactRelationship.Organisation = this.object
      .ShipFromParty as Organisation;
    organisationContactRelationship.Contact = person;

    this.shipFromContacts.push(person);
    this.object.ShipFromContactPerson = person;
  }

  public shipToContactPersonAdded(person: Person): void {
    const organisationContactRelationship =
      this.allors.context.create<OrganisationContactRelationship>(
        this.m.OrganisationContactRelationship
      );
    organisationContactRelationship.Organisation = this.object
      .ShipToParty as Organisation;
    organisationContactRelationship.Contact = person;

    this.shipToContacts.push(person);
    this.object.ShipToContactPerson = person;
  }

  public shipToAddressAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.shipToAddresses.push(partyContactMechanism.ContactMechanism);
    this.object.ShipToParty.addPartyContactMechanism(partyContactMechanism);
    this.object.ShipToAddress =
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
        object: supplier,
        select: {
          CurrentContacts: x,
        },
      }),
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
      this.shipFromContacts = loaded.collection<Person>(
        m.Party.CurrentContacts
      );
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
      this.shipToAddresses = partyContactMechanisms
        ?.filter(
          (v: PartyContactMechanism) =>
            v.ContactMechanism.strategy.cls === m.PostalAddress
        )
        ?.map((v: PartyContactMechanism) => v.ContactMechanism);
      this.shipToContacts = loaded.collection<Person>(m.Party.CurrentContacts);
    });
  }
}
