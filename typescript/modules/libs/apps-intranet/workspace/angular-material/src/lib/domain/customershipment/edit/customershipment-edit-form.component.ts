import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult, IObject } from '@allors/system/workspace/domain';
import {
  Carrier,
  Currency,
  CustomerShipment,
  Facility,
  InternalOrganisation,
  Organisation,
  OrganisationContactRelationship,
  Party,
  PartyContactMechanism,
  Person,
  PostalAddress,
  ShipmentMethod,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
  SearchFactory,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { Filters } from '../../../filters/filters';
import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  selector: 'customershipment-edit-form',
  templateUrl: './customershipment-edit-form.component.html',
  providers: [ContextService],
})
export class CustomerShipmentEditFormComponent extends AllorsFormComponent<CustomerShipment> {
  readonly m: M;

  currencies: Currency[];
  shipToAddresses: PostalAddress[] = [];
  shipToContacts: Person[] = [];
  shipFromAddresses: PostalAddress[] = [];
  shipFromContacts: Person[] = [];
  internalOrganisation: InternalOrganisation;
  facilities: Facility[];
  shipmentMethods: ShipmentMethod[];
  carriers: Carrier[];

  addShipFromAddress = false;

  addShipToAddress = false;
  addShipToContactPerson = false;

  private previousShipToparty: Party;

  customersFilter: SearchFactory;

  get shipToCustomerIsPerson(): boolean {
    return (
      !this.object.ShipToParty ||
      this.object.ShipToParty.strategy.cls === this.m.Person
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
      this.internalOrganisationId.value
    );
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      this.fetcher.internalOrganisation,
      this.fetcher.ownWarehouses,
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
      p.CustomerShipment({
        name: '_object',
        objectId: this.editRequest.objectId,
        include: {
          ShipFromParty: {},
          ShipFromAddress: {},
          ShipFromFacility: {},
          ShipToParty: {},
          ShipToAddress: {},
          ShipToContactPerson: {},
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

    this.facilities = this.fetcher.getOwnWarehouses(pullResult);
    this.carriers = pullResult.collection<Carrier>(this.m.Carrier);
    this.shipmentMethods = pullResult.collection<ShipmentMethod>(
      this.m.ShipmentMethod
    );

    if (this.object.ShipToParty) {
      this.updateShipToParty(this.object.ShipToParty);
    }

    if (this.object.ShipFromParty) {
      this.updateShipFromParty(this.object.ShipFromParty);
    }
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
    this.object.ShipToParty.addPartyContactMechanism(partyContactMechanism);

    const postalAddress =
      partyContactMechanism.ContactMechanism as PostalAddress;
    this.shipToAddresses.push(postalAddress);
    this.object.ShipToAddress = postalAddress;
  }

  public shipFromAddressAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.shipFromAddresses.push(
      partyContactMechanism.ContactMechanism as PostalAddress
    );
    this.object.ShipFromParty.addPartyContactMechanism(partyContactMechanism);
    this.object.ShipFromAddress =
      partyContactMechanism.ContactMechanism as PostalAddress;
  }

  public customerSelected(customer: IObject) {
    this.updateShipToParty(customer as Party);
    this.previousShipToparty = this.object.ShipToParty;
  }

  private updateShipToParty(customer: Party): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const pulls = [
      pull.Party({
        object: customer,
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
        object: customer,
        select: {
          CurrentContacts: x,
        },
      }),
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
      if (this.object.ShipToParty !== this.previousShipToparty) {
        this.object.ShipToAddress = null;
        this.object.ShipToContactPerson = null;
        this.previousShipToparty = this.object.ShipToParty;
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
        ?.map(
          (v: PartyContactMechanism) => v.ContactMechanism
        ) as PostalAddress[];
      this.shipToContacts = loaded.collection<Person>(m.Party.CurrentContacts);
    });
  }

  private updateShipFromParty(organisation: Party): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const pulls = [
      pull.Party({
        object: organisation,
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
      this.shipFromAddresses = partyContactMechanisms
        ?.filter(
          (v: PartyContactMechanism) =>
            v.ContactMechanism.strategy.cls === m.PostalAddress
        )
        ?.map(
          (v: PartyContactMechanism) => v.ContactMechanism
        ) as PostalAddress[];
      this.shipToContacts = loaded.collection<Person>(m.Party.CurrentContacts);
    });
  }
}
