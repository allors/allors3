import { Component, OnInit, Self, OnDestroy } from '@angular/core';
import { Subscription, combineLatest, BehaviorSubject } from 'rxjs';
import { switchMap, filter } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { Person, Organisation, OrganisationContactRelationship, Party, Facility, InternalOrganisation, PartyContactMechanism, PostalAddress, Currency, PurchaseShipment, ShipmentMethod, Carrier } from '@allors/workspace/domain/default';
import { NavigationService, PanelService, RefreshService, SaveService, SearchFactory } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';
import { IObject } from '@allors/workspace/domain/system';

import { FetcherService } from '../../../../services/fetcher/fetcher-service';
import { InternalOrganisationId } from '../../../../services/state/internal-organisation-id';
import { Filters } from '../../../../filters/filters';

@Component({
  // eslint-disable-next-line @angular-eslint/component-selector
  selector: 'purchaseshipment-overview-detail',
  templateUrl: './purchaseshipment-overview-detail.component.html',
  providers: [PanelService, ContextService],
})
export class PurchaseShipmentOverviewDetailComponent implements OnInit, OnDestroy {
  readonly m: M;

  purchaseShipment: PurchaseShipment;

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

  private subscription: Subscription;
  private refresh$: BehaviorSubject<Date>;
  previousShipFromParty: Party;

  suppliersFilter: SearchFactory;

  get shipFromCustomerIsPerson(): boolean {
    return !this.purchaseShipment.ShipFromParty || this.purchaseShipment.ShipFromParty.strategy.cls === this.m.Person;
  }

  constructor(
    @Self() public allors: ContextService,
    @Self() public panel: PanelService,
    public refreshService: RefreshService,
    public navigationService: NavigationService,
    private saveService: SaveService,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;
    this.refresh$ = new BehaviorSubject(new Date());

    panel.name = 'detail';
    panel.title = 'Purchase Shipment Details';
    panel.icon = 'local_shipping';
    panel.expandable = true;

    // Collapsed
    const pullName = `${this.panel.name}_${this.m.PurchaseShipment.tag}`;

    panel.onPull = (pulls) => {
      this.purchaseShipment = undefined;

      if (this.panel.isCollapsed) {
        const m = this.m;
        const { pullBuilder: pull } = m;
        const id = this.panel.manager.id;

        pulls.push(
          this.fetcher.internalOrganisation,
          pull.PurchaseShipment({
            name: pullName,
            objectId: id,
          })
        );
      }
    };

    panel.onPulled = (loaded) => {
      if (this.panel.isCollapsed) {
        this.purchaseShipment = loaded.object<PurchaseShipment>(pullName);
        this.internalOrganisation = this.fetcher.getInternalOrganisation(loaded);
      }
    };
  }

  public ngOnInit(): void {
    const m = this.m;

    // Maximized
    this.subscription = combineLatest([this.refresh$, this.panel.manager.on$])
      .pipe(
        filter(() => {
          return this.panel.isExpanded;
        }),
        switchMap(() => {
          this.purchaseShipment = undefined;

          const m = this.m;
          const { pullBuilder: pull } = m;
          const x = {};
          const id = this.panel.manager.id;

          const pulls = [
            pull.Facility({ sorting: [{ roleType: m.Facility.Name }] }),
            pull.InternalOrganisation({
              objectId: this.internalOrganisationId.value,
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
            pull.InternalOrganisation({
              objectId: this.internalOrganisationId.value,
              select: {
                CurrentContacts: x,
              },
            }),
            pull.ShipmentMethod({ sorting: [{ roleType: m.ShipmentMethod.Name }] }),
            pull.Carrier({ sorting: [{ roleType: m.Carrier.Name }] }),
            pull.Organisation({
              predicate: { kind: 'Equals', propertyType: m.Organisation.IsInternalOrganisation, value: true },
              sorting: [{ roleType: m.Organisation.DisplayName }],
            }),
            pull.PurchaseShipment({
              objectId: id,
              include: {
                ShipFromParty: x,
                ShipFromAddress: x,
                ShipFromFacility: x,
                ShipToParty: {
                  PartyContactMechanisms: x,
                },
                ShipToContactPerson: x,
                ShipToAddress: x,
                ShipFromContactPerson: x,
                Carrier: x,
                ShipmentState: x,
                ElectronicDocuments: x,
              },
            }),
          ];

          this.suppliersFilter = Filters.suppliersFilter(m, this.internalOrganisationId.value);

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        const partyContactMechanisms: PartyContactMechanism[] = loaded.collection<PartyContactMechanism>(m.Party.CurrentPartyContactMechanisms);
        this.shipToAddresses = partyContactMechanisms?.filter((v: PartyContactMechanism) => v.ContactMechanism.strategy.cls === m.PostalAddress)?.map((v: PartyContactMechanism) => v.ContactMechanism) as PostalAddress[];
        this.shipToContacts = loaded.collection<Person>(m.Party.CurrentContacts);

        this.purchaseShipment = loaded.object<PurchaseShipment>(m.PurchaseShipment);
        this.selectedFacility = this.purchaseShipment.ShipToFacility;

        this.facilities = loaded.collection<Facility>(m.Facility);
        this.shipmentMethods = loaded.collection<ShipmentMethod>(m.ShipmentMethod);
        this.carriers = loaded.collection<Carrier>(m.Carrier);

        if (this.purchaseShipment.ShipFromParty) {
          this.updateShipFromParty(this.purchaseShipment.ShipFromParty);
        }

        this.previousShipFromParty = this.purchaseShipment.ShipFromParty;
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.purchaseShipment.ShipToFacility = this.selectedFacility;

    this.allors.context.push().subscribe(() => {
      this.refreshService.refresh();
      this.panel.toggle();
    }, this.saveService.errorHandler);
  }

  public facilityAdded(facility: Facility): void {
    this.facilities.push(facility);
    this.selectedFacility = facility;
  }

  public shipFromContactPersonAdded(person: Person): void {
    const organisationContactRelationship = this.allors.context.create<OrganisationContactRelationship>(this.m.OrganisationContactRelationship);
    organisationContactRelationship.Organisation = this.purchaseShipment.ShipFromParty as Organisation;
    organisationContactRelationship.Contact = person;

    this.shipFromContacts.push(person);
    this.purchaseShipment.ShipFromContactPerson = person;
  }

  public shipToAddressAdded(partyContactMechanism: PartyContactMechanism): void {
    this.purchaseShipment.ShipToParty.addPartyContactMechanism(partyContactMechanism);

    const postalAddress = partyContactMechanism.ContactMechanism as PostalAddress;
    this.shipToAddresses.push(postalAddress);
    this.purchaseShipment.ShipToAddress = postalAddress;
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
      this.shipFromContacts = loaded.collection<Person>(m.Party.CurrentContacts);
    });
  }
}
