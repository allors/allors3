import { Composite, Dependency, RoleType } from '@allors/system/workspace/meta';
import { IRule } from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import { SerialisedItem } from '@allors/default/workspace/domain';

export class SerialisedItemAgeRule implements IRule<SerialisedItem> {
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    this.objectType = m.SerialisedItem;
    this.roleType = m.SerialisedItem.Age;
  }

  derive(serialisedItem: SerialisedItem) {
    if (
      serialisedItem.canReadDerivedPurchasePrice &&
      serialisedItem.ManufacturingYear != null
    ) {
      return new Date().getFullYear() - serialisedItem.ManufacturingYear;
    }

    return 0;
  }
}
