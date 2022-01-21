import { IObject, IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';

import { OrganisationDisplayAddressRule } from './rules/organisation-display-address.rule';
import { OrganisationDisplayAddress2Rule } from './rules/organisation-display-address2.rule';
import { OrganisationDisplayAddress3Rule } from './rules/organisation-display-address3.rule';
import { PartyDisplayPhoneRule } from './rules/party-display-phone.rule';
import { PersonDisplayEmailRule } from './rules/person-display-email.rule';

export function ruleBuilder(m: M): IRule<IObject>[] {
  return [new OrganisationDisplayAddressRule(m), new OrganisationDisplayAddress2Rule(m), new OrganisationDisplayAddress3Rule(m), new PartyDisplayPhoneRule(m), new PersonDisplayEmailRule(m)];
}
