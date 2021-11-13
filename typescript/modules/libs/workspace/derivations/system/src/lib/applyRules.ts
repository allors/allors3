import { IObject, IRule } from '@allors/workspace/domain/system';
import { MetaPopulation } from '@allors/workspace/meta/system';

export function applyRules(metaPopulation: MetaPopulation, rules: IRule<IObject>[]) {

  Object.freeze(rules);
    metaPopulation._.rules = rules;

  for (const rule of rules) {
    Object.freeze(rule);

    const roleType = rule.roleType;

    if(roleType.associationType.objectType.isClass){
      roleType._.rule = rule;
    } else{
      let ruleByClass = roleType._.ruleByClass;
      if(ruleByClass == null){
        ruleByClass = new Map();
        roleType._.ruleByClass = ruleByClass;
      }

      const objectType = rule.objectType
      for(const cls of objectType.classes){
        ruleByClass.set(cls, rule);
      }
    }
  }
}
