import { IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';

import { PartyDisplayNameRule } from './rules/party-display-name.rule';

export function ruleBuilder(m: M): IRule[] {
  return [new PartyDisplayNameRule(m)];
}
