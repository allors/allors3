import { Component, OnDestroy, OnInit, Self, Optional, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { Person, Organisation, OrganisationContactRelationship, Party, Facility, InternalOrganisation, SupplierRelationship, ContactMechanism, PartyContactMechanism, PostalAddress, PurchaseShipment } from '@allors/workspace/domain/default';
import { ObjectData, RefreshService, SaveService, SearchFactory, TestScope } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';
import { IObject } from '@allors/workspace/domain/system';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { Filters } from '../../../filters/filters';

@Component({
  templateUrl: './purchaseshipment-create.component.html',
  providers: [ContextService],
})
export class PurchaseShipmentCreateComponent extends TestScope implements OnInit, OnDestroy {
  readonly m: M;

  title = 'Add Purchase Shipment';

  shipment: PurchaseShipment;
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

  private subscription: Subscription;

  suppliersFilter: SearchFactory;

  constructor(
    @Self() public allors: ContextService,
    @Optional() @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<PurchaseShipmentCreateComponent>,
    private refreshService: RefreshService,
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
            pull.Facility({ sorting: [{ roleType: m.Facility.Name }] }),
            pull.Organisation({
              predicate: { kind: 'Equals', propertyType: m.Organisation.IsInternalOrganisation, value: true },
              sorting: [{ roleType: m.Organisation.PartyName }],
            }),
          ];

          this.suppliersFilter = Filters.suppliersFilter(m, this.internalOrganisationId.value);

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.internalOrganisation = this.fetcher.getInternalOrganisation(loaded);
        this.facilities = loaded.collection<Facility>(m.Facility);

        this.shipment = this.allors.context.create<PurchaseShipment>(m.PurchaseShipment);
        this.shipment.ShipToParty = this.internalOrganisation;

        if (this.shipment.ShipFromParty) {
          this.updateSupplier(this.shipment.ShipFromParty);
        }

        if (this.shipment.ShipToParty) {
          this.updateOrderedBy(this.shipment.ShipToParty);
        }
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.shipment.ShipToFacility = this.selectedFacility;

    this.allors.context.push().subscribe(() => {
      this.dialogRef.close(this.shipment);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }

  public facilityAdded(facility: Facility): void {
    this.facilities.push(facility);
    this.selectedFacility = facility;
  }

  public supplierAdded(organisation: Organisation): void {
    const supplierRelationship = this.allors.context.create<SupplierRelationship>(this.m.SupplierRelationship);
    supplierRelationship.Supplier = organisation;
    supplierRelationship.InternalOrganisation = this.internalOrganisation;

    this.shipment.ShipFromParty = organisation;
  }

  public shipFromContactPersonAdded(person: Person): void {
    const organisationContactRelationship = this.allors.context.create<OrganisationContactRelationship>(this.m.OrganisationContactRelationship);
    organisationContactRelationship.Organisation = this.shipment.ShipFromParty as Organisation;
    organisationContactRelationship.Contact = person;

    this.shipFromContacts.push(person);
    this.shipment.ShipFromContactPerson = person;
  }

  public shipToContactPersonAdded(person: Person): void {
    const organisationContactRelationship = this.allors.context.create<OrganisationContactRelationship>(this.m.OrganisationContactRelationship);
    organisationContactRelationship.Organisation = this.shipment.ShipToParty as Organisation;
    organisationContactRelationship.Contact = person;

    this.shipToContacts.push(person);
    this.shipment.ShipToContactPerson = person;
  }

  public shipToAddressAdded(partyContactMechanism: PartyContactMechanism): void {
    this.shipToAddresses.push(partyContactMechanism.ContactMechanism);
    this.shipment.ShipToParty.addPartyContactMechanism(partyContactMechanism);
    this.shipment.ShipToAddress = partyContactMechanism.ContactMechanism as PostalAddress;
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
      this.shipFromContacts = loaded.collection<Person>(m.Party.CurrentContacts);
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
      const partyContactMechanisms: PartyContactMechanism[] = loaded.collection<PartyContactMechanism>(m.Party.CurrentPartyContactMechanisms);
      this.shipToAddresses = partyContactMechanisms?.filter((v: PartyContactMechanism) => v.ContactMechanism.strategy.cls === m.PostalAddress)?.map((v: PartyContactMechanism) => v.ContactMechanism);
      this.shipToContacts = loaded.collection<Person>(m.Party.CurrentContacts);
    });
  }
}
