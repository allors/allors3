import { Composite, Dependency, RoleType } from '@allors/system/workspace/meta';
import { IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { InventoryItem } from '@allors/workspace/domain/default';
export class InventoryItemFacilityNameRule implements IRule<InventoryItem> {
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    const { dependency: d } = m;

    this.objectType = m.InventoryItem;
    this.roleType = m.InventoryItem.FacilityName;

    this.dependencies = [d(m.InventoryItem, (v) => v.Facility)];
  }

  derive(inventoryItem: InventoryItem) {
    return inventoryItem.Facility?.Name ?? '';
  }
}
