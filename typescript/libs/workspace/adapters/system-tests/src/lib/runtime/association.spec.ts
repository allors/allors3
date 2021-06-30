import { Database } from '@allors/workspace/adapters/system';
import { Pull } from '@allors/workspace/domain/system';
import { Fixture, name_c1C, name_c2C } from '../Fixture';
import '../Matchers';
import '@allors/workspace/domain/core';
import { C1 } from '@allors/workspace/domain/core';

let fixture: Fixture;

export async function initAssociation(database: Database, login: (login: string) => Promise<boolean>) {
  fixture = new Fixture(database, login);
}

export async function databaseGetOne2Many() {
  const session = fixture.workspace.createSession();
  const m = fixture.m;

  //  Class
  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C2,
    },
    results: [
      {
        select: {
          include: [
            {
              kind: 'Node',
              propertyType: m.C2.C1WhereC1C2One2Many,
            },
          ],
        },
      },
    ],
  };

  const result = await session.pull([pull]);

  const c2s = result.collection('C2s');

  const c2C = c2s.find((v) => v.Name === name_c2C);

  const c1WhereC1C2One2Many = c2C.C1WhereC1C2One2Many;

  expect(c1WhereC1C2One2Many).toBeDefined();
  expect(c1WhereC1C2One2Many.Name).toEqual(name_c1C);
}
