import { Database } from '@allors/workspace/adapters/system';
import { Pull } from '@allors/workspace/domain/system';
import { resultCollection } from '@allors/workspace/domain/core';
import { Fixture, name_c1B, name_c2B } from '../Fixture';
import '../Matchers';

let fixture: Fixture;

export async function initPull(database: Database, login: (login: string) => Promise<boolean>) {
  fixture = new Fixture(database, login);
  
}

export async function andGreaterThanLessThan() {
  const session = fixture.workspace.createSession();
  const m = fixture.m;

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
  let collection = resultCollection(result, m);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = collection('C1');

  expect(c1s).toEqualObjects([name_c1B]);

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
  collection = resultCollection(result, m);

  expect(result.collections.size).toEqual(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const i12s = collection('I12');
  expect(i12s).toEqualObjects([name_c1B, name_c2B]);
}
