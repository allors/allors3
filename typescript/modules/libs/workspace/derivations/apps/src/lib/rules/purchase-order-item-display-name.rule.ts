import { inlineLists } from 'common-tags';

import { ICycle, IRule, IPattern } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { PurchaseOrderItem } from '@allors/workspace/domain/default';
import { Dependency } from '@allors/workspace/meta/system';

export class PurchaseOrderItemDisplayNameRule implements IRule {
  patterns: IPattern[];
  dependencies: Dependency[];

  constructor(m: M) {
    const { treeBuilder: t, dependency: d } = m;

    this.patterns = [
      {
        kind: 'RolePattern',
        roleType: m.PurchaseOrderItem.Part,
      },
      {
        kind: 'RolePattern',
        roleType: m.Part.Name,
        tree: t.Part({
          PurchaseOrderItemsWherePart: {},
        }),
      },
      {
        kind: 'RolePattern',
        roleType: m.PurchaseOrder.OrderNumber,
        tree: t.PurchaseOrder({
          PurchaseOrderItems: {},
        }),
      },
      {
        kind: 'RolePattern',
        roleType: m.PurchaseOrder.TakenViaSupplier,
        tree: t.PurchaseOrder({
          PurchaseOrderItems: {},
        }),
      },
      {
        kind: 'RolePattern',
        roleType: m.Organisation.PartyName,
        tree: t.Organisation({
          PurchaseOrdersWhereTakenViaSupplier: {
            PurchaseOrderItems: {},
          },
        }),
      },
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
