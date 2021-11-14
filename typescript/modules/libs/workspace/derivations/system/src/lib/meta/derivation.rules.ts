import { IObject, IRule } from '@allors/workspace/domain/system';
import { MetaPopulation } from '@allors/workspace/meta/system';

interface DerivationRulesExtension {
  rules?: IRule<IObject>[];
}

export function derivationRules(meta: MetaPopulation, rules?: IRule<IObject>[]) {
  if (rules == null) {
    return (meta._ as DerivationRulesExtension).rules;
  }

  (meta._ as DerivationRulesExtension).rules = rules;
}
