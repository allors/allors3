import { ICycle, IRule, IPattern } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { SerialisedItem, UnifiedGood } from '@allors/workspace/domain/default';
import { Dependency } from '@allors/workspace/meta/system';

export class SerialisedItemYearsToGoRule implements IRule {
  patterns: IPattern[];
  dependencies: Dependency[];

  constructor(m: M) {
    const { treeBuilder: t, dependency: d } = m;

    this.patterns = [
      {
        kind: 'RolePattern',
        roleType: m.SerialisedItem.ManufacturingYear,
      },
      {
        kind: 'RolePattern',
        roleType: m.SerialisedItem.Age,
      },
      {
        kind: 'RolePattern',
        roleType: m.UnifiedGood.LifeTime,
        tree: t.UnifiedGood({
          SerialisedItems: {},
        }),
      },
    ];

    this.dependencies = [d(m.SerialisedItem, (v) => v.PartWhereSerialisedItem)];
  }

  derive(cycle: ICycle, matches: SerialisedItem[]) {
    for (const match of matches) {
      const good = match.PartWhereSerialisedItem as UnifiedGood | null;

      if (match.canReadPurchasePrice && match.ManufacturingYear != null && good?.LifeTime != null) {
        match.YearsToGo = good.LifeTime - match.Age < 0 ? 0 : good.LifeTime - match.Age;
      } else {
        match.YearsToGo = 0;
      }
    }
  }
}
