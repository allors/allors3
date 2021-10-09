import { ICycle, IRule, IPattern } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { InventoryItem } from '@allors/workspace/domain/default';
import { Dependency } from '@allors/workspace/meta/system';

export class InventoryItemFacilityNameRule implements IRule {
  patterns: IPattern[];
  dependencies: Dependency[];

  constructor(m: M) {
    const { treeBuilder: t, dependency: d } = m;

    this.patterns = [
      {
        kind: 'RolePattern',
        roleType: m.InventoryItem.Facility,
      },
      {
        kind: 'RolePattern',
        roleType: m.Facility.Name,
        tree: t.Facility({
          InventoryItemsWhereFacility: {},
        }),
      },
    ];

    this.dependencies = [d(m.InventoryItem, (v) => v.Facility)];
  }

  derive(cycle: ICycle, matches: InventoryItem[]) {
    for (const match of matches) {
      match.FacilityName = match.Facility?.Name ?? '';
    }
  }
}
