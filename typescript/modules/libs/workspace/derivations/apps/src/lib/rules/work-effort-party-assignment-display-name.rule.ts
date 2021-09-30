import { ICycle, IRule, IPattern } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { WorkEffortPartyAssignment } from '@allors/workspace/domain/default';

export class WorkEffortPartyAssignmentDisplayNameRule implements IRule {
  id: '32a48e168f47446599ec0030bec58084';
  patterns: IPattern[];

  constructor(m: M) {
    const { treeBuilder: t } = m;

    this.patterns = [
      {
        kind: 'RolePattern',
        roleType: m.WorkEffortPartyAssignment.Party,
      },
    ];
  }

  derive(cycle: ICycle, matches: WorkEffortPartyAssignment[]) {
    for (const match of matches) {
      match.DisplayName = match.Party.DisplayName ?? 'N/A';
    }
  }
}
