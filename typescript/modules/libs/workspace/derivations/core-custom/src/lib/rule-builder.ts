import { IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';

import { OrganisationDisplayNameRule } from './rules/organisation-display-name.rule';
import { PersonDisplayNameRule } from './rules/person-display-name.rule';

export function ruleBuilder(m: M): IRule[] {
  return [new OrganisationDisplayNameRule(m), new PersonDisplayNameRule(m)];
}
