import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  Facility,
  InventoryItem,
  InventoryItemTransaction,
  InventoryTransactionReason,
  Lot,
  NonSerialisedInventoryItem,
  NonSerialisedInventoryItemState,
  Part,
  SerialisedInventoryItem,
  SerialisedInventoryItemState,
  SerialisedItem,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { FetcherService } from '../../../services/fetcher/fetcher-service';

@Component({
  templateUrl: './inventoryitemtransaction-form.component.html',
  providers: [ContextService],
})
export class InventoryItemTransactionFormComponent extends AllorsFormComponent<InventoryItemTransaction> {
  readonly m: M;

  title = 'Add Inventory Item Transaction';

  inventoryItem: InventoryItem;
  inventoryTransactionReasons: InventoryTransactionReason[];
  selectedPart: Part;
  selectedFacility: Facility;
  addFacility = false;
  facilities: Facility[];
  lots: Lot[];
  serialised: boolean;
  serialisedInventoryItem: SerialisedInventoryItem;
  serialisedItem: SerialisedItem;
  part: Part;
  nonSerialisedInventoryItemState: NonSerialisedInventoryItemState[];
  serialisedInventoryItemState: SerialisedInventoryItemState[];
  nonSerialisedInventoryItem: NonSerialisedInventoryItem;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      p.Facility({ sorting: [{ roleType: m.Facility.Name }] }),
      p.InventoryTransactionReason({}),
      p.NonSerialisedInventoryItemState({}),
      p.SerialisedInventoryItemState({}),
      p.Lot({ sorting: [{ roleType: m.Lot.LotNumber }] })
    );

    if (this.editRequest) {
      pulls.push(
        p.BasePrice({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            Currency: {},
          },
        })
      );
    }
    const initializer = this.createRequest.initializer;
    if (initializer) {
      pulls.push(
        p.InventoryItem({
          objectId: initializer.id,
          include: {
            SerialisedInventoryItem_SerialisedItem: {},
            Facility: {},
            UnitOfMeasure: {},
            Lot: {},
            Part: {
              InventoryItemKind: {},
              PartWeightedAverage: {},
            },
          },
        }),
        p.SerialisedItem({
          objectId: initializer.id,
        }),
        p.SerialisedItem({
          objectId: initializer.id,
          select: {
            PartWhereSerialisedItem: {
              include: {
                PartWeightedAverage: {},
              },
            },
          },
        })
      );
    }
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.inventoryTransactionReasons = pullResult.collection(
      this.m.InventoryTransactionReason
    );

    this.nonSerialisedInventoryItemState = pullResult.collection(
      this.m.NonSerialisedInventoryItemState
    );

    this.serialisedInventoryItemState = pullResult.collection(
      this.m.SerialisedInventoryItemState
    );

    this.part =
      pullResult.object(this.m.Part) ||
      pullResult.collection<Part>(
        this.m.SerialisedItem.PartWhereSerialisedItem
      )[0];

    this.facilities = pullResult.collection(this.m.Facility);
    this.lots = pullResult.collection(this.m.Lot);
    this.serialisedItem = pullResult.object(this.m.SerialisedItem);
    this.inventoryItem = pullResult.object(this.m.InventoryItem);

    if (this.part) {
      this.selectedPart = this.part;
    }

    if (this.inventoryItem) {
      this.serialisedInventoryItem = pullResult.object(this.m.InventoryItem);
      this.nonSerialisedInventoryItem = pullResult.object(this.m.InventoryItem);
      this.part = this.inventoryItem.Part;
      this.selectedFacility = this.inventoryItem.Facility;
      this.serialised =
        this.inventoryItem.Part.InventoryItemKind.UniqueId ===
        '2596e2dd-3f5d-4588-a4a2-167d6fbe3fae';
    }

    this.object.TransactionDate = new Date();
    this.object.Part = this.part;
    this.object.Cost = this.part.PartWeightedAverage?.AverageCost;

    if (this.inventoryItem) {
      this.object.Facility = this.inventoryItem.Facility;
      this.object.UnitOfMeasure = this.inventoryItem.UnitOfMeasure;
      this.object.Lot = this.inventoryItem.Lot;

      if (this.serialised) {
        this.object.SerialisedItem =
          this.serialisedInventoryItem.SerialisedItem;
        this.object.SerialisedInventoryItemState =
          this.serialisedInventoryItem.SerialisedInventoryItemState;
      } else {
        this.object.NonSerialisedInventoryItemState =
          this.nonSerialisedInventoryItem.NonSerialisedInventoryItemState;
      }
    }

    if (this.serialisedItem) {
      this.object.SerialisedItem = this.serialisedItem;
      this.serialised = true;
    }
  }

  public override save(): void {
    this.object.Facility = this.selectedFacility;
    super.save();
  }

  public facilityAdded(facility: Facility): void {
    this.facilities.push(facility);
    this.selectedFacility = facility;
  }
}
