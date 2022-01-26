import { IObject, IRule } from '@allors/system/workspace/domain';
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
  const extension = roleType?._ as DerivationsRuleExtension;

  if (rule == null) {
    return extension?.rule;
  }

  extension.rule = rule;
}
