import { IObject, IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';

import { AutomatedAgentDisplayNameRule } from './rules/automated-agent-display-name.rule';
import { EmailAddressDisplayNameRule } from './rules/email-address-display-name.rule';
import { InventoryItemFacilityNameRule } from './rules/inventory-item-facility-name.rule';
import { InventoryItemPartDisplayNameRule } from './rules/inventory-item-part-display-name.rule';
import { OrganisationDisplayAddressRule } from './rules/organisation-display-address.rule';
import { OrganisationDisplayAddress2Rule } from './rules/organisation-display-address2.rule';
import { OrganisationDisplayAddress3Rule } from './rules/organisation-display-address3.rule';
import { OrganisationDisplayClassificationRule } from './rules/organisation-display-classification.rule';
import { OrganisationDisplayNameRule } from './rules/organisation-display-name.rule';
import { PartCategoryDisplayNameRule } from './rules/part-category-display-name.rule';
import { PartyDisplayPhoneRule } from './rules/party-display-phone.rule';
import { PersonDisplayEmailRule } from './rules/person-display-email.rule';
import { PersonDisplayNameRule } from './rules/person-display-name.rule';
import { PostalAddressDisplayNameRule } from './rules/postal-address-display-name.rule';
import { ProductCategoryDisplayNameRule } from './rules/product-category-display-name.rule';
import { PurchaseOrderDisplayNameRule } from './rules/purchase-order-display-name.rule';
import { PurchaseOrderItemDisplayNameRule } from './rules/purchase-order-item-display-name.rule';
import { SerialisedItemAgeRule } from './rules/serialised-item-age.rule';
import { SerialisedItemCharacteristicTypeDisplayNameRule } from './rules/serialised-item-characteristic-type-display-name.rule';
import { SerialisedItemDisplayNameRule } from './rules/serialised-item-display-name.rule';
import { SerialisedItemYearsToGoRule } from './rules/serialised-item-years-to-go.rule';
import { TelecommunicationsNumberDisplayNameRule } from './rules/telecommunications-number-display-name.rule';
import { UnifiedGoodDisplayNameRule } from './rules/unified-good-display-name.rule';
import { WebAddressDisplayNameRule } from './rules/web-address-display-name.rule';
import { WorkEffortInventoryAssignmentTotalSellingPriceRule } from './rules/work-effort-inventory-assignment-total-selling-price.rule';
import { WorkEffortPartyAssignmentDisplayNameRule } from './rules/work-effort-party-assignment-display-name.rule';

export function ruleBuilder(m: M): IRule<IObject>[] {
  return [
    new AutomatedAgentDisplayNameRule(m),
    new EmailAddressDisplayNameRule(m),
    new InventoryItemFacilityNameRule(m),
    new InventoryItemPartDisplayNameRule(m),
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
  ];
}
