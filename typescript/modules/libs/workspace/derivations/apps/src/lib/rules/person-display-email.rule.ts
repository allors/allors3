import { ICycle, IRule, IPattern, pattern as p } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { EmailAddress, Person } from '@allors/workspace/domain/default';
import { Dependency } from '@allors/workspace/meta/system';

export class PersonDisplayEmailRule implements IRule {
  patterns: IPattern[];
  dependencies: Dependency[];

  m: M;

  constructor(m: M) {
    const { treeBuilder: t, dependency: d } = m;
    this.m = m;

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

    this.dependencies = [d(m.Person, (v) => v.PartyContactMechanisms)];
  }

  derive(cycle: ICycle, matches: Person[]) {
    for (const match of matches) {
      const emailAddresses = match.PartyContactMechanisms.filter((v) => v.ContactMechanism?.strategy.cls === this.m.EmailAddress)
        .map((v) => {
          const emailAddress = v.ContactMechanism as EmailAddress;
          return emailAddress.ElectronicAddressString;
        })
        .filter((v) => v) as string[];

      match.DisplayEmail = emailAddresses.join(', ');
    }
  }
}
