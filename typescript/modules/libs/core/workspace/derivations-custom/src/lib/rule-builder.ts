import { IObject, IRule } from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import { OrganisationDisplayNameRule } from './rules/organisation-display-name.rule';
import { PersonDisplayNameRule } from './rules/person-display-name.rule';

export function ruleBuilder(m: M): IRule<IObject>[] {
  return [new OrganisationDisplayNameRule(m), new PersonDisplayNameRule(m)];
}
