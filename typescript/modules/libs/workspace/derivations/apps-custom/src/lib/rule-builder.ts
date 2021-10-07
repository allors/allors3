import { IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { FullNameRule } from './rules/full-name.rule';

import { AutomatedAgentDisplayNameRule } from '@allors/workspace/derivations/apps';
import { EmailAddressDisplayNameRule } from '@allors/workspace/derivations/apps';
import { InventoryItemFacilityNameRule } from '@allors/workspace/derivations/apps';
import { OrganisationDisplayAddressRule } from '@allors/workspace/derivations/apps';
import { OrganisationDisplayAddress2Rule } from '@allors/workspace/derivations/apps';
import { OrganisationDisplayAddress3Rule } from '@allors/workspace/derivations/apps';
import { OrganisationDisplayClassificationRule } from '@allors/workspace/derivations/apps';
import { OrganisationDisplayNameRule } from '@allors/workspace/derivations/apps';
import { PartCategoryDisplayNameRule } from '@allors/workspace/derivations/apps';
import { PartyDisplayPhoneRule } from '@allors/workspace/derivations/apps';
import { PersonDisplayEmailRule } from '@allors/workspace/derivations/apps';
import { PersonDisplayNameRule } from '@allors/workspace/derivations/apps';
import { PostalAddressDisplayNameRule } from '@allors/workspace/derivations/apps';
import { ProductCategoryDisplayNameRule } from '@allors/workspace/derivations/apps';
import { PurchaseOrderDisplayNameRule } from '@allors/workspace/derivations/apps';
import { PurchaseOrderItemDisplayNameRule } from '@allors/workspace/derivations/apps';
import { SerialisedItemAgeRule } from '@allors/workspace/derivations/apps';
import { SerialisedItemCharacteristicTypeDisplayNameRule } from '@allors/workspace/derivations/apps';
import { SerialisedItemDisplayNameRule } from '@allors/workspace/derivations/apps';
import { SerialisedItemYearsToGoRule } from '@allors/workspace/derivations/apps';
import { TelecommunicationsNumberDisplayNameRule } from '@allors/workspace/derivations/apps';
import { UnifiedGoodDisplayNameRule } from '@allors/workspace/derivations/apps';
import { WebAddressDisplayNameRule } from '@allors/workspace/derivations/apps';
import { WorkEffortInventoryAssignmentTotalSellingPriceRule } from '@allors/workspace/derivations/apps';
import { WorkEffortPartyAssignmentDisplayNameRule } from '@allors/workspace/derivations/apps';

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
