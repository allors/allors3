import { Pull, Result, Fetch, Tree, And, Equals, Extent, Node } from '@allors/data/core';
import { PullRequest } from '@allors/protocol/core';
import { Organisation, C1, Person, I1 } from '@allors/domain/generated';

import { Fixture } from '../Fixture';

import 'jest-extended';

describe('Extent', () => {
  let fixture: Fixture;

  beforeEach(async () => {
    fixture = new Fixture();
    await fixture.init('full');
  });

  describe('People', () => {
    it('should return all people', async () => {
      const { m, ctx } = fixture;

      const pulls = [
        new Pull({
          extent: new Extent({
            objectType: m.Person,
          }),
        }),
      ];

      ctx.session.reset();

      const pullRequest = new PullRequest({ pulls });
      const loaded = await ctx.load(pullRequest);

      const people = loaded.collections['People'] as Person[];

      expect(people).toBeArray();
      expect(people).not.toBeEmpty();
      expect(8).toBe(people.length);
    });
  });

  describe('C1 with include tree (C1C2One2One)', () => {
    it('should return all people', async () => {
      const { m, ctx } = fixture;

      const pulls = [
        new Pull({
          extent: new Extent({
            objectType: m.C1,
          }),
          results: [
            new Result({
              fetch: new Fetch({
                include: new Tree({
                  objectType: m.C1,
                  nodes: [
                    new Node({
                      propertyType: m.C1.C1C2One2One,
                    }),
                  ],
                }),
              }),
            }),
          ],
        }),
      ];

      ctx.session.reset();

      const pullRequest = new PullRequest({ pulls });

      const loaded = await ctx.load(pullRequest);

      const c1s = loaded.collections['C1s'] as C1[];

      expect(c1s).toBeArray();
      expect(c1s).toHaveLength(4);

      const c1a = c1s.find((v) => v.Name === 'c1A');
      const c1b = c1s.find((v) => v.Name === 'c1B');
      const c1c = c1s.find((v) => v.Name === 'c1C');
      const c1d = c1s.find((v) => v.Name === 'c1D');

      expect(c1a.C1C2One2One).toBeNull();
      expect(c1b.C1C2One2One?.Name).toEqual('c2B');
      expect(c1c.C1C2One2One?.Name).toEqual('c2C');
      expect(c1d.C1C2One2One?.Name).toEqual('c2D');
    });
  });

  describe('Organisation with tree builder', () => {
    it('should return all organisations', async () => {
      const { m, ctx, tree } = fixture;

      const pulls = [
        new Pull({
          extent: new Extent({
            objectType: m.Organisation,
          }),
          results: [
            new Result({
              fetch: new Fetch({
                include: tree.Organisation({
                  Owner: {},
                }),
              }),
            }),
          ],
        }),
      ];

      ctx.session.reset();

      const loaded = await ctx.load(new PullRequest({ pulls }));

      const organisations = loaded.collections['Organisations'] as Organisation[];

      expect(organisations).toBeArray();
      expect(organisations).not.toBeEmpty();

      organisations.forEach((organisation) => {
        const owner = organisation.Owner;
        if (owner) {
          // TODO:
        }
      });
    });
  });

  describe('I1 with tree (I1I2One2One)', () => {
    it('should return all users', async () => {
      const { m, ctx, tree } = fixture;

      const pulls = [
        new Pull({
          extent: new Extent({
            objectType: m.I1,
          }),
          results: [
            new Result({
              fetch: new Fetch({
                include: tree.I1({
                  I1I2One2One: {},
                }),
              }),
            }),
          ],
        }),
      ];

      ctx.session.reset();

      const loaded = await ctx.load(new PullRequest({ pulls }));

      const i1s = loaded.collections['I1s'] as I1[];

      expect(i1s).toBeArray();
      expect(i1s).not.toBeEmpty();

      const c1a = i1s.find((v) => v.Name === 'c1A');
      const c1b = i1s.find((v) => v.Name === 'c1B');
      const c1c = i1s.find((v) => v.Name === 'c1C');
      const c1d = i1s.find((v) => v.Name === 'c1D');

      expect(c1a.I1I2One2One).toBeNull();
      expect(c1b.I1I2One2One.Name).toEqual('c2B');
      expect(c1c.I1I2One2One.Name).toEqual('c2C');
      expect(c1d.I1I2One2One.Name).toEqual('c2D');
    });
  });

  describe('Organisation with path', () => {
    it('should return all owners', async () => {
      const { m, ctx, fetch } = fixture;

      const pulls = [
        new Pull({
          extent: new Extent({
            objectType: m.Organisation,
          }),
          results: [
            new Result({
              fetch: fetch.Organisation({
                Owner: {},
              }),
            }),
          ],
        }),
      ];

      ctx.session.reset();

      const loaded = await ctx.load(new PullRequest({ pulls }));

      const owners = loaded.collections['Owners'] as Person[];

      expect(owners).toBeArray();
      expect(owners).not.toBeEmpty();
      expect(2).toBe(owners.length);
    });

    it('should return all employees', async () => {
      const { m, ctx, fetch } = fixture;

      const pulls = [
        new Pull({
          extent: new Extent({
            objectType: m.Organisation,
          }),
          results: [
            new Result({
              fetch: fetch.Organisation({
                Employees: {},
              }),
            }),
          ],
        }),
      ];

      ctx.session.reset();

      const loaded = await ctx.load(new PullRequest({ pulls }));

      const employees = loaded.collections['Employees'] as Person[];

      expect(employees).toBeArray();
      expect(employees).not.toBeEmpty();
      expect(3).toBe(employees.length);
    });
  });

  describe('Organisation with typesafe path', () => {
    it('should return all employees', async () => {
      const { m, ctx, fetch } = fixture;

      const pulls = [
        new Pull({
          extent: new Extent(m.Organisation),
          results: [
            new Result({
              fetch: fetch.Organisation({
                Employees: {},
              }),
            }),
          ],
        }),
      ];

      ctx.session.reset();

      const loaded = await ctx.load(new PullRequest({ pulls }));

      const employees = loaded.collections['Employees'] as Person[];

      expect(employees).toBeArray();
      expect(employees).not.toBeEmpty();
      expect(3).toBe(employees.length);
    });
  });

  describe('Organisation with typesafe path and tree', () => {
    it('should return all people', async () => {
      const { m, ctx, fetch } = fixture;

      const pulls = [
        new Pull({
          extent: new Extent(m.Organisation),
          results: [
            new Result({
              fetch: fetch.Organisation({
                Owner: {
                  include: {
                    CycleOne: {},
                  },
                },
              }),
            }),
          ],
        }),
      ];

      ctx.session.reset();

      const loaded = await ctx.load(new PullRequest({ pulls }));

      const owners = loaded.collections['Owners'] as Person[];

      owners.forEach((v) => v.CycleOne);

      expect(owners).toBeArray();
      expect(owners).not.toBeEmpty();
      expect(2).toBe(owners.length);
    });
  });

  describe('with boolean predicate on class', () => {
    it('should return matching objects', async () => {
      const { m, ctx } = fixture;
      const pulls = [
        new Pull({
          extent: new Extent({
            objectType: m.C1,
            predicate: new Equals({
              propertyType: m.C1.C1AllorsBoolean,
              value: true,
            }),
          }),
        }),
      ];

      ctx.session.reset();

      const pullRequest = new PullRequest({ pulls });
      const loaded = await ctx.load(pullRequest);
      const c1s = loaded.collections['C1s'] as C1[];

      expect(c1s).toBeArray();
      expect(c1s).toBeArrayOfSize(1);
      expect(c1s.filter((v) => v.Name === 'c1B')).toBeDefined();
    });
  });

  describe('with boolean predicate on interface', () => {
    it('should return matching objects', async () => {
      const { m, ctx } = fixture;
      const pulls = [
        new Pull({
          extent: new Extent({
            objectType: m.C1,
            predicate: new Equals({
              propertyType: m.C1.I1AllorsBoolean,
              value: true,
            }),
          }),
        }),
      ];

      ctx.session.reset();

      const pullRequest = new PullRequest({ pulls });
      const loaded = await ctx.load(pullRequest);

      const c1s = loaded.collections['C1s'] as C1[];

      expect(c1s).toBeArray();
      expect(c1s).toBeArrayOfSize(1);
      expect(c1s.filter((v) => v.Name === 'c1B')).toBeDefined();
    });
  });

  describe('with date predicate on class', () => {
    it('should return matching objects', async () => {
      const { m, ctx } = fixture;
      const pulls = [
        new Pull({
          extent: new Extent({
            objectType: m.C1,
            predicate: new Equals({
              propertyType: m.C1.C1AllorsDateTime,
              value: '2000-01-01T00:00:04Z',
            }),
          }),
        }),
      ];

      ctx.session.reset();

      const pullRequest = new PullRequest({ pulls });
      const loaded = await ctx.load(pullRequest);
      const c1s = loaded.collections['C1s'] as C1[];

      expect(c1s).toBeArray();
      expect(c1s).toBeArrayOfSize(1);
      expect(c1s.filter((v) => v.Name === 'c1B')).toBeDefined();
    });
  });

  describe('with date predicate on interface', () => {
    it('should return matching objects', async () => {
      const { m, ctx } = fixture;
      const pulls = [
        new Pull({
          extent: new Extent({
            objectType: m.C1,
            predicate: new Equals({
              propertyType: m.C1.I1AllorsDateTime,
              value: '2000-01-01T00:00:04Z',
            }),
          }),
        }),
      ];

      ctx.session.reset();

      const pullRequest = new PullRequest({ pulls });
      const loaded = await ctx.load(pullRequest);
      const c1s = loaded.collections['C1s'] as C1[];

      expect(c1s).toBeArray();
      expect(c1s).toBeArrayOfSize(1);
      expect(c1s.filter((v) => v.Name === 'c1B')).toBeDefined();
    });
  });

  describe('with decimal predicate on class', () => {
    it('should return matching objects', async () => {
      const { m, ctx } = fixture;
      const pulls = [
        new Pull({
          extent: new Extent({
            objectType: m.C1,
            predicate: new Equals({
              propertyType: m.C1.C1AllorsDecimal,
              value: '1.1',
            }),
          }),
        }),
      ];

      ctx.session.reset();

      const pullRequest = new PullRequest({ pulls });
      const loaded = await ctx.load(pullRequest);
      const c1s = loaded.collections['C1s'] as C1[];

      expect(c1s).toBeArray();
      expect(c1s).toBeArrayOfSize(1);
      expect(c1s.filter((v) => v.Name === 'c1B')).toBeDefined();
    });
  });

  describe('with decimal (extra precision) predicate on class', () => {
    it('should return matching objects', async () => {
      const { m, ctx } = fixture;
      const pulls = [
        new Pull({
          extent: new Extent({
            objectType: m.C1,
            predicate: new Equals({
              propertyType: m.C1.C1AllorsDecimal,
              value: '1.10',
            }),
          }),
        }),
      ];

      ctx.session.reset();

      const pullRequest = new PullRequest({ pulls });
      const loaded = await ctx.load(pullRequest);
      const c1s = loaded.collections['C1s'] as C1[];

      expect(c1s).toBeArray();
      expect(c1s).toBeArrayOfSize(1);
      expect(c1s.filter((v) => v.Name === 'c1B')).toBeDefined();
    });
  });

  describe('with decimal predicate on interface', () => {
    it('should return matching objects', async () => {
      const { m, ctx } = fixture;
      const pulls = [
        new Pull({
          extent: new Extent({
            objectType: m.C1,
            predicate: new Equals({
              propertyType: m.C1.I1AllorsDecimal,
              value: '1.1',
            }),
          }),
        }),
      ];

      ctx.session.reset();

      const pullRequest = new PullRequest({ pulls });
      const loaded = await ctx.load(pullRequest);
      const c1s = loaded.collections['C1s'] as C1[];

      expect(c1s).toBeArray();
      expect(c1s).toBeArrayOfSize(1);
      expect(c1s.filter((v) => v.Name === 'c1B')).toBeDefined();
    });
  });

  describe('with double predicate on class', () => {
    it('should return matching objects', async () => {
      const { m, ctx } = fixture;
      const pulls = [
        new Pull({
          extent: new Extent({
            objectType: m.C1,
            predicate: new Equals({
              propertyType: m.C1.C1AllorsDouble,
              value: 1.1,
            }),
          }),
        }),
      ];

      ctx.session.reset();

      const pullRequest = new PullRequest({ pulls });
      const loaded = await ctx.load(pullRequest);
      const c1s = loaded.collections['C1s'] as C1[];

      expect(c1s).toBeArray();
      expect(c1s).toBeArrayOfSize(1);
      expect(c1s.filter((v) => v.Name === 'c1B')).toBeDefined();
    });
  });

  describe('with double predicate on interface', () => {
    it('should return matching objects', async () => {
      const { m, ctx } = fixture;
      const pulls = [
        new Pull({
          extent: new Extent({
            objectType: m.C1,
            predicate: new Equals({
              propertyType: m.C1.I1AllorsDouble,
              value: 1.1,
            }),
          }),
        }),
      ];

      ctx.session.reset();

      const pullRequest = new PullRequest({ pulls });
      const loaded = await ctx.load(pullRequest);

      const c1s = loaded.collections['C1s'] as C1[];

      expect(c1s).toBeArray();
      expect(c1s).toBeArrayOfSize(1);
      expect(c1s.filter((v) => v.Name === 'c1B')).toBeDefined();
    });
  });

  describe('with integer predicate on class', () => {
    it('should return matching objects', async () => {
      const { m, ctx } = fixture;
      const pulls = [
        new Pull({
          extent: new Extent({
            objectType: m.C1,
            predicate: new Equals({
              propertyType: m.C1.C1AllorsInteger,
              value: 1,
            }),
          }),
        }),
      ];

      ctx.session.reset();

      const pullRequest = new PullRequest({ pulls });
      const loaded = await ctx.load(pullRequest);
      const c1s = loaded.collections['C1s'] as C1[];

      expect(c1s).toBeArray();
      expect(c1s).toBeArrayOfSize(1);
      expect(c1s.filter((v) => v.Name === 'c1B')).toBeDefined();
    });
  });

  describe('with integer predicate on interface', () => {
    it('should return matching objects', async () => {
      const { m, ctx } = fixture;
      const pulls = [
        new Pull({
          extent: new Extent({
            objectType: m.C1,
            predicate: new Equals({
              propertyType: m.C1.I1AllorsInteger,
              value: 1,
            }),
          }),
        }),
      ];

      ctx.session.reset();

      const pullRequest = new PullRequest({ pulls });
      const loaded = await ctx.load(pullRequest);

      const c1s = loaded.collections['C1s'] as C1[];

      expect(c1s).toBeArray();
      expect(c1s).toBeArrayOfSize(1);
      expect(c1s.filter((v) => v.Name === 'c1B')).toBeDefined();
    });
  });

  describe('with unique predicate on class', () => {
    it('should return matching objects', async () => {
      const { m, ctx } = fixture;
      const pulls = [
        new Pull({
          extent: new Extent({
            objectType: m.C1,
            predicate: new Equals({
              propertyType: m.C1.C1AllorsUnique,
              value: '8B3C4978-72D3-40BA-B302-114EB331FE04',
            }),
          }),
        }),
      ];

      ctx.session.reset();

      const pullRequest = new PullRequest({ pulls });
      const loaded = await ctx.load(pullRequest);
      const c1s = loaded.collections['C1s'] as C1[];

      expect(c1s).toBeArray();
      expect(c1s).toBeArrayOfSize(1);
      expect(c1s.filter((v) => v.Name === 'c1B')).toBeDefined();
    });
  });

  describe('with unique ({} notation) predicate on class', () => {
    it('should return matching objects', async () => {
      const { m, ctx } = fixture;
      const pulls = [
        new Pull({
          extent: new Extent({
            objectType: m.C1,
            predicate: new Equals({
              propertyType: m.C1.C1AllorsUnique,
              value: '{8B3C4978-72D3-40BA-B302-114EB331FE04}',
            }),
          }),
        }),
      ];

      ctx.session.reset();

      const pullRequest = new PullRequest({ pulls });
      const loaded = await ctx.load(pullRequest);
      const c1s = loaded.collections['C1s'] as C1[];

      expect(c1s).toBeArray();
      expect(c1s).toBeArrayOfSize(1);
      expect(c1s.filter((v) => v.Name === 'c1B')).toBeDefined();
    });
  });

  describe('with unique (no -) predicate on class', () => {
    it('should return matching objects', async () => {
      const { m, ctx } = fixture;
      const pulls = [
        new Pull({
          extent: new Extent({
            objectType: m.C1,
            predicate: new Equals({
              propertyType: m.C1.C1AllorsUnique,
              value: '8B3C497872D340BAB302114EB331FE04',
            }),
          }),
        }),
      ];

      ctx.session.reset();

      const pullRequest = new PullRequest({ pulls });
      const loaded = await ctx.load(pullRequest);
      const c1s = loaded.collections['C1s'] as C1[];

      expect(c1s).toBeArray();
      expect(c1s).toBeArrayOfSize(1);
      expect(c1s.filter((v) => v.Name === 'c1B')).toBeDefined();
    });
  });

  describe('with unique predicate on interface', () => {
    it('should return matching objects', async () => {
      const { m, ctx } = fixture;
      const pulls = [
        new Pull({
          extent: new Extent({
            objectType: m.C1,
            predicate: new Equals({
              propertyType: m.C1.I1AllorsUnique,
              value: '7F7BF8EF-DDF2-47E6-B33F-627BE7DEAD6D',
            }),
          }),
        }),
      ];

      ctx.session.reset();

      const pullRequest = new PullRequest({ pulls });
      const loaded = await ctx.load(pullRequest);

      const c1s = loaded.collections['C1s'] as C1[];

      expect(c1s).toBeArray();
      expect(c1s).toBeArrayOfSize(1);
      expect(c1s.filter((v) => v.Name === 'c1B')).toBeDefined();
    });
  });

  describe('with boolean predicate parameter on class', () => {
    it('should return matching objects', async () => {
      const { m, ctx } = fixture;

      const pulls = [
        new Pull({
          extent: new Extent({
            objectType: m.C1,
            predicate: new And({
              operands: [
                new Equals({
                  propertyType: m.C1.C1AllorsBoolean,
                  parameter: 'p1',
                }),
              ],
            }),
          }),
          parameters: {
            p1: true,
          },
        }),
      ];

      ctx.session.reset();

      const pullRequest = new PullRequest({ pulls });
      const loaded = await ctx.load(pullRequest);

      const c1s = loaded.collections['C1s'] as C1[];

      expect(c1s).toBeArray();
      // expect(c1s).not.toBeEmpty();
      // expect(7).toBe( c1s.length);
    });
  });

  describe('with boolean predicate parameter on interface', () => {
    it('should return matching objects', async () => {
      const { m, ctx } = fixture;

      const pulls = [
        new Pull({
          extent: new Extent({
            objectType: m.C1,
            predicate: new And({
              operands: [
                new Equals({
                  propertyType: m.C1.I1AllorsBoolean,
                  parameter: 'p1',
                }),
              ],
            }),
          }),
          parameters: {
            p1: true,
          },
        }),
      ];

      ctx.session.reset();

      const pullRequest = new PullRequest({ pulls });
      const loaded = await ctx.load(pullRequest);

      const c1s = loaded.collections['C1s'] as C1[];

      expect(c1s).toBeArray();
      // expect(c1s).not.toBeEmpty();
      // expect(7).toBe( c1s.length);
    });
  });

  describe('with date predicate parameter on class', () => {
    it('should return matching objects', async () => {
      const { m, ctx } = fixture;

      const pulls = [
        new Pull({
          extent: new Extent({
            objectType: m.C1,
            predicate: new And({
              operands: [
                new Equals({
                  propertyType: m.C1.C1AllorsDateTime,
                  parameter: 'p1',
                }),
              ],
            }),
          }),
          parameters: {
            p1: '2000-01-01T00:00:04Z',
          },
        }),
      ];

      ctx.session.reset();

      const pullRequest = new PullRequest({ pulls });
      const loaded = await ctx.load(pullRequest);

      const c1s = loaded.collections['C1s'] as C1[];

      expect(c1s).toBeArray();
      // expect(c1s).not.toBeEmpty();
      // expect(7).toBe( c1s.length);
    });
  });

  describe('with date predicate parameter on interface', () => {
    it('should return matching objects', async () => {
      const { m, ctx } = fixture;

      const pulls = [
        new Pull({
          extent: new Extent({
            objectType: m.C1,
            predicate: new And({
              operands: [
                new Equals({
                  propertyType: m.C1.I1AllorsDateTime,
                  parameter: 'p1',
                }),
              ],
            }),
          }),
          parameters: {
            p1: '2000-01-01T00:00:04Z',
          },
        }),
      ];

      ctx.session.reset();

      const pullRequest = new PullRequest({ pulls });
      const loaded = await ctx.load(pullRequest);

      const c1s = loaded.collections['C1s'] as C1[];

      expect(c1s).toBeArray();
      // expect(c1s).not.toBeEmpty();
      // expect(7).toBe( c1s.length);
    });
  });

  describe('with decimal predicate parameter on class', () => {
    it('should return matching objects', async () => {
      const { m, ctx } = fixture;

      const pulls = [
        new Pull({
          extent: new Extent({
            objectType: m.C1,
            predicate: new And({
              operands: [
                new Equals({
                  propertyType: m.C1.C1AllorsDecimal,
                  parameter: 'p1',
                }),
              ],
            }),
          }),
          parameters: {
            p1: '1.1',
          },
        }),
      ];

      ctx.session.reset();

      const pullRequest = new PullRequest({ pulls });
      const loaded = await ctx.load(pullRequest);

      const c1s = loaded.collections['C1s'] as C1[];

      expect(c1s).toBeArray();
      // expect(c1s).not.toBeEmpty();
      // expect(7).toBe( c1s.length);
    });
  });

  describe('with decimal (extra precision) predicate parameter on class', () => {
    it('should return matching objects', async () => {
      const { m, ctx } = fixture;

      const pulls = [
        new Pull({
          extent: new Extent({
            objectType: m.C1,
            predicate: new And({
              operands: [
                new Equals({
                  propertyType: m.C1.C1AllorsDecimal,
                  parameter: 'p1',
                }),
              ],
            }),
          }),
          parameters: {
            p1: '1.10',
          },
        }),
      ];

      ctx.session.reset();

      const pullRequest = new PullRequest({ pulls });
      const loaded = await ctx.load(pullRequest);

      const c1s = loaded.collections['C1s'] as C1[];

      expect(c1s).toBeArray();
      // expect(c1s).not.toBeEmpty();
      // expect(7).toBe( c1s.length);
    });
  });

  describe('with decimal predicate parameter on interface', () => {
    it('should return matching objects', async () => {
      const { m, ctx } = fixture;

      const pulls = [
        new Pull({
          extent: new Extent({
            objectType: m.C1,
            predicate: new And({
              operands: [
                new Equals({
                  propertyType: m.C1.I1AllorsDecimal,
                  parameter: 'p1',
                }),
              ],
            }),
          }),
          parameters: {
            p1: '1.1',
          },
        }),
      ];

      ctx.session.reset();

      const pullRequest = new PullRequest({ pulls });
      const loaded = await ctx.load(pullRequest);

      const c1s = loaded.collections['C1s'] as C1[];

      expect(c1s).toBeArray();
      // expect(c1s).not.toBeEmpty();
      // expect(7).toBe( c1s.length);
    });
  });

  describe('with double predicate parameter on class', () => {
    it('should return matching objects', async () => {
      const { m, ctx } = fixture;

      const pulls = [
        new Pull({
          extent: new Extent({
            objectType: m.C1,
            predicate: new And({
              operands: [
                new Equals({
                  propertyType: m.C1.C1AllorsDouble,
                  parameter: 'p1',
                }),
              ],
            }),
          }),
          parameters: {
            p1: 1.1,
          },
        }),
      ];

      ctx.session.reset();

      const pullRequest = new PullRequest({ pulls });
      const loaded = await ctx.load(pullRequest);

      const c1s = loaded.collections['C1s'] as C1[];

      expect(c1s).toBeArray();
      // expect(c1s).not.toBeEmpty();
      // expect(7).toBe( c1s.length);
    });
  });

  describe('with double predicate parameter on interface', () => {
    it('should return matching objects', async () => {
      const { m, ctx } = fixture;

      const pulls = [
        new Pull({
          extent: new Extent({
            objectType: m.C1,
            predicate: new And({
              operands: [
                new Equals({
                  propertyType: m.C1.I1AllorsDouble,
                  parameter: 'p1',
                }),
              ],
            }),
          }),
          parameters: {
            p1: 1.1,
          },
        }),
      ];

      ctx.session.reset();

      const pullRequest = new PullRequest({ pulls });
      const loaded = await ctx.load(pullRequest);

      const c1s = loaded.collections['C1s'] as C1[];

      expect(c1s).toBeArray();
      // expect(c1s).not.toBeEmpty();
      // expect(7).toBe( c1s.length);
    });
  });

  describe('with integer predicate parameter on class', () => {
    it('should return matching objects', async () => {
      const { m, ctx } = fixture;

      const pulls = [
        new Pull({
          extent: new Extent({
            objectType: m.C1,
            predicate: new And({
              operands: [
                new Equals({
                  propertyType: m.C1.C1AllorsInteger,
                  parameter: 'p1',
                }),
              ],
            }),
          }),
          parameters: {
            p1: 1,
          },
        }),
      ];

      ctx.session.reset();

      const pullRequest = new PullRequest({ pulls });
      const loaded = await ctx.load(pullRequest);

      const c1s = loaded.collections['C1s'] as C1[];

      expect(c1s).toBeArray();
      // expect(c1s).not.toBeEmpty();
      // expect(7).toBe( c1s.length);
    });
  });

  describe('with integer predicate parameter on interface', () => {
    it('should return matching objects', async () => {
      const { m, ctx } = fixture;

      const pulls = [
        new Pull({
          extent: new Extent({
            objectType: m.C1,
            predicate: new And({
              operands: [
                new Equals({
                  propertyType: m.C1.I1AllorsInteger,
                  parameter: 'p1',
                }),
              ],
            }),
          }),
          parameters: {
            p1: 1,
          },
        }),
      ];

      ctx.session.reset();

      const pullRequest = new PullRequest({ pulls });
      const loaded = await ctx.load(pullRequest);

      const c1s = loaded.collections['C1s'] as C1[];

      expect(c1s).toBeArray();
      // expect(c1s).not.toBeEmpty();
      // expect(7).toBe( c1s.length);
    });
  });

  describe('with unique predicate parameter on class', () => {
    it('should return matching objects', async () => {
      const { m, ctx } = fixture;

      const pulls = [
        new Pull({
          extent: new Extent({
            objectType: m.C1,
            predicate: new And({
              operands: [
                new Equals({
                  propertyType: m.C1.C1AllorsUnique,
                  parameter: 'p1',
                }),
              ],
            }),
          }),
          parameters: {
            p1: '8B3C4978-72D3-40BA-B302-114EB331FE04',
          },
        }),
      ];

      ctx.session.reset();

      const pullRequest = new PullRequest({ pulls });
      const loaded = await ctx.load(pullRequest);

      const c1s = loaded.collections['C1s'] as C1[];

      expect(c1s).toBeArray();
      // expect(c1s).not.toBeEmpty();
      // expect(7).toBe( c1s.length);
    });
  });

  describe('with unique ({} notation) predicate parameter on class', () => {
    it('should return matching objects', async () => {
      const { m, ctx } = fixture;

      const pulls = [
        new Pull({
          extent: new Extent({
            objectType: m.C1,
            predicate: new And({
              operands: [
                new Equals({
                  propertyType: m.C1.C1AllorsUnique,
                  parameter: 'p1',
                }),
              ],
            }),
          }),
          parameters: {
            p1: '{8B3C4978-72D3-40BA-B302-114EB331FE04}',
          },
        }),
      ];

      ctx.session.reset();

      const pullRequest = new PullRequest({ pulls });
      const loaded = await ctx.load(pullRequest);

      const c1s = loaded.collections['C1s'] as C1[];

      expect(c1s).toBeArray();
      // expect(c1s).not.toBeEmpty();
      // expect(7).toBe( c1s.length);
    });
  });

  describe('with unique (no -) predicate parameter on class', () => {
    it('should return matching objects', async () => {
      const { m, ctx } = fixture;

      const pulls = [
        new Pull({
          extent: new Extent({
            objectType: m.C1,
            predicate: new And({
              operands: [
                new Equals({
                  propertyType: m.C1.C1AllorsUnique,
                  parameter: 'p1',
                }),
              ],
            }),
          }),
          parameters: {
            p1: '8B3C497872D340BAB302114EB331FE04',
          },
        }),
      ];

      ctx.session.reset();

      const pullRequest = new PullRequest({ pulls });
      const loaded = await ctx.load(pullRequest);

      const c1s = loaded.collections['C1s'] as C1[];

      expect(c1s).toBeArray();
      // expect(c1s).not.toBeEmpty();
      // expect(7).toBe( c1s.length);
    });
  });

  describe('with unique predicate parameter on interface', () => {
    it('should return matching objects', async () => {
      const { m, ctx } = fixture;

      const pulls = [
        new Pull({
          extent: new Extent({
            objectType: m.C1,
            predicate: new And({
              operands: [
                new Equals({
                  propertyType: m.C1.I1AllorsUnique,
                  parameter: 'p1',
                }),
              ],
            }),
          }),
          parameters: {
            p1: '7F7BF8EF-DDF2-47E6-B33F-627BE7DEAD6D',
          },
        }),
      ];

      ctx.session.reset();

      const pullRequest = new PullRequest({ pulls });
      const loaded = await ctx.load(pullRequest);

      const c1s = loaded.collections['C1s'] as C1[];

      expect(c1s).toBeArray();
      // expect(c1s).not.toBeEmpty();
      // expect(7).toBe( c1s.length);
    });
  });
});
