import { IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { FullNameRule } from './rules/full-name.rule';

import { AutomatedAgentDisplayNameRule } from '../../../apps/src/lib/rules/automated-agent-display-name.rule';
import { EmailAddressDisplayNameRule } from '../../../apps/src/lib/rules/email-address-display-name.rule';
import { OrganisationDisplayNameRule } from '../../../apps/src/lib/rules/organisation-display-name.rule';
import { PartCategoryDisplayNameRule } from '../../../apps/src/lib/rules/part-category-display-name.rule';
import { PersonDisplayNameRule } from '../../../apps/src/lib/rules/person-display-name.rule';
import { PostalAddressDisplayNameRule } from '../../../apps/src/lib/rules/postal-address-display-name.rule';
import { ProductCategoryDisplayNameRule } from '../../../apps/src/lib/rules/product-category-display-name.rule';
import { PurchaseOrderDisplayNameRule } from '../../../apps/src/lib/rules/purchase-order-display-name.rule';

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

    new FullNameRule(m),
  ];
}
