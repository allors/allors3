import { Meta } from '@allors/meta/generated';
import { assert } from '@allors/meta/core';
import { PurchaseOrderItem } from '@allors/domain/generated';
import { inlineLists } from 'common-tags';
import { Database } from '@allors/workspace/core';

export function extendPurchaseOrderItem(database: Database) {
  const m = database.metaPopulation as Meta;
  const cls = database.constructorByObjectType.get(m.PurchaseOrderItem);
  assert(cls);

  Object.defineProperty(cls.prototype, 'displayName', {
    configurable: true,
    get(this: PurchaseOrderItem): string {
      const purchaseOrder = this.PurchaseOrderWherePurchaseOrderItem;
      return inlineLists`${[
        purchaseOrder?.OrderNumber,
        purchaseOrder?.TakenViaSupplier?.PartyName,
        this.Part?.Name,
      ].filter((v) => v)}`;
    },
  });
}
