import { IAsyncDatabaseClient, IReactiveDatabaseClient, IWorkspace, Pull } from '@allors/workspace/domain/system';
import { Fixture, name_c1A, name_c1B, name_c1C, name_c1D, name_c2A, name_c2B, name_c2C, name_c2D } from '../Fixture';
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

export async function associationMany2ManyExists() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  //  Full
  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C2,
      predicate: {
        kind: 'Exists',
        propertyType: m.C2.C1sWhereC1C2Many2Many,
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2B, name_c2C, name_c2D]);
}

export async function associationMany2OneContainedIn() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  //  Full
  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C2,
      predicate: {
        kind: 'ContainedIn',
        propertyType: m.C2.C1sWhereC1C2Many2One,
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

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2B]);
}

export async function associationMany2OneContains() {
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
        propertyType: m.C2.C1sWhereC1C2Many2One,
        object: c1c,
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2C]);
}

export async function associationOne2ManyContainedIn() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  //  Full
  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C2,
      predicate: {
        kind: 'ContainedIn',
        propertyType: m.C2.C1WhereC1C2One2Many,
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

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2B]);
}

export async function associationOne2ManyEquals() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const c1b = await fixture.pullC1(session, name_c1B);
  const c1c = await fixture.pullC1(session, name_c1C);

  let pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C2,
      predicate: {
        kind: 'Equals',
        propertyType: m.C2.C1WhereC1C2One2Many,
        object: c1b,
      },
    },
  };

  let result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  let c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2B]);

  pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C2,
      predicate: {
        kind: 'Equals',
        propertyType: m.C2.C1WhereC1C2One2Many,
        object: c1c,
      },
    },
  };

  result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2C, name_c2D]);
}

export async function associationOne2ManyExists() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  //  Class
  let pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C2,
      predicate: {
        kind: 'Exists',
        propertyType: m.C2.C1WhereC1C2One2Many,
      },
    },
  };

  let result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  let c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2B, name_c2C, name_c2D]);

  //  Interface
  pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C2,
      predicate: {
        kind: 'Exists',
        propertyType: m.I2.I1WhereI1I2One2Many,
      },
    },
  };

  result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2B, name_c2C, name_c2D]);
}

export async function associationOne2ManyInstanceof() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  //  Full
  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C2,
      predicate: {
        kind: 'Instanceof',
        propertyType: m.C2.C1WhereC1C2One2Many,
        objectType: m.C1,
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2B, name_c2C, name_c2D]);
}

export async function associationOne2OneContainedIn() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  //  Full
  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C2,
      predicate: {
        kind: 'ContainedIn',
        propertyType: m.C2.C1WhereC1C2One2One,
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

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2B]);
}

export async function associationOne2OneEquals() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const c1b = await fixture.pullC1(session, name_c1B);
  const c1c = await fixture.pullC1(session, name_c1C);

  let pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C2,
      predicate: {
        kind: 'Equals',
        propertyType: m.C2.C1WhereC1C2One2One,
        object: c1b,
      },
    },
  };

  let result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  let c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2B]);

  pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C2,
      predicate: {
        kind: 'Equals',
        propertyType: m.C2.C1WhereC1C2One2One,
        object: c1c,
      },
    },
  };

  result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2C]);
}

export async function associationOne2OneExists() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  //  Class
  let pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Exists',
        propertyType: m.C1.C1WhereC1C1One2One,
      },
    },
  };

  let result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B, name_c1C, name_c1D]);

  //  Class
  pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C2,
      predicate: {
        kind: 'Exists',
        propertyType: m.C2.C1WhereC1C2One2One,
      },
    },
  };

  result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2B, name_c2C, name_c2D]);
}

export async function associationOne2OneInstanceof() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  // Class
  let pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.I12,
      predicate: {
        kind: 'Instanceof',
        propertyType: m.I12.I12WhereI12I12One2One,
        objectType: m.C1,
      },
    },
  };

  let result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  let i12s = result.collection(m.I12);

  expect(i12s).toEqualObjects([name_c1D, name_c2B, name_c2C]);

  // Interface
  pull = {
    extent: {
      kind: 'Filter',
      objectType: m.I12,
      predicate: {
        kind: 'Instanceof',
        propertyType: m.I12.I12WhereI12I12One2One,
        objectType: m.I2,
      },
    },
  };

  result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  i12s = result.collection(m.I12);

  expect(i12s).toEqualObjects([name_c1B, name_c1C, name_c2D]);
}

