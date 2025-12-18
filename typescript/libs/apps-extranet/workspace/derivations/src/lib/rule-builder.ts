import { IObject, IRule } from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';

import { OrganisationDisplayAddressRule } from './rules/organisation-display-address.rule';
import { OrganisationDisplayAddress2Rule } from './rules/organisation-display-address2.rule';
import { OrganisationDisplayAddress3Rule } from './rules/organisation-display-address3.rule';
import { PartyDisplayPhoneRule } from './rules/party-display-phone.rule';
import { PersonDisplayEmailRule } from './rules/person-display-email.rule';
import { MetaPopulation } from '@allors/system/workspace/meta';

export function ruleBuilder(metaPopulation: MetaPopulation): IRule<IObject>[] {
  const m = metaPopulation as M;
  return [
    new OrganisationDisplayAddressRule(m),
    new OrganisationDisplayAddress2Rule(m),
    new OrganisationDisplayAddress3Rule(m),
    new PartyDisplayPhoneRule(m),
    new PersonDisplayEmailRule(m),
  ];
}
