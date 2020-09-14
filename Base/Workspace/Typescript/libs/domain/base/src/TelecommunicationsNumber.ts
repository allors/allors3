import { Meta } from '@allors/meta/generated';
import { assert } from '@allors/meta/system';
import { TelecommunicationsNumber } from '@allors/domain/generated';
import { inlineLists } from 'common-tags';
import { Database } from '@allors/workspace/system';

export function extendTelecommunicationsNumber(database: Database) {

  const m = database.metaPopulation as Meta;
  const cls = database.constructorByObjectType.get(m.TelecommunicationsNumber);
  assert(cls);

  Object.defineProperty(cls?.prototype, 'displayName', {
    configurable: true,
    get(this: TelecommunicationsNumber) {
      return inlineLists`${[this.CountryCode, this.AreaCode, this.ContactNumber].filter(v => v)}`;
    },
  });

};
