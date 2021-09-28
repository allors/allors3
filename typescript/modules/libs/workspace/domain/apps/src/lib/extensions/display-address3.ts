import { tags } from '@allors/workspace/meta/default';
import { Organisation, PostalAddress } from '../generated';

export function displayAddress3(organisation: Organisation): string {
  if (organisation.GeneralCorrespondence && organisation.GeneralCorrespondence.strategy.cls.tag === tags.PostalAddress) {
    const postalAddress = organisation.GeneralCorrespondence as PostalAddress;
    return `${postalAddress.Country ? postalAddress.Country.Name : ''}`;
  }

  return '';
}
