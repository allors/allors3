import { ICycle, IRule, IPattern, pattern as p } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { SerialisedItem } from '@allors/workspace/domain/default';
import { Dependency } from '@allors/workspace/meta/system';

export class SerialisedItemDisplayNameRule implements IRule {
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    this.patterns = [p(m.SerialisedItem, (v) => v.ItemNumber), p(m.SerialisedItem, (v) => v.Name), p(m.SerialisedItem, (v) => v.SerialNumber)];
  }

  derive(cycle: ICycle, matches: SerialisedItem[]) {
    for (const match of matches) {
      match.DisplayName = `${match.ItemNumber} ${match.Name} SN: ${match.SerialNumber}`;
    }
  }
}
