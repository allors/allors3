import { C1, I12 } from '@allors/default/workspace/domain';
import { Pull } from '@allors/system/workspace/domain';
import {
  Fixture,
  name_c1A,
  name_c1B,
  name_c1C,
  name_c1D,
  name_c2A,
  name_c2B,
  name_c2C,
  name_c2D,
} from '../fixture';
import '../matchers';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
});

test('andGreaterThanLessThan', async () => {
  const { workspace, m } = fixture;
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

  let result = await session.pull([pull]);

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

  result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const i12s = result.collection<I12>(m.I12);
  expect(i12s).toEqualObjects([name_c1B, name_c2B]);
});

test('associationMany2ManyContainedIn', async () => {
  const { workspace, m } = fixture;
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

  let result = await session.pull([pull]);

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

  result = await session.pull([pull]);

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

  result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2B]);
});

test('associationMany2ManyContains', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2B, name_c2C]);
});

test('associationMany2ManyExists', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2B, name_c2C, name_c2D]);
});

test('associationMany2OneContainedIn', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2B]);
});

test('associationMany2OneContains', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2C]);
});

test('associationOne2ManyContainedIn', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2B]);
});

test('associationOne2ManyEquals', async () => {
  const { workspace, m } = fixture;
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

  let result = await session.pull([pull]);

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

  result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2C, name_c2D]);
});

test('associationOne2ManyExists', async () => {
  const { workspace, m } = fixture;
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

  let result = await session.pull([pull]);

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

  result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2B, name_c2C, name_c2D]);
});

test('associationOne2ManyInstanceof', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2B, name_c2C, name_c2D]);
});

test('associationOne2OneContainedIn', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2B]);
});

test('associationOne2OneEquals', async () => {
  const { workspace, m } = fixture;
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

  let result = await session.pull([pull]);

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

  result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2C]);
});

test('associationOne2OneExists', async () => {
  const { workspace, m } = fixture;
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

  let result = await session.pull([pull]);

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

  result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c2s = result.collection(m.C2);

  expect(c2s).toEqualObjects([name_c2B, name_c2C, name_c2D]);
});

test('associationOne2OneInstanceof', async () => {
  const { workspace, m } = fixture;
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

  let result = await session.pull([pull]);

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

  result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  i12s = result.collection(m.I12);

  expect(i12s).toEqualObjects([name_c1B, name_c1C, name_c2D]);
});

test('objectEquals', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const c1c = await fixture.pullC1(session, name_c1C);

  {
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

    const result = await session.pull([pull]);

    expect(result.collections.size).toBe(1);
    expect(result.objects.size).toBe(0);
    expect(result.values.size).toBe(0);

    const c1s = result.collection(m.C1);

    expect(c1s).toEqualObjects([name_c1C]);
  }

  // TODO:
  // {
  //   const pull: Pull = {
  //     extent: {
  //       kind: 'Filter',
  //       objectType: m.C1,
  //       predicate: {
  //         kind: 'Equals',
  //         parameter: 'obj',
  //       },
  //     },
  //     arguments: { obj: c1c },
  //   };

  //   const result = await session.pull([pull]);

  //   expect(result.collections.size).toBe(1);
  //   expect(result.objects.size).toBe(0);
  //   expect(result.values.size).toBe(0);

  //   const c1s = result.collection(m.C1);

  //   expect(c1s).toEqualObjects([name_c1C]);
  // }
});

test('extentInterface', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.I12,
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const i12s = result.collection(m.I12);

  expect(i12s).toEqualObjects([
    name_c1A,
    name_c1B,
    name_c1C,
    name_c1D,
    name_c2A,
    name_c2B,
    name_c2C,
    name_c2D,
  ]);
});

test('instanceof', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  // {
  //   const pull: Pull = {
  //     extent: {
  //       kind: 'Filter',
  //       objectType: m.I12,
  //       predicate: {
  //         kind: 'Instanceof',
  //         objectType: m.C1,
  //       },
  //     },
  //   };

  //   const result = await session.pull([pull]);

  //   expect(result.collections.size).toBe(1);
  //   expect(result.objects.size).toBe(0);
  //   expect(result.values.size).toBe(0);

  //   const i12s = result.collection(m.I12);

  //   expect(i12s).toEqualObjects([name_c1A, name_c1B, name_c1C, name_c1D]);
  // }

  {
    const pull: Pull = {
      extent: {
        kind: 'Filter',
        objectType: m.I12,
        predicate: {
          kind: 'Instanceof',
          parameter: 'type',
        },
      },
      arguments: { type: m.C1 },
    };

    const result = await session.pull([pull]);

    expect(result.collections.size).toBe(1);
    expect(result.objects.size).toBe(0);
    expect(result.values.size).toBe(0);

    const i12s = result.collection(m.I12);

    expect(i12s).toEqualObjects([name_c1A, name_c1B, name_c1C, name_c1D]);
  }
});

