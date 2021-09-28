import { IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';

import { FullNameRule } from './rules/full-name.rule';

export function ruleBuilder(m: M): IRule[] {
  return [new FullNameRule(m)];
}
