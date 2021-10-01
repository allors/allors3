import { ICycle, IRule, IPattern } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { InventoryItem } from '@allors/workspace/domain/default';

export class InventoryItemFacilityNameRule implements IRule {
  id= 'b3a6f2cc2ccb43d883e09cd03570c73a';
  patterns: IPattern[];

  constructor(m: M) {
    const { treeBuilder: t } = m;

    this.patterns = [
      {
        kind: 'RolePattern',
        roleType: m.InventoryItem.Facility,
      },
      {
        kind: 'RolePattern',
        roleType: m.Facility.Name,
        tree: t.Facility({
          InventoryItemVersionsWhereFacility: {},
        }),
      },
    ];
  }

  derive(cycle: ICycle, matches: InventoryItem[]) {
    for (const match of matches) {
      match.FacilityName = match.Facility?.Name ?? '';
    }
  }
}