test('notEquals', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1A, name_c1B, name_c1D]);
});

test('orEquals', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B, name_c1C]);
});

test('operatorExcept', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const i12s = result.collection(m.I12);

  expect(i12s).toEqualObjects([name_c1A, name_c1B, name_c1C, name_c1D]);
});

test('operatorIntersect', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const i12s = result.collection(m.I12);

  expect(i12s).toEqualObjects([name_c2A, name_c2B, name_c2C, name_c2D]);
});

test('operatorUnion', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1A, name_c1B]);
});

test('roleDateTimeBetweenPath', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1C, name_c1D]);
});

test('roleDateTimeBetweenValue', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
      predicate: {
        kind: 'Between',
        roleType: m.C1.C1AllorsDateTime,
        values: [
          new Date('Sat Jan 01 2000 00:00:04 GMT+0000'),
          new Date('Sat Jan 01 2000 00:00:06 GMT+0000'),
        ],
      },
    },
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B, name_c1C, name_c1D]);
});

test('roleDateTimeGreaterThanPath', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B]);
});

test('roleDateTimeGreaterThanValue', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1C, name_c1D]);
});

test('roleDateTimeLessThanPath', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1D]);
});

test('roleDateTimeLessThanValue', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B]);
});

test('roleDateTimeEquals', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B]);
});

test('roleDecimalBetweenPath', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1D]);
});

test('roleDecimalBetweenValue', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1C, name_c1D]);
});

test('roleDecimalGreaterThanPath', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B, name_c1C]);
});

test('roleDecimalGreaterThanValue', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1C, name_c1D]);
});

test('roleDecimalLessThanPath', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1D]);
});

test('roleDecimalLessThanValue', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B]);
});

test('roleDecimalEquals', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1C, name_c1D]);
});

test('roleDoubleBetweenPath', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1D]);
});

test('roleDoubleBetweenValue', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1C, name_c1D]);
});

test('roleDoubleGreaterThanPath', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B, name_c1C]);
});

test('roleDoubleGreaterThanValue', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1C, name_c1D]);
});

test('roleDoubleLessThanPath', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1D]);
});

test('roleDoubleLessThanValue', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B]);
});

test('roleDoubleEquals', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1C, name_c1D]);
});

test('roleIntegerBetweenPath', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1C, name_c1D]);
});

test('roleIntegerBetweenValue', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B, name_c1C, name_c1D]);
});

test('roleIntegerGreaterThanPath', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B]);
});

test('roleIntegerGreaterThanValue', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1C, name_c1D]);
});

test('roleIntegerLessThanPath', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1D]);
});

test('roleIntegerLessThanValue', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B]);
});

test('roleIntegerEquals', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1C, name_c1D]);
});

test('roleIntegerExist', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B, name_c1C, name_c1D]);
});

test('roleStringEqualsPath', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1C]);
});

test('roleStringEqualsValue', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  {
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

    const result = await session.pull([pull]);

    expect(result.collections.size).toBe(1);
    expect(result.objects.size).toBe(0);
    expect(result.values.size).toBe(0);

    const c1s = result.collection(m.C1);

    expect(c1s).toEqualObjects([name_c1B]);
  }

  {
    const pull: Pull = {
      extent: {
        kind: 'Filter',
        objectType: m.C1,
        predicate: {
          kind: 'Equals',
          propertyType: m.C1.C1AllorsString,
          parameter: 'str',
        },
      },
      arguments: { str: 'ᴀbra' },
    };

    const result = await session.pull([pull]);

    expect(result.collections.size).toBe(1);
    expect(result.objects.size).toBe(0);
    expect(result.values.size).toBe(0);

    const c1s = result.collection(m.C1);

    expect(c1s).toEqualObjects([name_c1B]);
  }
});

