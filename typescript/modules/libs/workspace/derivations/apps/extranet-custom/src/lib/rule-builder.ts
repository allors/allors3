import { IObject, IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';

import { OrganisationDisplayNameRule } from '@allors/workspace-derivations-apps-extranet';
import { PartCategoryDisplayNameRule } from '@allors/workspace-derivations-apps-extranet';
import { PartyDisplayPhoneRule } from '@allors/workspace-derivations-apps-extranet';
import { PersonDisplayEmailRule } from '@allors/workspace-derivations-apps-extranet';
import { PersonDisplayNameRule } from '@allors/workspace-derivations-apps-extranet';
import { SerialisedItemDisplayNameRule } from '@allors/workspace-derivations-apps-extranet';

export function ruleBuilder(m: M): IRule<IObject>[] {
  return [new OrganisationDisplayNameRule(m), new PartCategoryDisplayNameRule(m), new PartyDisplayPhoneRule(m), new PersonDisplayEmailRule(m), new PersonDisplayNameRule(m), new SerialisedItemDisplayNameRule(m)];
}
