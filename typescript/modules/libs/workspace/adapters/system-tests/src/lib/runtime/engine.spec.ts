import { MetaPopulation } from '@allors/workspace/meta/system';
import { LazyMetaPopulation } from '@allors/workspace/meta/json/system';
import { data } from '@allors/workspace/meta/json/default';
import { IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { ruleBuilder } from '@allors/workspace/derivations/core-custom';
import { Engine } from '@allors/workspace/adapters/system';

let metaPopulation: MetaPopulation;
let m: M;
let rules: IRule[];
let engine: Engine;

beforeEach(async () => {
  metaPopulation = new LazyMetaPopulation(data);
  m = metaPopulation as M;
  rules = ruleBuilder(m);
  engine = new Engine(rules);
});

test('engineCreate', async () => {
  expect(engine).toBeDefined();
});
