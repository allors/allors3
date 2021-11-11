import { Dependency, RoleType } from '@allors/workspace/meta/system';
import { IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { InventoryItem } from '@allors/workspace/domain/default';
export class InventoryItemFacilityNameRule implements IRule<InventoryItem> {
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    const { dependency: d } = m;

    this.roleType = m.InventoryItem.FacilityName;

    this.dependencies = [d(m.InventoryItem, (v) => v.Facility)];
  }

  derive(inventoryItem: InventoryItem) {
    inventoryItem.FacilityName = inventoryItem.Facility?.Name ?? '';
  }
}
