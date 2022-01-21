import { Composite, Dependency, RoleType } from '@allors/workspace/meta/system';
import { IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { InventoryItem } from '@allors/workspace/domain/default';

export class InventoryItemPartDisplayNameRule implements IRule<InventoryItem> {
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    const { dependency: d } = m;

    this.objectType = m.InventoryItem;
    this.roleType = m.InventoryItem.PartDisplayName;

    this.dependencies = [d(m.InventoryItem, (v) => v.Part)];
  }

  derive(inventoryItem: InventoryItem) {
    return inventoryItem.Part?.Name ?? '';
  }
}
