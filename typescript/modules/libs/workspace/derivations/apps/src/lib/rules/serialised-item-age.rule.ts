import { ICycle, IRule, IPattern } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { SerialisedItem } from '@allors/workspace/domain/default';

export class SerialisedItemAgeRule implements IRule {
  id: '671388a5a3ad4bb0987849ca2321614b';
  patterns: IPattern[];

  constructor(m: M) {
    const { treeBuilder: t } = m;

    this.patterns = [
      {
        kind: 'RolePattern',
        roleType: m.SerialisedItem.ManufacturingYear,
      },
    ];
  }

  derive(cycle: ICycle, matches: SerialisedItem[]) {
    for (const match of matches) {
      if (match.canReadPurchasePrice && match.ManufacturingYear != null) {
        match.Age = new Date().getFullYear() - match.ManufacturingYear;
      } else {
        match.Age =  0;
      }    
    }
  }
}
