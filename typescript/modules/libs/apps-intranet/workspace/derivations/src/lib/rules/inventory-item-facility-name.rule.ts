import { Composite, Dependency, RoleType } from '@allors/system/workspace/meta';
import { IRule } from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import { InventoryItem } from '@allors/default/workspace/domain';
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
