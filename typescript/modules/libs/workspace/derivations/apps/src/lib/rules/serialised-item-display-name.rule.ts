import { ICycle, IRule, IPattern } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { SerialisedItem } from '@allors/workspace/domain/default';

export class SerialisedItemDisplayNameRule implements IRule {
  id= 'dd8b9d7209044b6f9fd9ff1115edcd40';
  patterns: IPattern[];

  constructor(m: M) {
    const { treeBuilder: t } = m;

    this.patterns = [
      {
        kind: 'RolePattern',
        roleType: m.SerialisedItem.ItemNumber,
      },
      {
        kind: 'RolePattern',
        roleType: m.SerialisedItem.Name,
      },
      {
        kind: 'RolePattern',
        roleType: m.SerialisedItem.SerialNumber,
      },
    ];
  }

  derive(cycle: ICycle, matches: SerialisedItem[]) {
    for (const match of matches) {
      match.DisplayName = `${match.ItemNumber} ${match.Name} SN: ${match.SerialNumber}`;
    }
  }
}
