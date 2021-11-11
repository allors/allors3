import { Dependency, RoleType } from '@allors/workspace/meta/system';
import { IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { PurchaseOrder } from '@allors/workspace/domain/default';

export class PurchaseOrderDisplayNameRule implements IRule<PurchaseOrder> {
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    const { treeBuilder: t, dependency: d } = m;

    this.roleType = m.PurchaseOrder.DisplayName;

    this.dependencies = [d(m.PurchaseOrder, (v) => v.TakenViaSupplier)];
  }

  derive(purchaseOrder: PurchaseOrder) {
    purchaseOrder.DisplayName = (purchaseOrder.OrderNumber ?? '') + (purchaseOrder.TakenViaSupplier?.PartyName ? ` ${purchaseOrder.TakenViaSupplier?.PartyName}` : '');
  }
}