export async function objectEquals() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const c1c = await fixture.pullC1(session, name_c1C);

  //  Full
  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Equals',
        object: c1c,
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1C]);
}

export async function extentInterface() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.I12,
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const i12s = result.collection(m.I12);

  expect(i12s).toEqualObjects([name_c1A, name_c1B, name_c1C, name_c1D, name_c2A, name_c2B, name_c2C, name_c2D]);
}

export async function instanceof_() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.I12,
      predicate: {
        kind: 'Instanceof',
        objectType: m.C1,
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const i12s = result.collection(m.I12);

  expect(i12s).toEqualObjects([name_c1A, name_c1B, name_c1C, name_c1D]);
}

export async function notEquals() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const c1c = await fixture.pullC1(session, name_c1C);

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Not',
        operand: {
          kind: 'Equals',
          object: c1c,
        },
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1A, name_c1B, name_c1D]);
}

export async function orEquals() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const c1b = await fixture.pullC1(session, name_c1B);
  const c1c = await fixture.pullC1(session, name_c1C);

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Or',
        operands: [
          {
            kind: 'Equals',
            object: c1b,
          },
          {
            kind: 'Equals',
            object: c1c,
          },
        ],
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B, name_c1C]);
}

export async function operatorExcept() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Except',
      operands: [
        {
          kind: 'Filter',
          objectType: m.I12,
        },
        {
          kind: 'Filter',
          objectType: m.I12,
          predicate: {
            kind: 'Instanceof',
            objectType: m.C2,
          },
        },
      ],
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const i12s = result.collection(m.I12);

  expect(i12s).toEqualObjects([name_c1A, name_c1B, name_c1C, name_c1D]);
}

export async function operatorIntersect() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Intersect',
      operands: [
        {
          kind: 'Filter',
          objectType: m.I12,
        },
        {
          kind: 'Filter',
          objectType: m.I12,
          predicate: {
            kind: 'Instanceof',
            objectType: m.C2,
          },
        },
      ],
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const i12s = result.collection(m.I12);

  expect(i12s).toEqualObjects([name_c2A, name_c2B, name_c2C, name_c2D]);
}

export async function operatorUnion() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Union',
      operands: [
        {
          kind: 'Filter',
          objectType: m.C1,
          predicate: {
            kind: 'Equals',
            propertyType: m.C1.Name,
            value: 'c1A',
          },
        },
        {
          kind: 'Filter',
          objectType: m.C1,
          predicate: {
            kind: 'Equals',
            propertyType: m.C1.Name,
            value: 'c1B',
          },
        },
      ],
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1A, name_c1B]);
}

export async function roleDateTimeBetweenPath() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Between',
        roleType: m.C1.C1AllorsDateTime,
        paths: [m.C1.C1DateTimeBetweenA, m.C1.C1DateTimeBetweenB],
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1C, name_c1D]);
}

export async function roleDateTimeBetweenValue() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Between',
        roleType: m.C1.C1AllorsDateTime,
        values: [new Date('Sat Jan 01 2000 00:00:04 GMT+0000'), new Date('Sat Jan 01 2000 00:00:06 GMT+0000')],
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B, name_c1C, name_c1D]);
}

export async function roleDateTimeGreaterThanPath() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'GreaterThan',
        roleType: m.C1.C1AllorsDateTime,
        path: m.C1.C1DateTimeGreaterThan,
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B]);
}

export async function roleDateTimeGreaterThanValue() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'GreaterThan',
        roleType: m.C1.C1AllorsDateTime,
        value: new Date('Sat Jan 01 2000 00:00:04 GMT+0000'),
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1C, name_c1D]);
}

export async function roleDateTimeLessThanPath() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'LessThan',
        roleType: m.C1.C1AllorsDateTime,
        path: m.C1.C1DateTimeLessThan,
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1D]);
}

export async function roleDateTimeLessThanValue() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'LessThan',
        roleType: m.C1.C1AllorsDateTime,
        value: new Date('Sat Jan 01 2000 00:00:05 GMT+0000'),
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B]);
}

export async function roleDateTimeEquals() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Equals',
        propertyType: m.C1.C1AllorsDateTime,
        value: new Date('Sat Jan 01 2000 00:00:04 GMT+0000'),
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B]);
}

