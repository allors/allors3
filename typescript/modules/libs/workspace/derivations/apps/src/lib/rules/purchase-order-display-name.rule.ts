import { ICycle, IRule, IPattern } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { PurchaseOrder } from '@allors/workspace/domain/default';
import { Dependency } from '@allors/workspace/meta/system';

export class PurchaseOrderDisplayNameRule implements IRule {
  patterns: IPattern[];
  dependencies: Dependency[];

  constructor(m: M) {
    const { treeBuilder: t, dependency: d } = m;

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

    this.dependencies = [d(m.PurchaseOrder, (v) => v.TakenViaSupplier)];
  }

  derive(cycle: ICycle, matches: PurchaseOrder[]) {
    for (const match of matches) {
      match.DisplayName = (match.OrderNumber ?? '') + (match.TakenViaSupplier?.PartyName ? ` ${match.TakenViaSupplier?.PartyName}` : '');
    }
  }
}
