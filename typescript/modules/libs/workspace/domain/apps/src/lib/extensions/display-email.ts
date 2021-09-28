import { tags } from '@allors/workspace/meta/default';
import { Person, EmailAddress } from '../generated';

export function displayEmail(person: Person): string {
  const emailAddresses = person.PartyContactMechanisms.filter((v) => v.ContactMechanism?.strategy.cls.tag === tags.EmailAddress)
    .map((v) => {
      const emailAddress = v.ContactMechanism as EmailAddress;
      return emailAddress.ElectronicAddressString;
    })
    .filter((v) => v) as string[];

  return emailAddresses.join(', ');
}
