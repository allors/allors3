import { IObject, IRule } from '@allors/workspace/domain/system';
import { Class, RoleType } from '@allors/system/workspace/meta';

export interface DerivationsRuleByClassExtension {
  ruleByClass?: Map<Class, IRule<IObject>>;
}

export function derivationRuleByClass(
  roleType: RoleType
): Map<Class, IRule<IObject>>;
export function derivationRuleByClass(
  roleType: RoleType,
  ruleByClass?: Map<Class, IRule<IObject>>
): void;
export function derivationRuleByClass(
  roleType: RoleType,
  ruleByClass?: Map<Class, IRule<IObject>>
): void | Map<Class, IRule<IObject>> {
  if (roleType == null) {
    return;
  }

  if (ruleByClass == null) {
    return (roleType._ as DerivationsRuleByClassExtension).ruleByClass;
  }

  (roleType._ as DerivationsRuleByClassExtension).ruleByClass = ruleByClass;
}