export async function roleDecimalBetweenPath() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Between',
        roleType: m.C1.C1AllorsDecimal,
        paths: [m.C1.C1DecimalBetweenA, m.C1.C1DecimalBetweenB],
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1D]);
}

export async function roleDecimalBetweenValue() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Between',
        roleType: m.C1.C1AllorsDecimal,
        values: ['2.1', '2.3'],
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1C, name_c1D]);
}

export async function roleDecimalGreaterThanPath() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'GreaterThan',
        roleType: m.C1.C1AllorsDecimal,
        path: m.C1.C1DecimalGreaterThan,
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B, name_c1C]);
}

export async function roleDecimalGreaterThanValue() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'GreaterThan',
        roleType: m.C1.C1AllorsDecimal,
        value: '1.5',
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1C, name_c1D]);
}

export async function roleDecimalLessThanPath() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'LessThan',
        roleType: m.C1.C1AllorsDecimal,
        path: m.C1.C1DecimalLessThan,
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1D]);
}

export async function roleDecimalLessThanValue() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'LessThan',
        roleType: m.C1.C1AllorsDecimal,
        value: '1.9',
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B]);
}

export async function roleDecimalEquals() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Equals',
        propertyType: m.C1.C1AllorsDecimal,
        value: '2.2',
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1C, name_c1D]);
}

export async function roleDoubleBetweenPath() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Between',
        roleType: m.C1.C1AllorsDouble,
        paths: [m.C1.C1DoubleBetweenA, m.C1.C1DoubleBetweenB],
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1D]);
}

export async function roleDoubleBetweenValue() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Between',
        roleType: m.C1.C1AllorsDouble,
        values: [2.1, 2.3],
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1C, name_c1D]);
}

export async function roleDoubleGreaterThanPath() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'GreaterThan',
        roleType: m.C1.C1AllorsDouble,
        path: m.C1.C1DoubleGreaterThan,
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B, name_c1C]);
}

export async function roleDoubleGreaterThanValue() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'GreaterThan',
        roleType: m.C1.C1AllorsDouble,
        value: 1.5,
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1C, name_c1D]);
}

export async function roleDoubleLessThanPath() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'LessThan',
        roleType: m.C1.C1AllorsDouble,
        path: m.C1.C1DoubleLessThan,
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1D]);
}

export async function roleDoubleLessThanValue() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'LessThan',
        roleType: m.C1.C1AllorsDouble,
        value: 1.9,
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B]);
}

export async function roleDoubleEquals() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Equals',
        propertyType: m.C1.C1AllorsDouble,
        value: 2.2,
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1C, name_c1D]);
}

export async function roleIntegerBetweenPath() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Between',
        roleType: m.C1.C1AllorsInteger,
        paths: [m.C1.C1IntegerBetweenA, m.C1.C1IntegerBetweenB],
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1C, name_c1D]);
}

export async function roleIntegerBetweenValue() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Between',
        roleType: m.C1.C1AllorsInteger,
        values: [1, 2],
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B, name_c1C, name_c1D]);
}

export async function roleIntegerGreaterThanPath() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'GreaterThan',
        roleType: m.C1.C1AllorsInteger,
        path: m.C1.C1IntegerGreaterThan,
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B]);
}

export async function roleIntegerGreaterThanValue() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'GreaterThan',
        roleType: m.C1.C1AllorsInteger,
        value: 1,
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1C, name_c1D]);
}

export async function roleIntegerLessThanPath() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'LessThan',
        roleType: m.C1.C1AllorsInteger,
        path: m.C1.C1IntegerLessThan,
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1D]);
}

export async function roleIntegerLessThanValue() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'LessThan',
        roleType: m.C1.C1AllorsInteger,
        value: 2,
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B]);
}

export async function roleIntegerEquals() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Equals',
        propertyType: m.C1.C1AllorsInteger,
        value: 2,
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1C, name_c1D]);
}

export async function roleIntegerExist() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Exists',
        propertyType: m.C1.C1AllorsInteger,
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B, name_c1C, name_c1D]);
}

export async function roleStringEqualsPath() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Equals',
        propertyType: m.C1.C1AllorsString,
        path: m.C1.C1AllorsStringEquals,
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1C]);
}

export async function roleStringEqualsValue() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Equals',
        propertyType: m.C1.C1AllorsString,
        value: 'ᴀbra',
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B]);
}

