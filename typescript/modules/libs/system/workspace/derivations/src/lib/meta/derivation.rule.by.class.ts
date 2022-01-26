import { IObject, IRule } from '@allors/system/workspace/domain';
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
  const extension = roleType?._ as DerivationsRuleByClassExtension;

  if (ruleByClass == null) {
    return extension?.ruleByClass;
  }

  extension.ruleByClass = ruleByClass;
}
