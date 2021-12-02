import { Component, OnInit, Self, OnDestroy } from '@angular/core';
import { Subscription, combineLatest, BehaviorSubject } from 'rxjs';
import { switchMap, filter } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { Carrier, Person, Organisation, PartyContactMechanism, OrganisationContactRelationship, Party, CustomerShipment, Currency, PostalAddress, Facility, ShipmentMethod, InternalOrganisation } from '@allors/workspace/domain/default';
import { NavigationService, PanelService, RefreshService, SaveService, SearchFactory } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';
import { IObject } from '@allors/workspace/domain/system';

import { Filters } from '../../../../filters/filters';
import { FetcherService } from '../../../../services/fetcher/fetcher-service';
import { InternalOrganisationId } from '../../../../services/state/internal-organisation-id';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'customershipment-overview-detail',
  templateUrl: './customershipment-overview-detail.component.html',
  providers: [PanelService, ContextService],
})
export class CustomerShipmentOverviewDetailComponent implements OnInit, OnDestroy {
  readonly m: M;

  customerShipment: CustomerShipment;

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

  private subscription: Subscription;
  private refresh$: BehaviorSubject<Date>;

  customersFilter: SearchFactory;

  get shipToCustomerIsPerson(): boolean {
    return !this.customerShipment.ShipToParty || this.customerShipment.ShipToParty.strategy.cls === this.m.Person;
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
    panel.title = 'Customer Shipment Details';
    panel.icon = 'local_shipping';
    panel.expandable = true;

    // Collapsed
    const pullName = `${this.panel.name}_${this.m.CustomerShipment.tag}`;

    panel.onPull = (pulls) => {
      this.customerShipment = undefined;

      if (this.panel.isCollapsed) {
        const m = this.m;
        const { pullBuilder: pull } = m;
        const id = this.panel.manager.id;

        pulls.push(
          this.fetcher.internalOrganisation,
          pull.CustomerShipment({
            name: pullName,
            objectId: id,
          })
        );
      }
    };

    panel.onPulled = (loaded) => {
      if (this.panel.isCollapsed) {
        this.customerShipment = loaded.object<CustomerShipment>(pullName);
        this.internalOrganisation = this.fetcher.getInternalOrganisation(loaded);
      }
    };
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    // Maximized
    this.subscription = combineLatest([this.refresh$, this.panel.manager.on$])
      .pipe(
        filter(() => {
          return this.panel.isExpanded;
        }),
        switchMap(() => {
          this.customerShipment = undefined;

          const id = this.panel.manager.id;

          const pulls = [
            this.fetcher.ownWarehouses,
            pull.ShipmentMethod({ sorting: [{ roleType: m.ShipmentMethod.Name }] }),
            pull.Carrier({ sorting: [{ roleType: m.Carrier.Name }] }),
            pull.Organisation({
              predicate: { kind: 'Equals', propertyType: m.Organisation.IsInternalOrganisation, value: true },
              sorting: [{ roleType: m.Organisation.PartyName }],
            }),
            pull.CustomerShipment({
              objectId: id,
              include: {
                ShipFromParty: x,
                ShipFromAddress: x,
                ShipFromFacility: x,
                ShipToParty: x,
                ShipToAddress: x,
                ShipToContactPerson: x,
                Carrier: x,
                ShipmentState: x,
                ElectronicDocuments: x,
              },
            }),
          ];

          this.customersFilter = Filters.customersFilter(m, this.internalOrganisationId.value);

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        this.customerShipment = loaded.object<CustomerShipment>(m.CustomerShipment);
        this.facilities = this.fetcher.getOwnWarehouses(loaded);
        this.shipmentMethods = loaded.collection<ShipmentMethod>(m.ShipmentMethod);
        this.carriers = loaded.collection<Carrier>(m.Carrier);

        if (this.customerShipment.ShipToParty) {
          this.updateShipToParty(this.customerShipment.ShipToParty);
        }

        if (this.customerShipment.ShipFromParty) {
          this.updateShipFromParty(this.customerShipment.ShipFromParty);
        }

        this.previousShipToparty = this.customerShipment.ShipToParty;
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.allors.context.push().subscribe(() => {
      this.refreshService.refresh();
      this.panel.toggle();
    }, this.saveService.errorHandler);
  }

  public shipToContactPersonAdded(person: Person): void {
    const organisationContactRelationship = this.allors.context.create<OrganisationContactRelationship>(this.m.OrganisationContactRelationship);
    organisationContactRelationship.Organisation = this.customerShipment.ShipToParty as Organisation;
    organisationContactRelationship.Contact = person;

    this.shipToContacts.push(person);
    this.customerShipment.ShipToContactPerson = person;
  }

  public shipToAddressAdded(partyContactMechanism: PartyContactMechanism): void {
    this.customerShipment.ShipToParty.addPartyContactMechanism(partyContactMechanism);

    const postalAddress = partyContactMechanism.ContactMechanism as PostalAddress;
    this.shipToAddresses.push(postalAddress);
    this.customerShipment.ShipToAddress = postalAddress;
  }

  public shipFromAddressAdded(partyContactMechanism: PartyContactMechanism): void {
    this.shipFromAddresses.push(partyContactMechanism.ContactMechanism as PostalAddress);
    this.customerShipment.ShipFromParty.addPartyContactMechanism(partyContactMechanism);
    this.customerShipment.ShipFromAddress = partyContactMechanism.ContactMechanism as PostalAddress;
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

      const partyContactMechanisms: PartyContactMechanism[] = loaded.collection<PartyContactMechanism>(m.Party.CurrentPartyContactMechanisms);
      this.shipToAddresses = partyContactMechanisms?.filter((v: PartyContactMechanism) => v.ContactMechanism.strategy.cls === m.PostalAddress)?.map((v: PartyContactMechanism) => v.ContactMechanism) as PostalAddress[];
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
      const partyContactMechanisms: PartyContactMechanism[] = loaded.collection<PartyContactMechanism>(m.Party.CurrentPartyContactMechanisms);
      this.shipFromAddresses = partyContactMechanisms?.filter((v: PartyContactMechanism) => v.ContactMechanism.strategy.cls === m.PostalAddress)?.map((v: PartyContactMechanism) => v.ContactMechanism) as PostalAddress[];
      this.shipToContacts = loaded.collection<Person>(m.Party.CurrentContacts);
    });
  }
}
