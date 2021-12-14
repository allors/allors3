import { IObject, IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';

import { InventoryItemFacilityNameRule } from './rules/inventory-item-facility-name.rule';
import { InventoryItemPartDisplayNameRule } from './rules/inventory-item-part-display-name.rule';
import { OrganisationDisplayAddressRule } from './rules/organisation-display-address.rule';
import { OrganisationDisplayAddress2Rule } from './rules/organisation-display-address2.rule';
import { OrganisationDisplayAddress3Rule } from './rules/organisation-display-address3.rule';
import { OrganisationDisplayClassificationRule } from './rules/organisation-display-classification.rule';
import { PartyDisplayPhoneRule } from './rules/party-display-phone.rule';
import { PersonDisplayEmailRule } from './rules/person-display-email.rule';
import { SerialisedItemAgeRule } from './rules/serialised-item-age.rule';
import { SerialisedItemYearsToGoRule } from './rules/serialised-item-years-to-go.rule';
import { WorkEffortInventoryAssignmentTotalSellingPriceRule } from './rules/work-effort-inventory-assignment-total-selling-price.rule';
export function ruleBuilder(m: M): IRule<IObject>[] {
  return [
    new InventoryItemFacilityNameRule(m),
    new InventoryItemPartDisplayNameRule(m),
    new OrganisationDisplayAddressRule(m),
    new OrganisationDisplayAddress2Rule(m),
    new OrganisationDisplayAddress3Rule(m),
    new OrganisationDisplayClassificationRule(m),
    new PartyDisplayPhoneRule(m),
    new PersonDisplayEmailRule(m),
    new SerialisedItemAgeRule(m),
    new SerialisedItemYearsToGoRule(m),
    new WorkEffortInventoryAssignmentTotalSellingPriceRule(m),
  ];
}