export async function roleStringLike() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Like',
        roleType: m.C1.C1AllorsString,
        value: 'ᴀ%',
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B, name_c1C, name_c1D]);
}

export async function roleUniqueEquals() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Equals',
        propertyType: m.C1.C1AllorsUnique,
        value: '8B3C4978-72D3-40BA-B302-114EB331FE04',
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B]);
}

export async function roleMany2ManyContainedIn() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  //  Empty
  let pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'ContainedIn',
        propertyType: m.C1.C1I12Many2Manies,
        extent: {
          kind: 'Filter',
          objectType: m.I12,
          predicate: {
            kind: 'Equals',
            propertyType: m.I12.I12AllorsString,
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
      objectType: m.C1,
      predicate: {
        kind: 'ContainedIn',
        propertyType: m.C1.C1I12Many2Manies,
        extent: {
          kind: 'Filter',
          objectType: m.I12,
        },
      },
    },
  };

  result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  let c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B, name_c1C, name_c1D]);

  //  Filtered
  pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'ContainedIn',
        propertyType: m.C1.C1I12Many2Manies,
        extent: {
          kind: 'Filter',
          objectType: m.I12,
          predicate: {
            kind: 'Equals',
            propertyType: m.I12.I12AllorsString,
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

  c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B, name_c1C, name_c1D]);
}

export async function roleMany2ManyContains() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const c2c = await fixture.pullC2(session, name_c2C);

  //  Empty
  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Contains',
        propertyType: m.C1.C1C2Many2Manies,
        object: c2c,
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1C, name_c1D]);
}

export async function roleOne2ManyContainedIn() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'ContainedIn',
        propertyType: m.C1.C1I12One2Manies,
        extent: {
          kind: 'Filter',
          objectType: m.I12,
          predicate: {
            kind: 'Equals',
            propertyType: m.I12.I12AllorsString,
            value: 'ᴀbra',
          },
        },
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B]);
}

export async function roleOne2ManyContains() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const c2d = await fixture.pullC2(session, name_c2D);

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Contains',
        propertyType: m.C1.C1C2One2Manies,
        object: c2d,
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1C]);
}

export async function roleMany2OneContainedIn() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'ContainedIn',
        propertyType: m.C1.C1I12Many2One,
        extent: {
          kind: 'Filter',
          objectType: m.I12,
          predicate: {
            kind: 'Equals',
            propertyType: m.I12.I12AllorsString,
            value: 'ᴀbra',
          },
        },
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B]);
}

export async function roleOne2OneContainedIn() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'ContainedIn',
        propertyType: m.C1.C1I12One2One,
        extent: {
          kind: 'Filter',
          objectType: m.I12,
          predicate: {
            kind: 'Equals',
            propertyType: m.I12.I12AllorsString,
            value: 'ᴀbra',
          },
        },
      },
    },
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B, name_c1C]);
}

export async function withResultName() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Equals',
        propertyType: m.C1.C1AllorsInteger,
        value: 2,
      },
    },
    results: [
      {
        name: 'IetsAnders',
      },
    ],
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection('IetsAnders');

  expect(c1s).toEqualObjects([name_c1C, name_c1D]);
}

export async function pullWithObjectId() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const c1a_1 = await fixture.pullC1(session, name_c1A);

  const pull: Pull = {
    objectId: c1a_1.id,
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(0);
  expect(result.objects.size).toBe(1);
  expect(result.values.size).toBe(0);

  const c1a_2 = result.object(m.C1);

  expect([c1a_2]).toEqualObjects([name_c1A]);
}

export async function pullWithInclude() {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
    },
    results: [
      {
        select: {
          include: [
            {
              kind: 'Node',
              propertyType: m.C1.C1C2One2One,
            },
          ],
        },
      },
    ],
  };

  const result = await client.pullAsync(session, [pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection<C1>(m.C1);

  const c1b = c1s.find((v) => v.Name === name_c1B);
  const c1c = c1s.find((v) => v.Name === name_c1C);
  const c1d = c1s.find((v) => v.Name === name_c1D);

  expect(c1b.C1C2One2One.Name).toBe(name_c2B);
  expect(c1c.C1C2One2One.Name).toBe(name_c2C);
  expect(c1d.C1C2One2One.Name).toBe(name_c2D);
}
