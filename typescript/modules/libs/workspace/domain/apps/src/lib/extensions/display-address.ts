import { tags } from '@allors/workspace/meta/default';
import { Organisation, PostalAddress } from '../generated';

export function displayAddress(organisation: Organisation): string {
  if (organisation.GeneralCorrespondence && organisation.GeneralCorrespondence.strategy.cls.tag === tags.PostalAddress) {
    const postalAddress = organisation.GeneralCorrespondence as PostalAddress;
    return `${postalAddress.Address1 ? postalAddress.Address1 : ''} ${postalAddress.Address2 ? postalAddress.Address2 : ''} ${postalAddress.Address3 ? postalAddress.Address3 : ''}`;
  }

  return '';
}
