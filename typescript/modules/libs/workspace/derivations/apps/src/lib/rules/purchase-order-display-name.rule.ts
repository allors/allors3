import { Composite, Dependency, RoleType } from '@allors/workspace/meta/system';
import { IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { PurchaseOrder } from '@allors/workspace/domain/default';

export class PurchaseOrderDisplayNameRule implements IRule<PurchaseOrder> {
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    const { dependency: d } = m;

    this.objectType = m.PurchaseOrder;
    this.roleType = m.PurchaseOrder.DisplayName;

    this.dependencies = [d(m.PurchaseOrder, (v) => v.TakenViaSupplier)];
  }

  derive(purchaseOrder: PurchaseOrder) {
    return (purchaseOrder.OrderNumber ?? '') + (purchaseOrder.TakenViaSupplier?.PartyName ? ` ${purchaseOrder.TakenViaSupplier?.PartyName}` : '');
  }
}
