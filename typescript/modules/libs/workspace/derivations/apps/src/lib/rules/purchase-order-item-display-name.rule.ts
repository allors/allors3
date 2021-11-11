import { inlineLists } from 'common-tags';

import { ICycle, IRule, IPattern, pattern as p } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { PurchaseOrderItem } from '@allors/workspace/domain/default';
import { Dependency } from '@allors/workspace/meta/system';

export class PurchaseOrderItemDisplayNameRule implements IRule {
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    const { treeBuilder: t, dependency: d } = m;

    this.patterns = [
      p(m.PurchaseOrderItem, (v) => v.Part),
      p(
        m.Part,
        (v) => v.Name,
        t.Part({
          PurchaseOrderItemsWherePart: {},
        })
      ),
      p(
        m.PurchaseOrder,
        (v) => v.OrderNumber,
        t.PurchaseOrder({
          PurchaseOrderItems: {},
        })
      ),
      p(
        m.PurchaseOrder,
        (v) => v.TakenViaSupplier,
        t.PurchaseOrder({
          PurchaseOrderItems: {},
        })
      ),
      p(
        m.Organisation,
        (v) => v.PartyName,
        t.Organisation({
          PurchaseOrdersWhereTakenViaSupplier: {
            PurchaseOrderItems: {},
          },
        })
      ),
    ];

    this.dependencies = [d(m.PurchaseOrderItem, (v) => v.PurchaseOrderWherePurchaseOrderItem), d(m.PurchaseOrder, (v) => v.TakenViaSupplier), d(m.PurchaseOrderItem, (v) => v.Part)];
  }

  derive(cycle: ICycle, matches: PurchaseOrderItem[]) {
    for (const match of matches) {
      const purchaseOrder = match.PurchaseOrderWherePurchaseOrderItem;
      match.DisplayName = inlineLists`${[purchaseOrder?.OrderNumber, purchaseOrder?.TakenViaSupplier?.PartyName, match.Part?.Name].filter((v) => v)}`;
    }
  }
}
