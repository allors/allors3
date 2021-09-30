import { IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { FullNameRule } from './rules/full-name.rule';

import { AutomatedAgentDisplayNameRule } from '../../../apps/src/lib/rules/automated-agent-display-name.rule';
import { EmailAddressDisplayNameRule } from '../../../apps/src/lib/rules/email-address-display-name.rule';
import { InventoryItemFacilityNameRule } from '../../../apps/src/lib/rules/inventory-item-facility-name.rule';
import { OrganisationDisplayAddressRule } from '../../../apps/src/lib/rules/organisation-display-address.rule';
import { OrganisationDisplayAddress2Rule } from '../../../apps/src/lib/rules/organisation-display-address2.rule';
import { OrganisationDisplayAddress3Rule } from '../../../apps/src/lib/rules/organisation-display-address3.rule';
import { OrganisationDisplayClassificationRule } from '../../../apps/src/lib/rules/organisation-display-classification.rule';
import { OrganisationDisplayNameRule } from '../../../apps/src/lib/rules/organisation-display-name.rule';
import { PartCategoryDisplayNameRule } from '../../../apps/src/lib/rules/part-category-display-name.rule';
import { PartyDisplayPhoneRule } from '../../../apps/src/lib/rules/party-display-phone.rule';
import { PersonDisplayEmailRule } from '../../../apps/src/lib/rules/person-display-email.rule';
import { PersonDisplayNameRule } from '../../../apps/src/lib/rules/person-display-name.rule';
import { PostalAddressDisplayNameRule } from '../../../apps/src/lib/rules/postal-address-display-name.rule';
import { ProductCategoryDisplayNameRule } from '../../../apps/src/lib/rules/product-category-display-name.rule';
import { PurchaseOrderDisplayNameRule } from '../../../apps/src/lib/rules/purchase-order-display-name.rule';
import { PurchaseOrderItemDisplayNameRule } from '../../../apps/src/lib/rules/purchase-order-item-display-name.rule';
import { SerialisedItemAgeRule } from '../../../apps/src/lib/rules/serialised-item-age.rule';
import { SerialisedItemCharacteristicTypeDisplayNameRule } from '../../../apps/src/lib/rules/serialised-item-characteristic-type-display-name.rule';
import { SerialisedItemDisplayNameRule } from '../../../apps/src/lib/rules/serialised-item-display-name.rule';
import { SerialisedItemYearsToGoRule } from '../../../apps/src/lib/rules/serialised-item-years-to-go.rule';
import { TelecommunicationsNumberDisplayNameRule } from '../../../apps/src/lib/rules/telecommunications-number-display-name.rule';
import { UnifiedGoodDisplayNameRule } from '../../../apps/src/lib/rules/unified-good-display-name.rule';
import { WebAddressDisplayNameRule } from '../../../apps/src/lib/rules/web-address-display-name.rule';
import { WorkEffortInventoryAssignmentTotalSellingPriceRule } from '../../../apps/src/lib/rules/work-effort-inventory-assignment-total-selling-price.rule';
import { WorkEffortPartyAssignmentDisplayNameRule } from '../../../apps/src/lib/rules/work-effort-party-assignment-display-name.rule';

export function ruleBuilder(m: M): IRule[] {
  return [
    new AutomatedAgentDisplayNameRule(m),
    new EmailAddressDisplayNameRule(m),
    new InventoryItemFacilityNameRule(m),
    new OrganisationDisplayAddressRule(m),
    new OrganisationDisplayAddress2Rule(m),
    new OrganisationDisplayAddress3Rule(m),
    new OrganisationDisplayClassificationRule(m),
    new OrganisationDisplayNameRule(m),
    new PartCategoryDisplayNameRule(m),
    new PartyDisplayPhoneRule(m),
    new PersonDisplayEmailRule(m),
    new PersonDisplayNameRule(m),
    new PostalAddressDisplayNameRule(m),
    new ProductCategoryDisplayNameRule(m),
    new PurchaseOrderDisplayNameRule(m),
    new PurchaseOrderItemDisplayNameRule(m),
    new SerialisedItemAgeRule(m),
    new SerialisedItemCharacteristicTypeDisplayNameRule(m),
    new SerialisedItemDisplayNameRule(m),
    new SerialisedItemYearsToGoRule(m),
    new TelecommunicationsNumberDisplayNameRule(m),
    new UnifiedGoodDisplayNameRule(m),
    new WebAddressDisplayNameRule(m),
    new WorkEffortInventoryAssignmentTotalSellingPriceRule(m),
    new WorkEffortPartyAssignmentDisplayNameRule(m),

    new FullNameRule(m),
  ];
}
