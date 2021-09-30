import { ICycle, IRule, IPattern } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { AutomatedAgent } from '@allors/workspace/domain/default';

export class AutomatedAgentDisplayNameRule implements IRule {
  id: '798e246e73024644a3bb21a6aec48fdb';
  patterns: IPattern[];

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
