import { ICycle, IRule, IPattern } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { SerialisedItem } from '@allors/workspace/domain/default';
import { Dependency } from '@allors/workspace/meta/system';

export class SerialisedItemDisplayNameRule implements IRule {
  patterns: IPattern[];
  dependencies: Dependency[];

  constructor(m: M) {
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
