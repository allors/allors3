import { Database } from '@allors/workspace/adapters/system';
import { Tests } from '../Tests';
import { c1B, c2B } from '../Names';
import { Pull } from '@allors/workspace/domain/system';
import { expect } from '@jest/globals';
import { assert_equal } from '../Assert';

export class PullTests extends Tests {
  constructor(database: Database, public login: (login: string) => Promise<boolean>) {
    super(database, login);
  }

  async andGreaterThanLessThan() {
    const session = this.workspace.createSession();
    const m = this.m;

    //  Class
    let pull: Pull = {
      extent: {
        kind: 'Filter',
        objectType: m.C1,
        predicate: {
          kind: 'And',
          operands: [
            {
              kind: 'GreaterThan',
              roleType: m.C1.C1AllorsInteger,
              value: 0,
            },
            {
              kind: 'LessThan',
              roleType: m.C1.C1AllorsInteger,
              value: 2,
            },
          ],
        },
      },
    };

    let result = await session.pull([pull]);

    expect(result.collections.size).toBe(1);
    expect(result.objects.size).toBe(0);
    expect(result.values.size).toBe(0);

    let collection = result.collections.get('C1s');

    assert_equal(collection, [c1B]);

    //  Interface
    pull = {
      extent: {
        kind: 'Filter',
        objectType: m.I12,
        predicate: {
          kind: 'And',
          operands: [
            {
              kind: 'GreaterThan',
              roleType: m.I12.I12AllorsInteger,
              value: 0,
            },
            {
              kind: 'LessThan',
              roleType: m.I12.I12AllorsInteger,
              value: 2,
            },
          ],
        },
      },
    };

    result = await session.pull([pull]);

    expect(result.collections.size).toEqual(1);
    expect(result.objects.size).toBe(0);
    expect(result.values.size).toBe(0);

    collection = result.collections.get('I12s');
    assert_equal(collection, [c1B, c2B]);
  }
}
