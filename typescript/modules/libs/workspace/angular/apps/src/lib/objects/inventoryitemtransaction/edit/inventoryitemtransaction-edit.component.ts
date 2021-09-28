import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import {
  Part,
  Facility,
  InternalOrganisation,
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
import { ObjectData, RefreshService, SaveService, TestScope } from '@allors/workspace/angular/base';
import { SessionService } from '@allors/workspace/angular/core';
import { IObject } from '@allors/workspace/domain/system';

import { FetcherService } from '../../../services/fetcher/fetcher-service';

@Component({
  templateUrl: './inventoryitemtransaction-edit.component.html',
  providers: [SessionService],
})
export class InventoryItemTransactionEditComponent extends TestScope implements OnInit, OnDestroy {
  readonly m: M;

  title = 'Add Inventory Item Transaction';

  inventoryItem: InventoryItem;
  internalOrganisation: InternalOrganisation;
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
    @Self() public allors: SessionService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<InventoryItemTransactionEditComponent>,

    public refreshService: RefreshService,
    private saveService: SaveService,
    private fetcher: FetcherService
  ) {
    super();

    this.m = this.allors.workspace.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.allors.workspace.configuration.metaPopulation as M;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest(this.refreshService.refresh$)
      .pipe(
        switchMap(() => {
          const pulls = [
            this.fetcher.internalOrganisation,
            pull.Facility({ sorting: [{ roleType: m.Facility.Name }] }),
            pull.Part({}),
            pull.InventoryTransactionReason({}),
            pull.NonSerialisedInventoryItemState({}),
            pull.SerialisedInventoryItemState({}),
            pull.Lot({ sorting: [{ roleType: m.Lot.LotNumber }] }),
            pull.InventoryItem({
              objectId: this.data.associationId,
              include: {
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

          return this.allors.client.pullReactive(this.allors.session, pulls).pipe(map((loaded) => ({ loaded })));
        })
      )
      .subscribe(({ loaded }) => {
        this.allors.session.reset();

        this.inventoryTransactionReasons = loaded.collection<InventoryTransactionReason>(m.InventoryTransactionReason);
        this.nonSerialisedInventoryItemState = loaded.collection<NonSerialisedInventoryItemState>(m.NonSerialisedInventoryItemState);
        this.serialisedInventoryItemState = loaded.collection<SerialisedInventoryItemState>(m.SerialisedInventoryItemState);
        this.part = loaded.object<Part>(m.Part);
        this.parts = loaded.collection<Part>(m.Part);
        this.facilities = loaded.collection<Facility>(m.Facility);
        this.lots = loaded.collection<Lot>(m.Lot);
        this.serialisedItem = loaded.object<SerialisedItem>(m.SerialisedItem);
        this.inventoryItem = loaded.object<InventoryItem>(m.InventoryItem);

        if (this.part) {
          this.selectedPart = this.part;
        }

        if (this.inventoryItem) {
          this.serialisedInventoryItem = loaded.object<InventoryItem>(m.InventoryItem);
          this.nonSerialisedInventoryItem = loaded.object<InventoryItem>(m.InventoryItem);
          this.part = this.inventoryItem.Part;
          this.selectedFacility = this.inventoryItem.Facility;
          this.serialised = this.inventoryItem.Part.InventoryItemKind.UniqueId === '2596e2dd-3f5d-4588-a4a2-167d6fbe3fae';
        }

        this.inventoryItemTransaction = this.allors.session.create<InventoryItemTransaction>(m.InventoryItemTransaction);
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

    this.allors.client.pushReactive(this.allors.session).subscribe(() => {
      const data: IObject = {
        id: this.inventoryItemTransaction.id,
        objectType: this.inventoryItemTransaction.objectType,
      };

      this.dialogRef.close(data);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }

  public facilityAdded(facility: Facility): void {
    this.facilities.push(facility);
    this.selectedFacility = facility;

    this.allors.session.hasChanges = true;
  }
}
