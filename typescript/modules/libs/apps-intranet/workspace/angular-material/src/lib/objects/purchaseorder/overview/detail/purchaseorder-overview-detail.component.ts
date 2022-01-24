import { Component, OnInit, Self, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { switchMap, filter } from 'rxjs/operators';

import { M } from '@allors/default/workspace/meta';
import {
  Person,
  Organisation,
  OrganisationContactRelationship,
  Party,
  Facility,
  InternalOrganisation,
  SupplierRelationship,
  ContactMechanism,
  PartyContactMechanism,
  PostalAddress,
  Currency,
  PurchaseOrder,
  VatRegime,
  IrpfRegime,
  VatRate,
} from '@allors/default/workspace/domain';
import {
  PanelService,
  RefreshService,
  SaveService,
  SearchFactory,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { IObject } from '@allors/system/workspace/domain';

import { FetcherService } from '../../../../services/fetcher/fetcher-service';
import { InternalOrganisationId } from '../../../../services/state/internal-organisation-id';
import { Filters } from '../../../../filters/filters';

@Component({
  selector: 'purchaseorder-overview-detail',
  templateUrl: './purchaseorder-overview-detail.component.html',
  providers: [ContextService, PanelService],
})
export class PurchaseOrderOverviewDetailComponent implements OnInit, OnDestroy {
  readonly m: M;

  order: PurchaseOrder;
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

  private subscription: Subscription;

  suppliersFilter: SearchFactory;
  irpfRegimes: IrpfRegime[];
  showIrpf: boolean;

  constructor(
    @Self() public allors: ContextService,
    @Self() public panel: PanelService,
    public refreshService: RefreshService,
    private saveService: SaveService,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;

    panel.name = 'detail';
    panel.title = 'Purchase Order Details';
    panel.icon = 'business';
    panel.expandable = true;

    // Collapsed
    const purchaseOrderPullName = `${panel.name}_${this.m.PurchaseOrder.tag}`;

    panel.onPull = (pulls) => {
      if (this.panel.isCollapsed) {
        const m = this.m;
        const { pullBuilder: pull } = m;

        pulls.push(
          pull.PurchaseOrder({
            name: purchaseOrderPullName,
            objectId: this.panel.manager.id,
          })
        );
      }
    };

    panel.onPulled = (loaded) => {
      if (this.panel.isCollapsed) {
        this.order = loaded.object<PurchaseOrder>(purchaseOrderPullName);
      }
    };
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull, treeBuilder: tree } = m;
    const x = {};

    // Expanded
    this.subscription = this.panel.manager.on$
      .pipe(
        filter(() => {
          return this.panel.isExpanded;
        }),
        switchMap(() => {
          this.order = undefined;

          const id = this.panel.manager.id;

          const pulls = [
            this.fetcher.internalOrganisation,
            pull.PurchaseOrder({
              objectId: id,
              include: {
                OrderedBy: x,
                StoredInFacility: x,
                TakenViaSupplier: x,
                DerivedTakenViaContactMechanism: x,
                TakenViaContactPerson: x,
                BillToContactPerson: x,
                PurchaseOrderState: x,
                PurchaseOrderShipmentState: x,
                CreatedBy: x,
                LastModifiedBy: x,
                DerivedShipToAddress: {
                  Country: x,
                },
                DerivedBillToContactMechanism: {
                  PostalAddress_Country: x,
                },
                DerivedVatRegime: {
                  VatRates: x,
                },
              },
            }),
            pull.IrpfRegime({}),
            pull.Facility({ sorting: [{ roleType: m.Facility.Name }] }),
            pull.Currency({
              predicate: {
                kind: 'Equals',
                propertyType: m.Currency.IsActive,
                value: true,
              },
              sorting: [{ roleType: m.Currency.IsoCode }],
            }),
          ];

          this.suppliersFilter = Filters.suppliersFilter(
            m,
            this.internalOrganisationId.value
          );

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        this.order = loaded.object<PurchaseOrder>(m.PurchaseOrder);
        this.internalOrganisation =
          this.fetcher.getInternalOrganisation(loaded);
        this.showIrpf = this.internalOrganisation.Country.IsoCode === 'ES';
        this.vatRegimes = this.internalOrganisation.Country.DerivedVatRegimes;
        this.irpfRegimes = loaded.collection<IrpfRegime>(m.IrpfRegime);
        this.facilities = loaded.collection<Facility>(m.Facility);
        this.currencies = loaded.collection<Currency>(m.Currency);

        if (this.order.TakenViaSupplier) {
          this.takenVia = this.order.TakenViaSupplier;
          this.updateSupplier(this.takenVia);
        }

        if (this.order.OrderedBy) {
          this.updateOrderedBy(this.order.OrderedBy);
        }

        this.previousSupplier = this.order.TakenViaSupplier;
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

  public facilityAdded(facility: Facility): void {
    this.facilities.push(facility);
    this.order.StoredInFacility = facility;
  }

  public supplierAdded(organisation: Organisation): void {
    const supplierRelationship =
      this.allors.context.create<SupplierRelationship>(
        this.m.SupplierRelationship
      );
    supplierRelationship.Supplier = organisation;
    supplierRelationship.InternalOrganisation = this.internalOrganisation;

    this.order.TakenViaSupplier = organisation;
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
    this.order.TakenViaContactPerson = person;
  }

  public takenViaContactMechanismAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.takenViaContactMechanisms.push(partyContactMechanism.ContactMechanism);
    this.takenVia.addPartyContactMechanism(partyContactMechanism);
    this.order.AssignedTakenViaContactMechanism =
      partyContactMechanism.ContactMechanism;
  }

  public billToContactPersonAdded(person: Person): void {
    const organisationContactRelationship =
      this.allors.context.create<OrganisationContactRelationship>(
        this.m.OrganisationContactRelationship
      );
    organisationContactRelationship.Organisation = this.order
      .OrderedBy as Organisation;
    organisationContactRelationship.Contact = person;

    this.billToContacts.push(person);
    this.order.BillToContactPerson = person;
  }

  public billToContactMechanismAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.billToContactMechanisms.push(partyContactMechanism.ContactMechanism);
    this.order.OrderedBy.addPartyContactMechanism(partyContactMechanism);
    this.order.AssignedBillToContactMechanism =
      partyContactMechanism.ContactMechanism;
  }

  public shipToContactPersonAdded(person: Person): void {
    const organisationContactRelationship =
      this.allors.context.create<OrganisationContactRelationship>(
        this.m.OrganisationContactRelationship
      );
    organisationContactRelationship.Organisation = this.order
      .OrderedBy as Organisation;
    organisationContactRelationship.Contact = person;

    this.shipToContacts.push(person);
    this.order.ShipToContactPerson = person;
  }

  public shipToAddressAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.shipToAddresses.push(partyContactMechanism.ContactMechanism);
    this.order.OrderedBy.addPartyContactMechanism(partyContactMechanism);
    this.order.AssignedShipToAddress =
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
      if (this.order.TakenViaSupplier !== this.previousSupplier) {
        this.order.AssignedTakenViaContactMechanism = null;
        this.order.TakenViaContactPerson = null;
        this.previousSupplier = this.order.TakenViaSupplier;
      }

      const partyContactMechanisms: PartyContactMechanism[] =
        loaded.collection<PartyContactMechanism>(
          m.Party.CurrentPartyContactMechanisms
        );
      this.takenViaContactMechanisms = partyContactMechanisms?.map(
        (v: PartyContactMechanism) => v.ContactMechanism
      );
      this.takenViaContacts = loaded.collection<Person>(
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
