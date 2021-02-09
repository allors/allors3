import { Pull, Result, Fetch, Tree, Extent, Node } from '@allors/data/core';
import { PullRequest } from '@allors/protocol/core';
import { Person } from '@allors/domain/generated';

import { Fixture } from '../Fixture';

import 'jest-extended';

describe('Instantiate', () => {
  let fixture: Fixture;

  let people: Person[] = [];

  beforeEach(async () => {
    fixture = new Fixture();
    await fixture.init();

    const { m, ctx } = fixture;

    const pulls = [
      new Pull({
        extent: new Extent({
          objectType: m.Person,
        }),
      }),
      new Pull({
        extent: new Extent({
          objectType: m.Organisation,
        }),
      }),
    ];

    ctx.session.reset();

    try {
      const loaded = await ctx.load(new PullRequest({ pulls }));

      people = loaded.collections['People'] as Person[];
    } catch (e) {
      console.log(e);
    }
  });

  describe('Person', () => {
    it('should return person', async () => {
      const { ctx } = fixture;

      const object = people[0].id;

      const pulls = [
        new Pull({
          object: object,
        }),
      ];

      ctx.session.reset();

      const loaded = await ctx.load(new PullRequest({ pulls }));

      const person = loaded.objects['Person'] as Person;

      expect(person).not.toBeNull();
      expect(object).toBe(person.id);
    });
  });

  describe('People with include tree', () => {
    it('should return all people', async () => {
      const { m, ctx } = fixture;

      const pulls = [
        new Pull({
          extent: new Extent({
            objectType: m.Person,
          }),
          results: [
            new Result({
              fetch: new Fetch({
                include: new Tree({
                  objectType: m.Person,
                  nodes: [
                    new Node({
                      propertyType: m.Person.CycleOne,
                    }),
                  ],
                }),
              }),
            }),
          ],
        }),
      ];

      ctx.session.reset();

      const loaded = await ctx.load(new PullRequest({ pulls }));

      people = loaded.collections['People'] as Person[];

      expect([]).toBeArray();

      expect(people).toBeArray();
      expect(people).not.toBeEmpty();

      people.forEach(() => {});
    });
  });
});
