import { Composite, Dependency, RoleType } from '@allors/system/workspace/meta';
import { IRule } from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  TelecommunicationsNumber,
  Party,
} from '@allors/default/workspace/domain';

export class PartyDisplayPhoneRule implements IRule<Party> {
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  m: M;

  constructor(m: M) {
    this.m = m;
    const { dependency: d } = m;

    this.objectType = m.Party;
    this.roleType = m.Party.DisplayPhone;

    this.dependencies = [
      d(m.Party, (v) => v.CurrentPartyContactMechanisms),
      d(m.PartyContactMechanism, (v) => v.ContactMechanism),
    ];
  }

  derive(party: Party) {
    const telecommunicationsNumbers =
      party.CurrentPartyContactMechanisms.filter(
        (v) =>
          v.ContactMechanism?.strategy.cls === this.m.TelecommunicationsNumber
      );

    if (telecommunicationsNumbers.length > 0) {
      return telecommunicationsNumbers
        .map((v) => {
          const telecommunicationsNumber =
            v.ContactMechanism as TelecommunicationsNumber;
          return telecommunicationsNumber.DisplayName;
        })
        .reduce((acc: string, cur: string) => acc + ', ' + cur, '');
    }

    return '';
  }
}
