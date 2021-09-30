import { IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';

import { AutomatedAgentDisplayNameRule } from './rules/automated-agent-display-name.rule';
import { EmailAddressDisplayNameRule } from './rules/email-address-display-name.rule';
import { OrganisationDisplayNameRule } from './rules/organisation-display-name.rule';
import { PartCategoryDisplayNameRule } from './rules/part-category-display-name.rule';
import { PersonDisplayNameRule } from './rules/person-display-name.rule';
import { PostalAddressDisplayNameRule } from './rules/postal-address-display-name.rule';
import { ProductCategoryDisplayNameRule } from './rules/product-category-display-name.rule';
import { PurchaseOrderDisplayNameRule } from './rules/purchase-order-display-name.rule';

export function ruleBuilder(m: M): IRule[] {
  return [
    new AutomatedAgentDisplayNameRule(m),
    new EmailAddressDisplayNameRule(m),
    new OrganisationDisplayNameRule(m),
    new PartCategoryDisplayNameRule(m),
    new PersonDisplayNameRule(m),
    new PostalAddressDisplayNameRule(m),
    new ProductCategoryDisplayNameRule(m),
    new PurchaseOrderDisplayNameRule(m),
  ];
}
