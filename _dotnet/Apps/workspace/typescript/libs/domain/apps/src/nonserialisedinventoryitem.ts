import { Meta } from '@allors/meta/generated';
import { assert } from '@allors/meta/core';
import { NonSerialisedInventoryItem } from '@allors/domain/generated';
import { Database } from '@allors/workspace/core';

export function extendNonSerialisedInventoryItem(database: Database) {

  const m = database.metaPopulation as Meta;
  const cls = database.constructorByObjectType.get(m.NonSerialisedInventoryItem);
  assert(cls);

  Object.defineProperty(cls.prototype, 'facilityName', {
    configurable: true,
    get(this: NonSerialisedInventoryItem): string  {
      return this.Facility?.Name ?? '';
    },
  });

  // Object.defineProperty(cls.prototype, 'quantityOnHand', {
  //   configurable: true,
  //   get(this: NonSerialisedInventoryItem) {
  //     let quantity = 0;

  //     this.InventoryItemTransactionsWhereInventoryItem.forEach((inventoryTransaction: InventoryItemTransaction) => {
  //       const reason = inventoryTransaction.Reason;

  //       if (reason?.IncreasesQuantityOnHand && inventoryTransaction.Quantity) {
  //         if (reason.IncreasesQuantityOnHand) {
  //           quantity += parseFloat(inventoryTransaction.Quantity);
  //         } else {
  //           quantity -= parseFloat(inventoryTransaction.Quantity);
  //         }
  //       }

  //     });

  //     return quantity;
  //   },
  // });

  // Object.defineProperty(cls.prototype, 'quantityCommittedOut', {
  //   configurable: true,
  //   get(this: NonSerialisedInventoryItem) {
  //     let quantity = 0;

  //     this.InventoryItemTransactionsWhereInventoryItem.forEach((inventoryTransaction: InventoryItemTransaction) => {
  //       const reason = inventoryTransaction.Reason;

  //       if (reason?.IncreasesQuantityCommittedOut && inventoryTransaction.Quantity) {
  //         if (reason.IncreasesQuantityCommittedOut) {
  //           quantity += parseFloat(inventoryTransaction.Quantity);
  //         } else {
  //           quantity -= parseFloat(inventoryTransaction.Quantity);
  //         }
  //       }
  //     });

  //     return quantity;
  //   },
  // });

  // Object.defineProperty(cls.prototype, 'availableToPromise', {
  //   configurable: true,
  //   get(this: NonSerialisedInventoryItem) {

  //     let quantity = this.quantityOnHand - this.quantityCommittedOut;

  //     if (quantity < 0) {
  //       quantity = 0;
  //     }

  //     return quantity;
  //   },
  // });

};
