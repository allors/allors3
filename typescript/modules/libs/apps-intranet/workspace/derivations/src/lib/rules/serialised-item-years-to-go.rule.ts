import { Composite, Dependency, RoleType } from '@allors/system/workspace/meta';
import { IRule } from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import { SerialisedItem, UnifiedGood } from '@allors/default/workspace/domain';

export class SerialisedItemYearsToGoRule implements IRule<SerialisedItem> {
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    const { dependency: d } = m;

    this.objectType = m.SerialisedItem;
    this.roleType = m.SerialisedItem.YearsToGo;

    this.dependencies = [d(m.SerialisedItem, (v) => v.PartWhereSerialisedItem)];
  }

  derive(serialisedItem: SerialisedItem) {
    const good = serialisedItem.PartWhereSerialisedItem as UnifiedGood | null;

    if (
      serialisedItem.canReadPurchasePrice &&
      serialisedItem.ManufacturingYear != null &&
      good?.LifeTime != null
    ) {
      return good.LifeTime - serialisedItem.Age < 0
        ? 0
        : good.LifeTime - serialisedItem.Age;
    }

    return 0;
  }
}
