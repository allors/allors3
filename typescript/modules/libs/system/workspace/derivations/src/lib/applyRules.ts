import { IObject, IRule } from '@allors/workspace/domain/system';
import { MetaPopulation } from '@allors/system/workspace/meta';
import { derivationRule } from './meta/derivation.rule';
import { derivationRuleByClass } from './meta/derivation.rule.by.class';
import { derivationRules } from './meta/derivation.rules';

export function applyRules(
  metaPopulation: MetaPopulation,
  rules: IRule<IObject>[]
) {
  Object.freeze(rules);
  derivationRules(metaPopulation, rules);

  for (const rule of rules) {
    Object.freeze(rule);

    const roleType = rule.roleType;

    if (roleType.associationType.objectType.isClass) {
      derivationRule(roleType, rule);
    } else {
      let ruleByClass = derivationRuleByClass(roleType);
      if (ruleByClass == null) {
        ruleByClass = new Map();
        derivationRuleByClass(roleType, ruleByClass);
      }

      const objectType = rule.objectType;
      for (const cls of objectType.classes) {
        ruleByClass.set(cls, rule);
      }
    }
  }
}
