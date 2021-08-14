import { IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/core';
import { FullNameRule } from './FullNameRule';

export function ruleBuilder(m: M): IRule[] {
  return [new FullNameRule(m)];
}
