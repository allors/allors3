import { tags } from '@allors/workspace/meta/default';
import { Party, TelecommunicationsNumber } from '../generated';
import { displayName } from './display-name';

export function displayPhone(party: Party): string {
  const telecommunicationsNumbers = party.PartyContactMechanisms.filter((v) => v.ContactMechanism?.strategy.cls.tag === tags.TelecommunicationsNumber);

  if (telecommunicationsNumbers.length > 0) {
    return telecommunicationsNumbers
      .map((v) => {
        const telecommunicationsNumber = v.ContactMechanism as TelecommunicationsNumber;
        return displayName(telecommunicationsNumber);
      })
      .reduce((acc: string, cur: string) => acc + ', ' + cur);
  }

  return '';
}
