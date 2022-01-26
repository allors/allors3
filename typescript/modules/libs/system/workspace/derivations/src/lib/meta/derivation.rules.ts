import { IObject, IRule } from '@allors/system/workspace/domain';
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
  const extension = metaPopulation._ as DerivationRulesExtension;

  if (rules == null) {
    return extension.rules;
  }

  extension.rules = rules;
}
