import { ICycle, IRule, IPattern } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { Organisation, Party } from '@allors/workspace/domain/default';

export class OrganisationDisplayNameRule implements IRule {
  id: '7A62C83563AF4E989BB8BF24A9CB7CE7';
  patterns: IPattern[];

  constructor(m: M) {
    this.patterns = [
      {
        kind: 'RolePattern',
        roleType: m.Organisation.Name,
      },
    ];
  }

  derive(cycle: ICycle, matches: Organisation[]) {
    for (const match of matches) {
      match.DisplayName = or;
    }
  }
}
