import { ICycle, IRule, IPattern } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { SerialisedItemCharacteristicType } from '@allors/workspace/domain/default';

export class SerialisedItemCharacteristicTypeDisplayNameRule implements IRule {
  id= '9ddd15e0f42649539086a167438ae144';
  patterns: IPattern[];

  constructor(m: M) {
    const { treeBuilder: t } = m;

    this.patterns = [
      {
        kind: 'RolePattern',
        roleType: m.SerialisedItemCharacteristicType.UnitOfMeasure,
      },
      {
        kind: 'RolePattern',
        roleType: m.SerialisedItemCharacteristicType.Name,
      },
    ];
  }

  derive(cycle: ICycle, matches: SerialisedItemCharacteristicType[]) {
    for (const match of matches) {
      match.DisplayName = (match.UnitOfMeasure ? match.Name + ' (' + match.UnitOfMeasure.Abbreviation + ')' : match.Name) ?? '';
    }
  }
}
