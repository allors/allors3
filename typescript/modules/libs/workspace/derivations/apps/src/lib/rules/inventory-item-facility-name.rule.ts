import { ICycle, IRule, IPattern, pattern as p } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { InventoryItem } from '@allors/workspace/domain/default';
import { Dependency } from '@allors/workspace/meta/system';

export class InventoryItemFacilityNameRule implements IRule {
  patterns: IPattern[];
  dependencies: Dependency[];

  constructor(m: M) {
    const { treeBuilder: t, dependency: d } = m;

    this.patterns = [
      p(m.InventoryItem, (v) => v.Facility),
      p(
        m.Facility,
        (v) => v.Name,
        t.Facility({
          InventoryItemsWhereFacility: {},
        })
      ),
    ];

    this.dependencies = [d(m.InventoryItem, (v) => v.Facility)];
  }

  derive(cycle: ICycle, matches: InventoryItem[]) {
    for (const match of matches) {
      match.FacilityName = match.Facility?.Name ?? '';
    }
  }
}
