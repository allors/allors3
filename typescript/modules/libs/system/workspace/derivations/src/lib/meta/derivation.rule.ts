import { IObject, IRule } from '@allors/workspace/domain/system';
import { RoleType } from '@allors/system/workspace/meta';

export interface DerivationsRuleExtension {
  rule?: IRule<IObject>;
}

export function derivationRule(roleType: RoleType): IRule<IObject>;
export function derivationRule(roleType: RoleType, rule: IRule<IObject>): void;
export function derivationRule(
  roleType: RoleType,
  rule?: IRule<IObject>
): void | IRule<IObject> {
  if (roleType == null) {
    return;
  }

  if (rule == null) {
    return (roleType._ as DerivationsRuleExtension).rule;
  }

  (roleType._ as DerivationsRuleExtension).rule = rule;
}
