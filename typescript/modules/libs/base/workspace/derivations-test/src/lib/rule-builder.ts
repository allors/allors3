import { IObject, IRule } from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import { PersonFullNameRule } from './rules/person-full-name.rule';
import { MetaPopulation } from '@allors/system/workspace/meta';

export function ruleBuilder(metaPopulation: MetaPopulation): IRule<IObject>[] {
  const m = metaPopulation as M;
  return [new PersonFullNameRule(m)];
}
