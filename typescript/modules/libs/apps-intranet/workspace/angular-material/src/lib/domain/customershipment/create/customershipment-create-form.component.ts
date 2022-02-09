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
  CustomerShipment,
  InternalOrganisation,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { Filters } from '../../../filters/filters';

@Component({
  templateUrl: './customershipment-create-form.component.html',
  providers: [OldPanelManagerService, ContextService],
})
export class CustomerShipmentCreateFormComponent
  extends AllorsFormComponent<CustomerShipment>
  implements CreateOrEditPullHandler, EditIncludeHandler, PostCreatePullHandler
{
  readonly m: M;
  public title: string;
  public subTitle: string;

  customerShipment: CustomerShipment;
  currencies: Currency[];
  shipToAddresses: PostalAddress[] = [];
  shipToContacts: Person[] = [];
  shipFromAddresses: PostalAddress[] = [];
  shipFromContacts: Person[] = [];
  internalOrganisation: InternalOrganisation;

  addShipFromAddress = false;

  addShipToAddress = false;
  addShipToContactPerson = false;

  private previousShipToparty: Party;

  private subscription: Subscription;
  facilities: Facility[];
  locales: Locale[];
  shipmentMethods: ShipmentMethod[];
  carriers: Carrier[];

  customersFilter: SearchFactory;

  get shipToCustomerIsPerson(): boolean {
    return (
      !this.customerShipment.ShipToParty ||
      this.customerShipment.ShipToParty.strategy.cls === this.m.Person
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

    this.subscription = combineLatest([
      this.refreshService.refresh$,
      this.internalOrganisationId.observable$,
    ])
      .pipe(
        switchMap(([, internalOrganisationId]) => {
          const isCreate = this.data.id == null;

          const pulls = [
            this.fetcher.internalOrganisation,
            this.fetcher.locales,
            this.fetcher.ownWarehouses,
            pull.ShipmentMethod({
              sorting: [{ roleType: m.ShipmentMethod.Name }],
            }),
            pull.Carrier({ sorting: [{ roleType: m.Carrier.Name }] }),
            pull.Organisation({
              predicate: {
                kind: 'Equals',
                propertyType: m.Organisation.IsInternalOrganisation,
                value: true,
              },
              sorting: [{ roleType: m.Organisation.DisplayName }],
            }),
          ];

          this.customersFilter = Filters.customersFilter(
            m,
            internalOrganisationId
          );

          return this.allors.context
            .pull(pulls)
            .pipe(map((loaded) => ({ loaded, isCreate })));
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.context.reset();
        this.internalOrganisation =
          this.fetcher.getInternalOrganisation(loaded);
        this.locales = this.fetcher.getAdditionalLocales(loaded);
        this.facilities = this.fetcher.getOwnWarehouses(loaded);
        this.shipmentMethods = loaded.collection<ShipmentMethod>(
          m.ShipmentMethod
        );
        this.carriers = loaded.collection<Carrier>(m.Carrier);

        if (isCreate) {
          this.title = 'Add Customer Shipment';
          this.customerShipment = this.allors.context.create<CustomerShipment>(
            m.CustomerShipment
          );
          this.customerShipment.ShipFromParty = this.internalOrganisation;

          const shipmentPackage = this.allors.context.create<ShipmentPackage>(
            m.ShipmentPackage
          );
          this.customerShipment.addShipmentPackage(shipmentPackage);

          if (this.facilities?.length === 1) {
            this.customerShipment.ShipFromFacility = this.facilities[0];
          }
        } else {
          this.customerShipment = loaded.object<CustomerShipment>(
            m.CustomerShipment
          );

          if (this.customerShipment.canWriteComment) {
            this.title = 'Edit Customer Shipment';
          } else {
            this.title = 'View Customer Shipment';
          }
        }

        if (this.customerShipment.ShipToParty) {
          this.updateShipToParty(this.customerShipment.ShipToParty);
        }

        if (this.customerShipment.ShipFromParty) {
          this.updateShipFromParty(this.customerShipment.ShipFromParty);
        }

        this.previousShipToparty = this.customerShipment.ShipToParty;
      });
  }

  public shipToContactPersonAdded(person: Person): void {
    const organisationContactRelationship =
      this.allors.context.create<OrganisationContactRelationship>(
        this.m.OrganisationContactRelationship
      );
    organisationContactRelationship.Organisation = this.customerShipment
      .ShipToParty as Organisation;
    organisationContactRelationship.Contact = person;

    this.shipToContacts.push(person);
    this.customerShipment.ShipToContactPerson = person;
  }

  public shipToAddressAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.customerShipment.ShipToParty.addPartyContactMechanism(
      partyContactMechanism
    );

    const postalAddress =
      partyContactMechanism.ContactMechanism as PostalAddress;
    this.shipToAddresses.push(postalAddress);
    this.customerShipment.ShipToAddress = postalAddress;
  }

  public shipFromAddressAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.shipFromAddresses.push(
      partyContactMechanism.ContactMechanism as PostalAddress
    );
    this.customerShipment.ShipFromParty.addPartyContactMechanism(
      partyContactMechanism
    );
    this.customerShipment.ShipFromAddress =
      partyContactMechanism.ContactMechanism as PostalAddress;
  }

  public customerSelected(customer: IObject) {
    this.updateShipToParty(customer as Party);
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
      if (this.customerShipment.ShipToParty !== this.previousShipToparty) {
        this.customerShipment.ShipToAddress = null;
        this.customerShipment.ShipToContactPerson = null;
        this.previousShipToparty = this.customerShipment.ShipToParty;
      }

      const partyContactMechanisms = loaded.collection<PartyContactMechanism>(
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
