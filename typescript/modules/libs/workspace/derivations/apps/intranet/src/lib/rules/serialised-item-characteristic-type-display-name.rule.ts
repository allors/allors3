import { Composite, Dependency, RoleType } from '@allors/workspace/meta/system';
import { IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { SerialisedItemCharacteristicType } from '@allors/workspace/domain/default';

export class SerialisedItemCharacteristicTypeDisplayNameRule implements IRule<SerialisedItemCharacteristicType> {
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    const { dependency: d } = m;

    this.objectType = m.SerialisedItemCharacteristic;
    this.roleType = m.SerialisedItemCharacteristicType.DisplayName;

    this.dependencies = [d(m.SerialisedItemCharacteristicType, (v) => v.UnitOfMeasure)];
  }

  derive(serialisedItemCharacteristicType: SerialisedItemCharacteristicType) {
    return (serialisedItemCharacteristicType.UnitOfMeasure ? serialisedItemCharacteristicType.Name + ' (' + serialisedItemCharacteristicType.UnitOfMeasure.Abbreviation + ')' : serialisedItemCharacteristicType.Name) ?? '';
  }
}
