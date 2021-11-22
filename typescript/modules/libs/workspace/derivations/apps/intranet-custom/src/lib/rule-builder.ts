import { IObject, IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';

import { AutomatedAgentDisplayNameRule } from '@allors/workspace-derivations-apps-intranet';
import { EmailAddressDisplayNameRule } from '@allors/workspace-derivations-apps-intranet';
import { InventoryItemFacilityNameRule } from '@allors/workspace-derivations-apps-intranet';
import { OrganisationDisplayAddressRule } from '@allors/workspace-derivations-apps-intranet';
import { OrganisationDisplayAddress2Rule } from '@allors/workspace-derivations-apps-intranet';
import { OrganisationDisplayAddress3Rule } from '@allors/workspace-derivations-apps-intranet';
import { OrganisationDisplayClassificationRule } from '@allors/workspace-derivations-apps-intranet';
import { OrganisationDisplayNameRule } from '@allors/workspace-derivations-apps-intranet';
import { PartCategoryDisplayNameRule } from '@allors/workspace-derivations-apps-intranet';
import { PartyDisplayPhoneRule } from '@allors/workspace-derivations-apps-intranet';
import { PersonDisplayEmailRule } from '@allors/workspace-derivations-apps-intranet';
import { PersonDisplayNameRule } from '@allors/workspace-derivations-apps-intranet';
import { PostalAddressDisplayNameRule } from '@allors/workspace-derivations-apps-intranet';
import { ProductCategoryDisplayNameRule } from '@allors/workspace-derivations-apps-intranet';
import { PurchaseOrderDisplayNameRule } from '@allors/workspace-derivations-apps-intranet';
import { PurchaseOrderItemDisplayNameRule } from '@allors/workspace-derivations-apps-intranet';
import { SerialisedItemAgeRule } from '@allors/workspace-derivations-apps-intranet';
import { SerialisedItemCharacteristicTypeDisplayNameRule } from '@allors/workspace-derivations-apps-intranet';
import { SerialisedItemDisplayNameRule } from '@allors/workspace-derivations-apps-intranet';
import { SerialisedItemYearsToGoRule } from '@allors/workspace-derivations-apps-intranet';
import { TelecommunicationsNumberDisplayNameRule } from '@allors/workspace-derivations-apps-intranet';
import { UnifiedGoodDisplayNameRule } from '@allors/workspace-derivations-apps-intranet';
import { WebAddressDisplayNameRule } from '@allors/workspace-derivations-apps-intranet';
import { WorkEffortInventoryAssignmentTotalSellingPriceRule } from '@allors/workspace-derivations-apps-intranet';
import { WorkEffortPartyAssignmentDisplayNameRule } from '@allors/workspace-derivations-apps-intranet';

export function ruleBuilder(m: M): IRule<IObject>[] {
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
  ];
}
