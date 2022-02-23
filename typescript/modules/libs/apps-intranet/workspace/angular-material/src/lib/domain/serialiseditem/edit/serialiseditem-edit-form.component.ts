import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  Enumeration,
  Facility,
  InternalOrganisation,
  Locale,
  Organisation,
  Ownership,
  Part,
  SerialisedInventoryItem,
  SerialisedItem,
  SerialisedItemAvailability,
  SerialisedItemState,
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
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'serialiseditem-edit-form',
  templateUrl: './serialiseditem-edit-form.component.html',
  providers: [ContextService],
})
export class SerialisedItemEditFormComponent extends AllorsFormComponent<SerialisedItem> {
  readonly m: M;

  internalOrganisation: InternalOrganisation;
  locales: Locale[];
  serialisedItemStates: Enumeration[];
  ownerships: Enumeration[];
  part: Part;
  currentSuppliers: Organisation[];
  currentFacility: Facility;
  serialisedItemAvailabilities: Enumeration[];
  internalOrganisationsFilter: SearchFactory;
  partiesFilter: SearchFactory;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private snackBar: MatSnackBar,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;

    this.internalOrganisationsFilter = Filters.internalOrganisationsFilter(
      this.m
    );
    this.partiesFilter = Filters.partiesFilter(this.m);
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      p.SerialisedItem({
        name: '_object',
        objectId: this.editRequest.objectId,
        include: {
          SerialisedItemState: {},
          SerialisedItemCharacteristics: {
            SerialisedItemCharacteristicType: {
              UnitOfMeasure: {},
            },
          },
          LocalisedNames: {
            Locale: {},
          },
          LocalisedDescriptions: {
            Locale: {},
          },
          LocalisedComments: {
            Locale: {},
          },
          LocalisedKeywords: {
            Locale: {},
          },
          Ownership: {},
          Buyer: {},
          Seller: {},
          OwnedBy: {},
          RentedBy: {},
          PrimaryPhoto: {},
          SecondaryPhotos: {},
          AdditionalPhotos: {},
          PrivatePhotos: {},
          PublicElectronicDocuments: {},
          PrivateElectronicDocuments: {},
          PublicLocalisedElectronicDocuments: {},
          PrivateLocalisedElectronicDocuments: {},
          PurchaseInvoice: {},
          PurchaseOrder: {},
          SuppliedBy: {},
          AssignedSuppliedBy: {},
        },
      }),
      this.fetcher.locales,
      p.SerialisedItem({
        objectId: this.editRequest.objectId,
        select: {
          PartWhereSerialisedItem: {
            include: { SerialisedItems: {} },
          },
        },
      }),
      p.SerialisedItem({
        objectId: this.editRequest.objectId,
        select: {
          SerialisedInventoryItemsWhereSerialisedItem: {
            include: {
              Facility: {},
            },
          },
        },
      }),
      p.InternalOrganisation({
        objectId: this.internalOrganisationId.value,
        select: {
          ObsoleteCurrentSuppliers: {},
        },
      }),
      p.SerialisedItemState({
        predicate: {
          kind: 'Equals',
          propertyType: m.SerialisedItemState.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.SerialisedItemState.Name }],
      }),
      p.SerialisedItemAvailability({
        predicate: {
          kind: 'Equals',
          propertyType: m.SerialisedItemAvailability.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.SerialisedItemAvailability.Name }],
      }),
      p.Ownership({
        predicate: {
          kind: 'Equals',
          propertyType: m.Ownership.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.Ownership.Name }],
      })
    );

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    this.object = pullResult.object('_object');

    this.onPostPullInitialize(pullResult);

    this.currentSuppliers = pullResult.collection<Organisation>(
      this.m.InternalOrganisation.ObsoleteCurrentSuppliers
    );

    this.locales = this.fetcher.getAdditionalLocales(pullResult);
    this.serialisedItemStates = pullResult.collection<SerialisedItemState>(
      this.m.SerialisedItemState
    );
    this.serialisedItemAvailabilities =
      pullResult.collection<SerialisedItemAvailability>(
        this.m.SerialisedItemAvailability
      );
    this.ownerships = pullResult.collection<Ownership>(this.m.Ownership);
    this.part = pullResult.collection<Part>(
      this.m.SerialisedItem.PartWhereSerialisedItem
    )[0];

    const serialisedInventoryItems =
      pullResult.collection<SerialisedInventoryItem>(
        this.m.SerialisedItem.SerialisedInventoryItemsWhereSerialisedItem
      );
    const inventoryItem = serialisedInventoryItems?.find(
      (v) => v.Quantity === 1
    );
    if (inventoryItem) {
      this.currentFacility = inventoryItem.Facility;
    }
  }

  public partSelected(part: Part): void {
    if (part) {
      this.part = part;
    }
  }
}
