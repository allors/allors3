import { IObject, IRule } from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import { PersonFullNameRule } from './rules/person-full-name.rule';

export function ruleBuilder(m: M): IRule<IObject>[] {
  return [new PersonFullNameRule(m)];
}