test('roleStringLike', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B, name_c1C, name_c1D]);
});

test('roleUniqueEquals', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B]);
});

test('roleMany2ManyContainedIn', async () => {
  const { workspace, m } = fixture;
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

  let result = await session.pull([pull]);

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

  result = await session.pull([pull]);

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

  result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B, name_c1C, name_c1D]);
});

test('roleMany2ManyContains', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const c2c = await fixture.pullC2(session, name_c2C);

  // {
  //   const pull: Pull = {
  //     extent: {
  //       kind: 'Filter',
  //       objectType: m.C1,
  //       predicate: {
  //         kind: 'Contains',
  //         propertyType: m.C1.C1C2Many2Manies,
  //         object: c2c,
  //       },
  //     },
  //   };

  //   const result = await session.pull([pull]);

  //   expect(result.collections.size).toBe(1);
  //   expect(result.objects.size).toBe(0);
  //   expect(result.values.size).toBe(0);

  //   const c1s = result.collection(m.C1);

  //   expect(c1s).toEqualObjects([name_c1C, name_c1D]);
  // }

  {
    const pull: Pull = {
      extent: {
        kind: 'Filter',
        objectType: m.C1,
        predicate: {
          kind: 'Contains',
          propertyType: m.C1.C1C2Many2Manies,
          parameter: 'obj',
        },
      },
      arguments: { obj: c2c },
    };

    const result = await session.pull([pull]);

    expect(result.collections.size).toBe(1);
    expect(result.objects.size).toBe(0);
    expect(result.values.size).toBe(0);

    const c1s = result.collection(m.C1);

    expect(c1s).toEqualObjects([name_c1C, name_c1D]);
  }
});

test('roleOne2ManyContainedIn', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B]);
});

test('roleOne2ManyContains', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1C]);
});

test('roleMany2OneContainedIn', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  {
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

    const result = await session.pull([pull]);

    expect(result.collections.size).toBe(1);
    expect(result.objects.size).toBe(0);
    expect(result.values.size).toBe(0);

    const c1s = result.collection(m.C1);

    expect(c1s).toEqualObjects([name_c1B]);
  }

  {
    const objectsPull: Pull = {
      extent: {
        kind: 'Filter',
        objectType: m.I12,
        predicate: {
          kind: 'Equals',
          propertyType: m.I12.I12AllorsString,
          value: 'ᴀbra',
        },
      },
    };

    const objectsResult = await session.pull([objectsPull]);
    const objects = objectsResult.collection(m.I12);

    const pull: Pull = {
      extent: {
        kind: 'Filter',
        objectType: m.C1,
        predicate: {
          kind: 'ContainedIn',
          propertyType: m.C1.C1I12Many2One,
          parameter: 'objects',
        },
      },
      arguments: {
        objects,
      },
    };

    const result = await session.pull([pull]);

    expect(result.collections.size).toBe(1);
    expect(result.objects.size).toBe(0);
    expect(result.values.size).toBe(0);

    const c1s = result.collection(m.C1);

    expect(c1s).toEqualObjects([name_c1B]);
  }
});

test('roleOne2OneContainedIn', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection(m.C1);

  expect(c1s).toEqualObjects([name_c1B, name_c1C]);
});

test('withResultName', async () => {
  const { workspace, m } = fixture;
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

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(1);
  expect(result.objects.size).toBe(0);
  expect(result.values.size).toBe(0);

  const c1s = result.collection('IetsAnders');

  expect(c1s).toEqualObjects([name_c1C, name_c1D]);
});

test('pullWithObjectId', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const c1a_1 = await fixture.pullC1(session, name_c1A);

  const pull: Pull = {
    objectId: c1a_1.id,
  };

  const result = await session.pull([pull]);

  expect(result.collections.size).toBe(0);
  expect(result.objects.size).toBe(1);
  expect(result.values.size).toBe(0);

  const c1a_2 = result.object(m.C1);

  expect([c1a_2]).toEqualObjects([name_c1A]);
});

test('pullWithInclude', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const pull: Pull = {
    extent: {
      kind: 'Filter',
      objectType: m.C1,
    },
    results: [
      {
        include: [
          {
            propertyType: m.C1.C1C2One2One,
          },
        ],
      },
    ],
  };

  const result = await session.pull([pull]);

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
});
