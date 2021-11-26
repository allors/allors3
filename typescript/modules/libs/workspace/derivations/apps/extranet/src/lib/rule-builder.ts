import { IObject, IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';

import { EmailAddressDisplayNameRule } from './rules/email-address-display-name.rule';
import { OrganisationDisplayAddressRule } from './rules/organisation-display-address.rule';
import { OrganisationDisplayAddress2Rule } from './rules/organisation-display-address2.rule';
import { OrganisationDisplayAddress3Rule } from './rules/organisation-display-address3.rule';
import { OrganisationDisplayNameRule } from './rules/organisation-display-name.rule';
import { PartCategoryDisplayNameRule } from './rules/part-category-display-name.rule';
import { PartyDisplayPhoneRule } from './rules/party-display-phone.rule';
import { PersonDisplayEmailRule } from './rules/person-display-email.rule';
import { PersonDisplayNameRule } from './rules/person-display-name.rule';
import { PostalAddressDisplayNameRule } from './rules/postal-address-display-name.rule';
import { SerialisedItemDisplayNameRule } from './rules/serialised-item-display-name.rule';
import { TelecommunicationsNumberDisplayNameRule } from './rules/telecommunications-number-display-name.rule';
import { WebAddressDisplayNameRule } from './rules/web-address-display-name.rule';

export function ruleBuilder(m: M): IRule<IObject>[] {
  return [
    new EmailAddressDisplayNameRule(m),
    new OrganisationDisplayAddressRule(m),
    new OrganisationDisplayAddress2Rule(m),
    new OrganisationDisplayAddress3Rule(m),
    new OrganisationDisplayNameRule(m),
    new PartCategoryDisplayNameRule(m),
    new PartyDisplayPhoneRule(m),
    new PersonDisplayEmailRule(m),
    new PersonDisplayNameRule(m),
    new PostalAddressDisplayNameRule(m),
    new SerialisedItemDisplayNameRule(m),
    new TelecommunicationsNumberDisplayNameRule(m),
    new WebAddressDisplayNameRule(m),
  ];
}
