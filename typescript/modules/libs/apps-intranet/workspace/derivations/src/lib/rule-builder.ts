import { IObject, IRule } from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';

import { EmailAddressDisplayNameRule } from './rules/email-address-display-name.rule';
import { InventoryItemFacilityNameRule } from './rules/inventory-item-facility-name.rule';
import { InventoryItemPartDisplayNameRule } from './rules/inventory-item-part-display-name.rule';
import { OrganisationDisplayAddressRule } from './rules/organisation-display-address.rule';
import { OrganisationDisplayAddress2Rule } from './rules/organisation-display-address2.rule';
import { OrganisationDisplayAddress3Rule } from './rules/organisation-display-address3.rule';
import { OrganisationDisplayClassificationRule } from './rules/organisation-display-classification.rule';
import { OrganisationDisplayNameRule } from './rules/organisation-display-name.rule';
import { PartyDisplayPhoneRule } from './rules/party-display-phone.rule';
import { PersonDisplayEmailRule } from './rules/person-display-email.rule';
import { PersonDisplayNameRule } from './rules/person-display-name.rule';
import { PostalAddressDisplayNameRule } from './rules/postal-address-display-name.rule';
import { PurchaseInvoiceItemTotalIncVatRule } from './rules/purchase-invoice-item-total-inc-vat.rule';
import { PurchaseInvoiceItemUnitVatRule } from './rules/purchase-invoice-item-unit-vat.rule';
import { PurchaseOrderItemTotalIncVatRule } from './rules/purchase-order-item-total-inc-vat.rule';
import { PurchaseOrderItemUnitVatRule } from './rules/purchase-order-item-unit-vat.rule';
import { SerialisedItemAgeRule } from './rules/serialised-item-age.rule';
import { SerialisedItemYearsToGoRule } from './rules/serialised-item-years-to-go.rule';
import { TelecommunicationsNumberDisplayNameRule } from './rules/telecommunications-number-display-name.ruley';
import { WebAddressDisplayNameRule } from './rules/web-address-display-name.rule';
import { WorkEffortInventoryAssignmentTotalSellingPriceRule } from './rules/work-effort-inventory-assignment-total-selling-price.rule';
import { MetaPopulation } from '@allors/system/workspace/meta';
export function ruleBuilder(metaPopulation: MetaPopulation): IRule<IObject>[] {
  const m = metaPopulation as M;
  return [
    new EmailAddressDisplayNameRule(m),
    new InventoryItemFacilityNameRule(m),
    new InventoryItemPartDisplayNameRule(m),
    new OrganisationDisplayAddressRule(m),
    new OrganisationDisplayAddress2Rule(m),
    new OrganisationDisplayAddress3Rule(m),
    new OrganisationDisplayClassificationRule(m),
    new OrganisationDisplayNameRule(m),
    new PartyDisplayPhoneRule(m),
    new PersonDisplayNameRule(m),
    new PersonDisplayEmailRule(m),
    new PostalAddressDisplayNameRule(m),
    new PurchaseInvoiceItemTotalIncVatRule(m),
    new PurchaseInvoiceItemUnitVatRule(m),
    new PurchaseOrderItemTotalIncVatRule(m),
    new PurchaseOrderItemUnitVatRule(m),
    new SerialisedItemAgeRule(m),
    new SerialisedItemYearsToGoRule(m),
    new TelecommunicationsNumberDisplayNameRule(m),
    new WebAddressDisplayNameRule(m),
    new WorkEffortInventoryAssignmentTotalSellingPriceRule(m),
  ];
}
