import { Meta } from '@allors/meta/generated';
import { assert } from '@allors/meta/system';
import { PurchaseOrderItem } from '@allors/domain/generated';
import { inlineLists } from 'common-tags';
import { Database } from '@allors/workspace/system';

export function extendPurchaseOrderItem(workspace: Database) {
  const m = workspace.metaPopulation as Meta;
  const cls = workspace.constructorByObjectType.get(m.PurchaseOrderItem);
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
