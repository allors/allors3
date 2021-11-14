import { IObject, IRule } from '@allors/workspace/domain/system';
import { Class, RoleType } from '@allors/workspace/meta/system';

export interface DerivationsRuleByClassExtension {
  rule?: IRule<IObject>;
  ruleByClass?: Map<Class, IRule<IObject>>;
}

export function derivationRuleByClass(meta: RoleType, ruleByClass?: Map<Class, IRule<IObject>>) {
  if (ruleByClass == null) {
    return (meta._ as DerivationsRuleByClassExtension).ruleByClass;
  }

  (meta._ as DerivationsRuleByClassExtension).ruleByClass = ruleByClass;
}
