import { Dependency, RoleType } from '@allors/workspace/meta/system';
import { IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { TelecommunicationsNumber, Party } from '@allors/workspace/domain/default';

export class PartyDisplayPhoneRule implements IRule<Party> {
  roleType: RoleType;
  dependencies: Dependency[];

  m: M;

  constructor(m: M) {
    this.m = m;
    const { dependency: d } = m;

    this.roleType = m.Party.DisplayName;

    this.dependencies = [d(m.Party, (v) => v.PartyContactMechanisms), d(m.PartyContactMechanism, (v) => v.ContactMechanism)];
  }

  derive(match: Party) {
    const telecommunicationsNumbers = match.PartyContactMechanisms.filter((v) => v.ContactMechanism?.strategy.cls === this.m.TelecommunicationsNumber);

    if (telecommunicationsNumbers.length > 0) {
      match.DisplayPhone = telecommunicationsNumbers
        .map((v) => {
          const telecommunicationsNumber = v.ContactMechanism as TelecommunicationsNumber;
          return telecommunicationsNumber.DisplayName;
        })
        .reduce((acc: string, cur: string) => acc + ', ' + cur, '');
    }

    match.DisplayPhone = '';
  }
}
