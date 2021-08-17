import { IAsyncDatabaseClient, IObject, IReactiveDatabaseClient, ISession, IWorkspace, Pull } from '@allors/workspace/domain/system';
import { Fixture, name_c1B, name_c1C, name_c2B, name_c2C, name_c2D } from '../Fixture';
import '../Matchers';
import '@allors/workspace/domain/core';
import { C1, C2 } from '@allors/workspace/domain/core';

let fixture: Fixture;

it('dummy', () => {
  // otherwise jest will complain that there are no specs
  expect(true).toBeTruthy();
});

export async function initPull(asyncClient: IAsyncDatabaseClient, reactiveClient: IReactiveDatabaseClient, workspace: IWorkspace, login: (login: string) => Promise<boolean>) {
  fixture = new Fixture(asyncClient, reactiveClient, workspace, login);
}

export async function andGreaterThanLessThan() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

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

  let result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

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

  result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const i12s = result.collection(m.I12);
  expect(i12s).toEqualObjects([name_c1B, name_c2B]);
}

export async function associationMany2ManyContainedIn() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  //  Empty
  let pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C2,
      predicate: {
        kind: 'ContainedIn',
        propertyType: m.C2.C1sWhereC1C2Many2Many,
        extent: {
          kind: 'Filter',
          objectType: m.C1,
          predicate: {
            kind: 'Equals',
            propertyType: m.C1.C1AllorsString,
            value: 'Nothing here!',
          },
        },
      },
    },
  };

  let result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(0);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  //  Full
  pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C2,
      predicate: {
        kind: 'ContainedIn',
        propertyType: m.C2.C1sWhereC1C2Many2Many,
        extent: {
          kind: 'Filter',
          objectType: m.C1,
        },
      },
    },
  };

  result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  let c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2B, name_c2C, name_c2D]);

  //  Filtered
  pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C2,
      predicate: {
        kind: 'ContainedIn',
        propertyType: m.C2.C1sWhereC1C2Many2Many,
        extent: {
          kind: 'Filter',
          objectType: m.C1,
          predicate: {
            kind: 'Equals',
            propertyType: m.C1.C1AllorsString,
            value: 'ᴀbra',
          },
        },
      },
    },
  };

  result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2B]);
}

export async function associationMany2ManyContains() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const c1c = await fixture.pullC1(session, name_c1C);

  //  Full
  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C2,
      predicate: {
        kind: 'Contains',
        propertyType: m.C2.C1sWhereC1C2Many2Many,
        object: c1c,
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2B, name_c2C]);
}
