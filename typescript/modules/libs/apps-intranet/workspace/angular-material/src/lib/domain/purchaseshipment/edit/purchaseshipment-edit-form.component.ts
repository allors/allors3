import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult, IObject } from '@allors/system/workspace/domain';
import {
  Carrier,
  Currency,
  Facility,
  InternalOrganisation,
  Organisation,
  OrganisationContactRelationship,
  Party,
  PartyContactMechanism,
  Person,
  PostalAddress,
  PurchaseShipment,
  ShipmentMethod,
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
  selector: 'purchaseshipment-edit-form',
  templateUrl: './purchaseshipment-edit-form.component.html',
  providers: [ContextService],
})
export class PurchaseShipmentEditFormComponent extends AllorsFormComponent<PurchaseShipment> {
  readonly m: M;

  currencies: Currency[];
  shipToAddresses: PostalAddress[] = [];
  shipToContacts: Person[] = [];
  shipFromContacts: Person[] = [];
  internalOrganisation: InternalOrganisation;
  locales: Locale[];
  shipmentMethods: ShipmentMethod[];
  carriers: Carrier[];
  addShipToAddress = false;
  addShipFromContactPerson = false;

  facilities: Facility[];
  selectedFacility: Facility;
  addFacility = false;
  previousShipFromParty: Party;

  suppliersFilter: SearchFactory;

  get shipFromCustomerIsPerson(): boolean {
    return (
      !this.object.ShipFromParty ||
      this.object.ShipFromParty.strategy.cls === this.m.Person
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

    this.suppliersFilter = Filters.suppliersFilter(
      this.m,
      this.internalOrganisationId.value
    );
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      p.Facility({ sorting: [{ roleType: m.Facility.Name }] }),
      p.InternalOrganisation({
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
      }),
      p.InternalOrganisation({
        objectId: this.internalOrganisationId.value,
        select: {
          CurrentContacts: {},
        },
      }),
      p.ShipmentMethod({
        sorting: [{ roleType: m.ShipmentMethod.Name }],
      }),
      p.Carrier({ sorting: [{ roleType: m.Carrier.Name }] }),
      p.Organisation({
        predicate: {
          kind: 'Equals',
          propertyType: m.Organisation.IsInternalOrganisation,
          value: true,
        },
        sorting: [{ roleType: m.Organisation.DisplayName }],
      }),
      p.PurchaseShipment({
        objectId: this.editRequest.objectId,
        include: {
          ShipFromParty: {},
          ShipFromAddress: {},
          ShipFromFacility: {},
          ShipToParty: {
            PartyContactMechanisms: {},
          },
          ShipToContactPerson: {},
          ShipToAddress: {},
          ShipFromContactPerson: {},
          Carrier: {},
          ShipmentState: {},
          ElectronicDocuments: {},
        },
      })
    );

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    this.object = pullResult.object('_object');

    this.onPostPullInitialize(pullResult);

    const partyContactMechanisms: PartyContactMechanism[] =
      pullResult.collection<PartyContactMechanism>(
        this.m.Party.CurrentPartyContactMechanisms
      );
    this.shipToAddresses = partyContactMechanisms
      ?.filter(
        (v: PartyContactMechanism) =>
          v.ContactMechanism.strategy.cls === this.m.PostalAddress
      )
      ?.map(
        (v: PartyContactMechanism) => v.ContactMechanism
      ) as PostalAddress[];
    this.shipToContacts = pullResult.collection<Person>(
      this.m.Party.CurrentContacts
    );

    this.selectedFacility = this.object.ShipToFacility;

    this.facilities = pullResult.collection<Facility>(this.m.Facility);
    this.shipmentMethods = pullResult.collection<ShipmentMethod>(
      this.m.ShipmentMethod
    );
    this.carriers = pullResult.collection<Carrier>(this.m.Carrier);

    if (this.object.ShipFromParty) {
      this.updateShipFromParty(this.object.ShipFromParty);
    }

    this.previousShipFromParty = this.object.ShipFromParty;
  }

  public override save(): void {
    this.object.ShipToFacility = this.selectedFacility;

    super.save();
  }

  public facilityAdded(facility: Facility): void {
    this.facilities.push(facility);
    this.selectedFacility = facility;
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

  public shipToAddressAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.object.ShipToParty.addPartyContactMechanism(partyContactMechanism);

    const postalAddress =
      partyContactMechanism.ContactMechanism as PostalAddress;
    this.shipToAddresses.push(postalAddress);
    this.object.ShipToAddress = postalAddress;
  }

  public supplierSelected(customer: IObject) {
    this.updateShipFromParty(customer as Party);
  }

  private updateShipFromParty(organisation: Party): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const pulls = [
      pull.Party({
        object: organisation,
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
}
