import { IObject, IRule } from '@allors/workspace/domain/system';
import { MetaPopulation } from '@allors/system/workspace/meta';

interface DerivationRulesExtension {
  rules?: IRule<IObject>[];
}

export function derivationRules(
  metaPopulation: MetaPopulation
): IRule<IObject>[];
export function derivationRules(
  metaPopulation: MetaPopulation,
  rules: IRule<IObject>[]
): void;
export function derivationRules(
  metaPopulation: MetaPopulation,
  rules?: IRule<IObject>[]
): void | IRule<IObject>[] {
  if (metaPopulation == null) {
    return;
  }

  if (rules == null) {
    return (metaPopulation._ as DerivationRulesExtension).rules;
  }

  (metaPopulation._ as DerivationRulesExtension).rules = rules;
}
