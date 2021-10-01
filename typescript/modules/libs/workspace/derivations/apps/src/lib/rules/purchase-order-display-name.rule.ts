import { ICycle, IRule, IPattern } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { PurchaseOrder } from '@allors/workspace/domain/default';

export class PurchaseOrderDisplayNameRule implements IRule {
  id= 'fa7678bb3abf4414bb2f09e30585f3aa';
  patterns: IPattern[];

  constructor(m: M) {
    const { treeBuilder: t } = m;

    this.patterns = [
      {
        kind: 'RolePattern',
        roleType: m.PurchaseOrder.OrderNumber,
      },
      {
        kind: 'RolePattern',
        roleType: m.PurchaseOrder.TakenViaSupplier,
      },
      {
        kind: 'RolePattern',
        roleType: m.Organisation.PartyName,
        tree: t.Organisation({
          PurchaseOrdersWhereTakenViaSupplier: {},
        }),
      },
    ];
  }

  derive(cycle: ICycle, matches: PurchaseOrder[]) {
    for (const match of matches) {
      match.DisplayName = (match.OrderNumber ?? '') + (match.TakenViaSupplier?.PartyName ? ` ${match.TakenViaSupplier?.PartyName}` : '');
    }
  }
}
