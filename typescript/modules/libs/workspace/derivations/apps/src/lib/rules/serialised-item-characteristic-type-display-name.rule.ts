import { ICycle, IRule, IPattern, pattern as p } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { SerialisedItemCharacteristicType } from '@allors/workspace/domain/default';
import { Dependency } from '@allors/workspace/meta/system';

export class SerialisedItemCharacteristicTypeDisplayNameRule implements IRule {
  patterns: IPattern[];
  dependencies: Dependency[];

  constructor(m: M) {
    const { treeBuilder: t, dependency: d } = m;

    this.patterns = [
      p(m.SerialisedItemCharacteristicType, (v) => v.UnitOfMeasure),
      p(m.SerialisedItemCharacteristicType, (v) => v.Name),
    ];

    this.dependencies = [d(m.SerialisedItemCharacteristicType, (v) => v.UnitOfMeasure)];
  }

  derive(cycle: ICycle, matches: SerialisedItemCharacteristicType[]) {
    for (const match of matches) {
      match.DisplayName = (match.UnitOfMeasure ? match.Name + ' (' + match.UnitOfMeasure.Abbreviation + ')' : match.Name) ?? '';
    }
  }
}
