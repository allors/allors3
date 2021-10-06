import { ICycle, IRule, IPattern } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { InventoryItem } from '@allors/workspace/domain/default';
import { InventoryItemsWherePart } from '../../../../../meta/apps/src/lib/generated/m.g';

export class InventoryItemPartDisplayNameRule implements IRule {
  id= '3d754fffc68a425b8fa94ba60e6b38e0';
  patterns: IPattern[];

  constructor(m: M) {
    const { treeBuilder: t } = m;

    this.patterns = [
      {
        kind: 'RolePattern',
        roleType: m.Part.Name,
        tree: t.Part({
          InventoryItemsWherePart: {},
        }),
      },
    ];
  }

  derive(cycle: ICycle, matches: InventoryItem[]) {
    for (const match of matches) {
      match.PartDisplayName = match.Part?.Name ?? '';
    }
  }
}
