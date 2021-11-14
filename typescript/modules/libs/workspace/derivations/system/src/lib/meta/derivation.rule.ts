import { IObject, IRule } from '@allors/workspace/domain/system';
import { Class, RoleType } from '@allors/workspace/meta/system';

export interface DerivationsRuleExtension {
  rule?: IRule<IObject>;
  ruleByClass?: Map<Class, IRule<IObject>>;
}

export function derivationRule(meta: RoleType, rule?: IRule<IObject>) {
  if (rule == null) {
    return (meta._ as DerivationsRuleExtension).rule;
  }

  (meta._ as DerivationsRuleExtension).rule = rule;
}
