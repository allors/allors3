import { ICycle, IRule, IPattern } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { TelecommunicationsNumber, Party } from '@allors/workspace/domain/default';

export class PartyDisplayPhoneRule implements IRule {
  id = 'f27a9a776f2b4119b623e4c518f07f46';
  patterns: IPattern[];
  m: M;

  constructor(m: M) {
    this.m = m;
    const { treeBuilder: t } = m;

    this.patterns = [
      {
        kind: 'RolePattern',
        roleType: m.Party.PartyContactMechanisms,
      },
      {
        kind: 'RolePattern',
        roleType: m.PartyContactMechanism.ContactMechanism,
        tree: t.ContactMechanism({
          PartyContactMechanismsWhereContactMechanism: {
            PartyWherePartyContactMechanism: {},
          },
        }),
      },
    ];
  }

  derive(cycle: ICycle, matches: Party[]) {
    for (const match of matches) {
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
}
