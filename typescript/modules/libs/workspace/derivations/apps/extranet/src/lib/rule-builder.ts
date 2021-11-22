import { IObject, IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';

import { OrganisationDisplayNameRule } from './rules/organisation-display-name.rule';
import { PartCategoryDisplayNameRule } from './rules/part-category-display-name.rule';
import { PartyDisplayPhoneRule } from './rules/party-display-phone.rule';
import { PersonDisplayEmailRule } from './rules/person-display-email.rule';
import { PersonDisplayNameRule } from './rules/person-display-name.rule';
import { SerialisedItemDisplayNameRule } from './rules/serialised-item-display-name.rule';

export function ruleBuilder(m: M): IRule<IObject>[] {
  return [new OrganisationDisplayNameRule(m), new PartCategoryDisplayNameRule(m), new PartyDisplayPhoneRule(m), new PersonDisplayEmailRule(m), new PersonDisplayNameRule(m), new SerialisedItemDisplayNameRule(m)];
}
