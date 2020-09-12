import { MetaPopulation } from '@allors/meta/system';
import { Database } from '@allors/workspace/system';

import { data, PullFactory, Meta } from '@allors/meta/generated';
import { MemoryDatabase } from '@allors/workspace/memory';

describe('Filter',
    () => {
        let m: Meta;
        let factory: PullFactory;
        let database: Database;

        beforeEach(async () => {
            m = new MetaPopulation(data) as Meta;
            database = new MemoryDatabase(m);

            factory = new PullFactory(m);
        });

        describe('with empty flatPull',
            () => {
                it('should serialize to correct json', () => {

                    const original = factory.Organisation({});

                    const json = JSON.stringify(original);
                    const pull = JSON.parse(json);

                    expect(pull).toBeDefined();
                });
            });
    });
