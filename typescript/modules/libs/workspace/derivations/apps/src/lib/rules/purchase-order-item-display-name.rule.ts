import { inlineLists } from 'common-tags';

import { Composite, Dependency, RoleType } from '@allors/workspace/meta/system';
import { IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { PurchaseOrderItem } from '@allors/workspace/domain/default';

export class PurchaseOrderItemDisplayNameRule implements IRule<PurchaseOrderItem> {
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    const { dependency: d } = m;

    this.objectType = m.PurchaseOrderItem;
    this.roleType = m.PurchaseOrderItem.DisplayName;

    this.dependencies = [d(m.PurchaseOrderItem, (v) => v.PurchaseOrderWherePurchaseOrderItem), d(m.PurchaseOrder, (v) => v.TakenViaSupplier), d(m.PurchaseOrderItem, (v) => v.Part)];
  }

  derive(match: PurchaseOrderItem) {
    const purchaseOrder = match.PurchaseOrderWherePurchaseOrderItem;
    return inlineLists`${[purchaseOrder?.OrderNumber, purchaseOrder?.TakenViaSupplier?.PartyName, match.Part?.Name].filter((v) => v)}`;
  }
}
