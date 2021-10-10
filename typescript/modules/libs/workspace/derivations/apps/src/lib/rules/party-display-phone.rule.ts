import { ICycle, IRule, IPattern, pattern as p } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { TelecommunicationsNumber, Party } from '@allors/workspace/domain/default';
import { Dependency } from '@allors/workspace/meta/system';

export class PartyDisplayPhoneRule implements IRule {
  patterns: IPattern[];
  dependencies: Dependency[];

  m: M;

  constructor(m: M) {
    this.m = m;
    const { treeBuilder: t, dependency: d } = m;

    this.patterns = [
      p(m.Party, (v) => v.PartyContactMechanisms),
      p(
        m.PartyContactMechanism,
        (v) => v.ContactMechanism,
        t.ContactMechanism({
          PartyContactMechanismsWhereContactMechanism: {
            PartyWherePartyContactMechanism: {},
          },
        })
      ),
    ];

    this.dependencies = [d(m.Party, (v) => v.PartyContactMechanisms), d(m.PartyContactMechanism, (v) => v.ContactMechanism)];
  }

  derive(cycle: ICycle, matches: Party[]) {
    for (const match of matches) {
      const clsName = match.strategy.cls.singularName;

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
