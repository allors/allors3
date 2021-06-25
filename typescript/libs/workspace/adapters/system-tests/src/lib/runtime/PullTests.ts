import { Database } from '@allors/workspace/adapters/system';
import { Tests } from '../Tests';
import { c1B, c2B } from '../Names';
import { Pull, GreaterThan, LessThan } from '@allors/workspace/domain/system';

import 'jest-extended';
import 'jest-chain';

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
          operands: [{ roleType: m.C1.C1AllorsInteger, value: 0 } as GreaterThan, { roleType: m.C1.C1AllorsInteger, value: 2 } as LessThan],
        },
      },
    };

    let result = await session.pull([pull]);

    expect(result.collections).toHaveLength(1);
    expect(result.objects).toBeEmpty();
    expect(result.values).toBeEmpty();

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
    expect(result.collections).toHaveLength(1);
    expect(result.objects).toBeEmpty();
    expect(result.values).toBeEmpty();

    expect(result.collections).toIncludeAllMembers([c1B, c2B]);
  }
}
