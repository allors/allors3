import { ICycle, IRule, IPattern } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { AutomatedAgent } from '@allors/workspace/domain/default';
import { Dependency } from '@allors/workspace/meta/system';

export class AutomatedAgentDisplayNameRule implements IRule {
  patterns: IPattern[];
  dependencies: Dependency[];

  constructor(m: M) {
    this.patterns = [
      {
        kind: 'RolePattern',
        roleType: m.AutomatedAgent.UserName,
      },
    ];
  }

  derive(cycle: ICycle, matches: AutomatedAgent[]) {
    for (const match of matches) {
      match.DisplayName = match.UserName ?? 'N/A';
    }
  }
}
