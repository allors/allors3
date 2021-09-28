import { tags } from '@allors/workspace/meta/apps';
import { Organisation, PostalAddress } from '../generated';

export function displayAddress2(organisation: Organisation): string {
  if (organisation.GeneralCorrespondence && organisation.GeneralCorrespondence.strategy.cls.tag === tags.PostalAddress) {
    const postalAddress = organisation.GeneralCorrespondence as PostalAddress;
    return `${postalAddress.PostalCode} ${postalAddress.Locality}`;
  }

  return '';
}
