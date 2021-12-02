import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import {
  Part,
  Facility,
  NonSerialisedInventoryItem,
  InventoryItem,
  InventoryItemTransaction,
  InventoryTransactionReason,
  Lot,
  SerialisedInventoryItem,
  SerialisedItem,
  NonSerialisedInventoryItemState,
  SerialisedInventoryItemState,
} from '@allors/workspace/domain/default';
import { ObjectData, RefreshService, SaveService } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';

import { FetcherService } from '../../../services/fetcher/fetcher-service';

@Component({
  templateUrl: './inventoryitemtransaction-edit.component.html',
  providers: [ContextService],
})
export class InventoryItemTransactionEditComponent implements OnInit, OnDestroy {
  readonly m: M;

  title = 'Add Inventory Item Transaction';

  inventoryItem: InventoryItem;
  inventoryItemTransaction: InventoryItemTransaction;
  inventoryTransactionReasons: InventoryTransactionReason[];
  selectedPart: Part;
  parts: Part[];
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

  private subscription: Subscription;
  nonSerialisedInventoryItem: NonSerialisedInventoryItem;

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<InventoryItemTransactionEditComponent>,

    public refreshService: RefreshService,
    private saveService: SaveService,
    private fetcher: FetcherService
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest(this.refreshService.refresh$)
      .pipe(
        switchMap(() => {
          const pulls = [
            pull.Facility({ sorting: [{ roleType: m.Facility.Name }] }),
            pull.Part({}),
            pull.InventoryTransactionReason({}),
            pull.NonSerialisedInventoryItemState({}),
            pull.SerialisedInventoryItemState({}),
            pull.Lot({ sorting: [{ roleType: m.Lot.LotNumber }] }),
            pull.InventoryItem({
              objectId: this.data.associationId,
              include: {
                SerialisedInventoryItem_SerialisedItem: x,
                Facility: x,
                UnitOfMeasure: x,
                Lot: x,
                Part: {
                  InventoryItemKind: x,
                  PartWeightedAverage: x,
                },
              },
            }),
            pull.Part({
              objectId: this.data.associationId,
              include: {
                PartWeightedAverage: x,
              },
            }),
            pull.SerialisedItem({
              objectId: this.data.associationId,
            }),
            pull.SerialisedItem({
              objectId: this.data.associationId,
              select: {
                PartWhereSerialisedItem: {
                  include: {
                    PartWeightedAverage: x,
                  },
                },
              },
            }),
          ];

          return this.allors.context.pull(pulls).pipe(map((loaded) => ({ loaded })));
        })
      )
      .subscribe(({ loaded }) => {
        this.allors.context.reset();

        this.inventoryTransactionReasons = loaded.collection(m.InventoryTransactionReason);
        this.nonSerialisedInventoryItemState = loaded.collection(m.NonSerialisedInventoryItemState);
        this.serialisedInventoryItemState = loaded.collection(m.SerialisedInventoryItemState);
        this.part = loaded.object(m.Part) || loaded.object<Part>(m.SerialisedItem.PartWhereSerialisedItem);
        this.parts = loaded.collection(m.Part);
        this.facilities = loaded.collection(m.Facility);
        this.lots = loaded.collection(m.Lot);
        this.serialisedItem = loaded.object(m.SerialisedItem);
        this.inventoryItem = loaded.object(m.InventoryItem);

        if (this.part) {
          this.selectedPart = this.part;
        }

        if (this.inventoryItem) {
          this.serialisedInventoryItem = loaded.object(m.InventoryItem);
          this.nonSerialisedInventoryItem = loaded.object(m.InventoryItem);
          this.part = this.inventoryItem.Part;
          this.selectedFacility = this.inventoryItem.Facility;
          this.serialised = this.inventoryItem.Part.InventoryItemKind.UniqueId === '2596e2dd-3f5d-4588-a4a2-167d6fbe3fae';
        }

        this.inventoryItemTransaction = this.allors.context.create<InventoryItemTransaction>(m.InventoryItemTransaction);
        this.inventoryItemTransaction.TransactionDate = new Date();
        this.inventoryItemTransaction.Part = this.part;
        this.inventoryItemTransaction.Cost = this.part.PartWeightedAverage?.AverageCost;

        if (this.inventoryItem) {
          this.inventoryItemTransaction.Facility = this.inventoryItem.Facility;
          this.inventoryItemTransaction.UnitOfMeasure = this.inventoryItem.UnitOfMeasure;
          this.inventoryItemTransaction.Lot = this.inventoryItem.Lot;

          if (this.serialised) {
            this.inventoryItemTransaction.SerialisedItem = this.serialisedInventoryItem.SerialisedItem;
            this.inventoryItemTransaction.SerialisedInventoryItemState = this.serialisedInventoryItem.SerialisedInventoryItemState;
          } else {
            this.inventoryItemTransaction.NonSerialisedInventoryItemState = this.nonSerialisedInventoryItem.NonSerialisedInventoryItemState;
          }
        }

        if (this.serialisedItem) {
          this.inventoryItemTransaction.SerialisedItem = this.serialisedItem;
          this.serialised = true;
        }
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.inventoryItemTransaction.Facility = this.selectedFacility;

    this.allors.context.push().subscribe(() => {
      this.dialogRef.close(this.inventoryItemTransaction);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }

  public facilityAdded(facility: Facility): void {
    this.facilities.push(facility);
    this.selectedFacility = facility;
  }
}
