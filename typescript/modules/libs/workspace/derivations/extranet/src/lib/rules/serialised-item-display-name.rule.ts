import { Composite, Dependency, RoleType } from '@allors/workspace/meta/system';
import { IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { SerialisedItem } from '@allors/workspace/domain/default';

export class SerialisedItemDisplayNameRule implements IRule<SerialisedItem> {
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    this.objectType = m.SerialisedItem;
    this.roleType = m.SerialisedItem.DisplayName;
  }

  derive(serialisedItem: SerialisedItem) {
    return `${serialisedItem.ItemNumber} ${serialisedItem.Name} SN: ${serialisedItem.SerialNumber}`;
  }
}
