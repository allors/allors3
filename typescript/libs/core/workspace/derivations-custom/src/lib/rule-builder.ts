import { IObject, IRule } from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import { OrganisationDisplayNameRule } from './rules/organisation-display-name.rule';
import { PersonDisplayNameRule } from './rules/person-display-name.rule';
import { MetaPopulation } from '@allors/system/workspace/meta';

export function ruleBuilder(metaPopulation: MetaPopulation): IRule<IObject>[] {
  const m = metaPopulation as M;
  return [new OrganisationDisplayNameRule(m), new PersonDisplayNameRule(m)];
}
