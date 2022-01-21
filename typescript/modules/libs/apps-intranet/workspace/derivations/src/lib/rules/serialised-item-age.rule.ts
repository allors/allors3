import { Composite, Dependency, RoleType } from '@allors/system/workspace/meta';
import { IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { SerialisedItem } from '@allors/workspace/domain/default';

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
      serialisedItem.canReadPurchasePrice &&
      serialisedItem.ManufacturingYear != null
    ) {
      return new Date().getFullYear() - serialisedItem.ManufacturingYear;
    }

    return 0;
  }
}
