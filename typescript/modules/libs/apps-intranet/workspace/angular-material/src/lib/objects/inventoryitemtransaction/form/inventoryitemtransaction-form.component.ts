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
  InternalOrganisation,
  InventoryItemTransaction,
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
export class InventoryItemTransactionFormComponent
  extends AllorsFormComponent<InventoryItemTransaction>
  implements CreateOrEditPullHandler, EditIncludeHandler, PostCreatePullHandler
{
  readonly m: M;

  title = 'Add Inventory Item Transaction';

  inventoryItem: InventoryItem;
  inventoryItemTransaction: InventoryItemTransaction;
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

  private subscription: Subscription;
  nonSerialisedInventoryItem: NonSerialisedInventoryItem;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService
  ) {
    super(allors, errorService, form);
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

          return this.allors.context
            .pull(pulls)
            .pipe(map((loaded) => ({ loaded })));
        })
      )
      .subscribe(({ loaded }) => {
        this.allors.context.reset();

        this.inventoryTransactionReasons = loaded.collection(
          m.InventoryTransactionReason
        );
        this.nonSerialisedInventoryItemState = loaded.collection(
          m.NonSerialisedInventoryItemState
        );
        this.serialisedInventoryItemState = loaded.collection(
          m.SerialisedInventoryItemState
        );
        this.part =
          loaded.object(m.Part) ||
          loaded.object<Part>(m.SerialisedItem.PartWhereSerialisedItem);
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
          this.serialised =
            this.inventoryItem.Part.InventoryItemKind.UniqueId ===
            '2596e2dd-3f5d-4588-a4a2-167d6fbe3fae';
        }

        this.inventoryItemTransaction =
          this.allors.context.create<InventoryItemTransaction>(
            m.InventoryItemTransaction
          );
        this.inventoryItemTransaction.TransactionDate = new Date();
        this.inventoryItemTransaction.Part = this.part;
        this.inventoryItemTransaction.Cost =
          this.part.PartWeightedAverage?.AverageCost;

        if (this.inventoryItem) {
          this.inventoryItemTransaction.Facility = this.inventoryItem.Facility;
          this.inventoryItemTransaction.UnitOfMeasure =
            this.inventoryItem.UnitOfMeasure;
          this.inventoryItemTransaction.Lot = this.inventoryItem.Lot;

          if (this.serialised) {
            this.inventoryItemTransaction.SerialisedItem =
              this.serialisedInventoryItem.SerialisedItem;
            this.inventoryItemTransaction.SerialisedInventoryItemState =
              this.serialisedInventoryItem.SerialisedInventoryItemState;
          } else {
            this.inventoryItemTransaction.NonSerialisedInventoryItemState =
              this.nonSerialisedInventoryItem.NonSerialisedInventoryItemState;
          }
        }

        if (this.serialisedItem) {
          this.inventoryItemTransaction.SerialisedItem = this.serialisedItem;
          this.serialised = true;
        }
      });
  }

  public override save(): void {
    this.inventoryItemTransaction.Facility = this.selectedFacility;
    super.save();
  }

  public facilityAdded(facility: Facility): void {
    this.facilities.push(facility);
    this.selectedFacility = facility;
  }
}
