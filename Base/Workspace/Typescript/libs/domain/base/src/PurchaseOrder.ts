import { Meta } from '@allors/meta/generated';
import { assert } from '@allors/meta/system';
import { PurchaseOrder } from '@allors/domain/generated';
import { Database } from '@allors/workspace/system';

export function extendPurchaseOrder(database: Database) {
  const m = database.metaPopulation as Meta;
  const cls = database.constructorByObjectType.get(m.PurchaseOrder);
  assert(cls);

  Object.defineProperty(cls.prototype, 'displayName', {
    configurable: true,
    get(this: PurchaseOrder): string {
      return (
        (this.OrderNumber ?? '') +
        (this.TakenViaSupplier?.PartyName
          ? ` ${this.TakenViaSupplier?.PartyName}`
          : '')
      );
    },
  });
}
